/*
This file is part of the OdinMS Maple Story Server
Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
Matthias Butz <matze@odinms.de>
Jan Christian Meyer <vimes@odinms.de>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation version 3 as published by
the Free Software Foundation. You may not use, modify or distribute
this program under any other version of the GNU Affero General Public
License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using client.inventory;
using constants.game;
using constants.id;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using Microsoft.ClearScript.V8;
using Microsoft.EntityFrameworkCore;
using net;
using net.netty;
using net.packet;
using net.packet.logging;
using net.server;
using net.server.channel;
using net.server.coordinator.login;
using net.server.coordinator.session;
using net.server.guild;
using net.server.world;
using scripting;
using scripting.Event;
using scripting.npc;
using scripting.quest;
using server;
using server.life;
using server.maps;
using System.Net;
using System.Text.RegularExpressions;
using tools;
using static net.server.coordinator.session.SessionCoordinator;

namespace client;

public class Client : ChannelHandlerAdapter
{
    readonly ILogger _logger = LogFactory.GetLogger("Client_");
    public static int LOGIN_NOTLOGGEDIN = 0;
    public static int LOGIN_SERVER_TRANSITION = 1;
    public static int LOGIN_LOGGEDIN = 2;

    private Type type;
    private long sessionId;
    private PacketProcessor packetProcessor;

    private Hwid? _hwid;
    private string remoteAddress;
    private volatile bool inTransition;

    private IChannel ioChannel;
    private Character? player;
    private int channel = 1;
    private int accId = -4;
    private bool loggedIn = false;
    private bool serverTransition = false;
    private DateTime? birthday = null;
    private string? accountName = null;
    private int world;
    private long lastPong;
    private int gmlevel;
    private HashSet<string> macs = new();
    private Dictionary<string, V8ScriptEngine> engines = new();
    private sbyte characterSlots = 3;
    private byte loginattempt = 0;
    private string _pin = "";
    private int pinattempt = 0;
    private string _pic = "";
    private int picattempt = 0;
    private byte csattempt = 0;
    private sbyte gender = -1;
    private bool disconnecting = false;
    private Semaphore actionsSemaphore = new Semaphore(7, 7);
    private object lockObj = new object();
    private object encoderLock = new object();
    // thanks Masterrulax & try2hack for pointing out a bottleneck issue with shared locks, shavit for noticing an opportunity for improvement
    private DateTimeOffset? tempBanCalendar;
    private int votePoints;
    private int voteTime = -1;
    private int visibleWorlds;
    private long lastNpcClick;
    private long lastPacket = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    private int lang = 0;

    public enum Type
    {
        Mock = -1,
        LOGIN,
        CHANNEL
    }

    public Client(Type type, long sessionId, string remoteAddress, PacketProcessor packetProcessor, int world, int channel)
    {
        this.type = type;
        this.sessionId = sessionId;
        this.remoteAddress = remoteAddress;
        this.packetProcessor = packetProcessor;
        this.world = world;
        this.channel = channel;
    }

    public static Client createLoginClient(long sessionId, string remoteAddress, PacketProcessor packetProcessor,
                                           int world, int channel)
    {
        return new Client(Type.LOGIN, sessionId, remoteAddress, packetProcessor, world, channel);
    }

    public static Client createChannelClient(long sessionId, string remoteAddress, PacketProcessor packetProcessor,
                                             int world, int channel)
    {
        return new Client(Type.CHANNEL, sessionId, remoteAddress, packetProcessor, world, channel);
    }

    public static Client createMock()
    {
        return new Client(Type.Mock, -1, null, null, -123, -123);
    }

    public override async void ChannelActive(IChannelHandlerContext ctx)
    {
        var channel = ctx.Channel;
        if (!Server.getInstance().isOnline())
        {
            await channel.CloseAsync();
            return;
        }

        this.remoteAddress = getRemoteAddress(channel);
        this.ioChannel = channel;
    }

    private static string getRemoteAddress(IChannel channel)
    {
        string remoteAddress = "null";
        try
        {
            remoteAddress = channel.RemoteAddress.GetIPv4Address();
        }
        catch (NullReferenceException npe)
        {
            Log.Logger.Warning("Unable to get remote address for client" + npe);
        }

        return remoteAddress;
    }

    public override void ChannelRead(IChannelHandlerContext ctx, object msg)
    {
        if (!(msg is InPacket packet))
        {
            _logger.Warning("Received invalid message: {}", msg);
            return;
        }

        short opcode = packet.readShort();
        PacketHandler handler = packetProcessor.getHandler(opcode);

        if (YamlConfig.config.server.USE_DEBUG_SHOW_RCVD_PACKET && !LoggingUtil.isIgnoredRecvPacket(opcode))
        {
            _logger.Debug("Received packet id {}", opcode);
        }

        if (handler != null && handler.validateState(this))
        {
            try
            {
                MonitoredChrLogger.logPacketIfMonitored(this, opcode, packet.getBytes());
                handler.handlePacket(packet, this);
            }
            catch (Exception t)
            {
                string chrInfo = player != null ? player.getName() + " on map " + player.getMapId() : "?";
                _logger.Warning("Error in packet handler {}. Chr {}, account {}. Packet: {}", handler.GetType().Name,
                        chrInfo, getAccountName(), packet, t);
                //client.sendPacket(PacketCreator.enableActions());//bugs sometimes
            }
        }

        updateLastPacket();
    }

    public override void UserEventTriggered(IChannelHandlerContext ctx, object evt)
    {
        if (evt is IdleStateEvent idleEvent)
        {
            checkIfIdle(idleEvent);
        }
    }

    public override void ExceptionCaught(IChannelHandlerContext ctx, Exception cause)
    {
        if (player != null)
        {
            _logger.Warning("Exception caught by {}", player, cause);
        }

        if (cause is InvalidPacketHeaderException)
        {
            SessionCoordinator.getInstance().closeSession(this, true);
        }
        else if (cause is IOException)
        {
            closeMapleSession();
        }
    }

    public override void ChannelInactive(IChannelHandlerContext ctx)
    {
        closeMapleSession();
    }

    private void closeMapleSession()
    {
        switch (type)
        {
            case Type.LOGIN:
                SessionCoordinator.getInstance().closeLoginSession(this);
                break;
            case Type.CHANNEL:
                SessionCoordinator.getInstance().closeSession(this, null);
                break;
            default:
                break;
        }

        try
        {
            // client freeze issues on session transition states found thanks to yolinlin, Omo Oppa, Nozphex
            if (!inTransition)
            {
                disconnect(false, false);
            }
        }
        catch (Exception t)
        {
            _logger.Warning("Account stuck", t);
        }
        finally
        {
            closeSession();
        }
    }

    public void updateLastPacket()
    {
        lastPacket = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }

    public long getLastPacket()
    {
        return lastPacket;
    }

    public void closeSession()
    {
        ioChannel.CloseAsync().Wait();
    }

    public void disconnectSession()
    {
        ioChannel.DisconnectAsync().Wait();
    }

    public Hwid? getHwid()
    {
        return _hwid;
    }

    public void setHwid(Hwid? hwid)
    {
        this._hwid = hwid;
    }

    public string getRemoteAddress()
    {
        return remoteAddress;
    }

    public bool isInTransition()
    {
        return inTransition;
    }

    public EventManager getEventManager(string evt)
    {
        return getChannelServer().getEventSM().getEventManager(evt);
    }

    public Character getPlayer()
    {
        return player;
    }

    public void setPlayer(Character? player)
    {
        this.player = player;
    }

    public AbstractPlayerInteraction getAbstractPlayerInteraction()
    {
        return new AbstractPlayerInteraction(this);
    }

    public void sendCharList(int server)
    {
        this.sendPacket(PacketCreator.getCharList(this, server, 0));
    }

    public List<Character> loadCharacters(int serverId)
    {
        List<Character> chars = new(15);
        try
        {
            foreach (CharacterNameAndId cni in loadCharactersInternal(serverId))
            {
                chars.Add(Character.loadCharFromDB(cni.id, this, false));
            }
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
        }
        return chars;
    }

    public List<string> loadCharacterNames(int worldId)
    {
        List<string> chars = new(15);
        foreach (CharacterNameAndId cni in loadCharactersInternal(worldId))
        {
            chars.Add(cni.name);
        }
        return chars;
    }

    private List<CharacterNameAndId> loadCharactersInternal(int worldId)
    {
        using var dbContext = new DBContext();
        return dbContext.Characters.Where(x => x.AccountId == accId && x.World == worldId).Select(x => new CharacterNameAndId(x.Id, x.Name)).Take(15).ToList();
    }

    public bool isLoggedIn()
    {
        return loggedIn;
    }

    public bool hasBannedIP()
    {
        using var dbContext = new DBContext();
        var ip = ioChannel.RemoteAddress.GetIPv4Address();
        return dbContext.Ipbans.Any(x => x.Ip.Contains(ip.ToString()));
    }

    //public int getVoteTime()
    //{
    //    if (voteTime != -1)
    //    {
    //        return voteTime;
    //    }

    //    try (DBContext dbContext = DatabaseConnection.getConnection();
    //    PreparedStatement ps = con.prepareStatement("SELECT date FROM bit_votingrecords WHERE UPPER(account) = UPPER(?)")) {
    //        ps.setString(1, accountName);
    //        try (ResultSet rs = ps.executeQuery()) {
    //            if (!rs.next())
    //            {
    //                return -1;
    //            }
    //            voteTime = rs.getInt("date");
    //        }
    //        }
    //        catch (SQLException e)
    //        {
    //            _logger.Error("Error getting voting time");
    //            return -1;
    //        }
    //        return voteTime;
    //    }

    //    public void resetVoteTime()
    //    {
    //        voteTime = -1;
    //    }

    //    public bool hasVotedAlready()
    //    {
    //        Date currentDate = new Date();
    //        int timeNow = (int)(currentDate.getTime() / 1000);
    //        int difference = (timeNow - getVoteTime());
    //        return difference < 86400 && difference > 0;
    //    }

    public bool hasBannedHWID()
    {
        if (_hwid == null)
        {
            return false;
        }
        using var dbContext = new DBContext();
        return dbContext.Hwidbans.Any(x => x.Hwid.Contains(_hwid.hwid));
    }

    public bool hasBannedMac()
    {
        if (macs.Count == 0)
        {
            return false;
        }
        using var _dbContext = new DBContext();
        return _dbContext.Macbans.Any(x => macs.Contains(x.Mac));
    }

    private void loadHWIDIfNescessary()
    {
        if (_hwid == null)
        {
            using var _dbContext = new DBContext();
            _hwid = new Hwid(_dbContext.Accounts.Where(x => x.Id == accId).Select(x => x.Hwid).FirstOrDefault());
        }
    }

    // TODO: Recode to close statements...
    private void loadMacsIfNescessary()
    {
        if (macs.Count == 0)
        {
            using var dbContext = new DBContext();
            var dbModel = dbContext.Accounts.Where(x => x.Id == accId).Select(x => new { x.Macs }).FirstOrDefault();
            if (dbModel != null && !string.IsNullOrEmpty(dbModel.Macs))
            {
                var splitedData = dbModel.Macs.Split(",").Where(x => !string.IsNullOrEmpty(x)).ToList();
                macs.addAll(splitedData);
            }
        }
    }


    public void banHWID()
    {
        try
        {
            loadHWIDIfNescessary();

            using var _dbContext = new DBContext();
            _dbContext.Hwidbans.Add(new Hwidban() { Hwid = getHwid().hwid });
            _dbContext.SaveChanges();
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
        }
    }

    public void banMacs()
    {
        try
        {
            loadMacsIfNescessary();

            using var dbContext = new DBContext();
            List<string> filtered = dbContext.Macfilters.Select(x => x.Filter).ToList();

            foreach (string mac in macs)
            {
                if (!filtered.Any(x => Regex.IsMatch(mac, x)))
                {
                    dbContext.Macbans.Add(new Macban(mac, getAccID().ToString()));
                }
            }
            dbContext.SaveChanges();
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
        }
    }

    public int finishLogin()
    {
        Monitor.Enter(encoderLock);
        try
        {
            if (getLoginState() > LOGIN_NOTLOGGEDIN)
            { // 0 = LOGIN_NOTLOGGEDIN, 1= LOGIN_SERVER_TRANSITION, 2 = LOGIN_LOGGEDIN
                loggedIn = false;
                return 7;
            }
            updateLoginState(Client.LOGIN_LOGGEDIN);
        }
        finally
        {
            Monitor.Exit(encoderLock);
        }

        return 0;
    }

    public void setPin(string pin)
    {
        this._pin = pin;
        using var _dbContext = new DBContext();
        _dbContext.Accounts.Where(x => x.Id == accId).ExecuteUpdate(x => x.SetProperty(y => y.Pin, pin));
    }

    public string getPin()
    {
        return _pin;
    }

    public bool checkPin(string other)
    {
        if (!(YamlConfig.config.server.ENABLE_PIN && !canBypassPin()))
        {
            return true;
        }

        pinattempt++;
        if (pinattempt > 5)
        {
            SessionCoordinator.getInstance().closeSession(this, false);
        }
        if (_pin.Equals(other))
        {
            pinattempt = 0;
            LoginBypassCoordinator.getInstance().registerLoginBypassEntry(_hwid, accId, false);
            return true;
        }
        return false;
    }

    public void setPic(string pic)
    {
        this._pic = pic;
        using var _dbContext = new DBContext();
        _dbContext.Accounts.Where(x => x.Id == accId).ExecuteUpdate(x => x.SetProperty(y => y.Pic, pic));
    }

    public string getPic()
    {
        return _pic;
    }

    public bool checkPic(string other)
    {
        if (!(YamlConfig.config.server.ENABLE_PIC && !canBypassPic()))
        {
            return true;
        }

        picattempt++;
        if (picattempt > 5)
        {
            SessionCoordinator.getInstance().closeSession(this, false);
        }
        if (_pic.Equals(other))
        {    // thanks ryantpayton (HeavenClient) for noticing null pics being checked here
            picattempt = 0;
            LoginBypassCoordinator.getInstance().registerLoginBypassEntry(_hwid, accId, true);
            return true;
        }
        return false;
    }

    public int login(string login, string pwd, Hwid nibbleHwid)
    {
        int loginok = 5;

        loginattempt++;
        if (loginattempt > 4)
        {
            loggedIn = false;
            SessionCoordinator.getInstance().closeSession(this, false);
            return 6;   // thanks Survival_Project for finding out an issue with AUTOMATIC_REGISTER here
        }

        try
        {
            using var dbContext = new DBContext();
            var dbModel = dbContext.Accounts.Where(x => x.Name == login).FirstOrDefault();
            accId = -2;
            if (dbModel != null)
            {
                accId = dbModel.Id;
                if (accId <= 0)
                {
                    Log.Logger.Error("Tried to login with accid " + accId);
                    return 15;
                }

                bool banned = dbModel.Banned == 1;
                gmlevel = 0;
                _pin = dbModel.Pin;
                _pic = dbModel.Pic;
                gender = dbModel.Gender;
                characterSlots = dbModel.Characterslots;
                lang = dbModel.Language;
                string passhash = dbModel.Password;
                var tos = dbModel.Tos;

                if (banned)
                {
                    return 3;
                }

                if (getLoginState() > LOGIN_NOTLOGGEDIN)
                { // already loggedin
                    loggedIn = false;
                    loginok = 7;
                }
                else if (passhash.ElementAt(0) == '$' && passhash.ElementAt(1) == '2' && BCrypt.checkpw(pwd, passhash))
                {
                    loginok = !tos ? 23 : 0;
                }
                else if (pwd.Equals(passhash) || checkHash(passhash, "SHA-1", pwd) || checkHash(passhash, "SHA-512", pwd))
                {
                    // thanks GabrielSin for detecting some no-bcrypt inconsistencies here
                    loginok = !tos ? (!YamlConfig.config.server.BCRYPT_MIGRATION ? 23 : -23) : (!YamlConfig.config.server.BCRYPT_MIGRATION ? 0 : -10); // migrate to bcrypt
                }
                else
                {
                    loggedIn = false;
                    loginok = 4;
                }
            }
            else
            {
                accId = -3;
            }
        }
        catch (DbUpdateException e)
        {
            Log.Logger.Error(e.ToString());
        }

        if (loginok == 0 || loginok == 4)
        {
            AntiMulticlientResult res = SessionCoordinator.getInstance().attemptLoginSession(this, nibbleHwid, accId, loginok == 4);

            switch (res)
            {
                case AntiMulticlientResult.SUCCESS:
                    if (loginok == 0)
                    {
                        loginattempt = 0;
                    }

                    return loginok;

                case AntiMulticlientResult.REMOTE_LOGGEDIN:
                    return 17;

                case AntiMulticlientResult.REMOTE_REACHED_LIMIT:
                    return 13;

                case AntiMulticlientResult.REMOTE_PROCESSING:
                    return 10;

                case AntiMulticlientResult.MANY_ACCOUNT_ATTEMPTS:
                    return 16;

                default:
                    return 8;
            }
        }
        else
        {
            return loginok;
        }
    }

    public DateTimeOffset? getTempBanCalendarFromDB()
    {
        try
        {
            using var dbContext = new DBContext();
            var dbModel = dbContext.Accounts.Where(x => x.Id == getAccID()).Select(x => new { x.Tempban }).FirstOrDefault();
            if (dbModel == null || dbModel.Tempban == DateTimeOffset.MinValue || dbModel.Tempban == DefaultDates.getTempban())
            {
                return null;
            }
            tempBanCalendar = dbModel.Tempban;
            return tempBanCalendar;
        }
        catch (DbUpdateException e)
        {
            Log.Logger.Error(e.ToString());
        }
        return null;//why oh why!?!
    }

    public DateTimeOffset? getTempBanCalendar()
    {
        return tempBanCalendar;
    }

    public bool hasBeenBanned()
    {
        return tempBanCalendar != null;
    }

    public static long dottedQuadToLong(string dottedQuad)
    {
        string[] quads = dottedQuad.Split("\\.");
        if (quads.Length != 4)
        {
            throw new Exception("Invalid IP Address format.");
        }
        long ipAddress = 0;
        for (int i = 0; i < 4; i++)
        {
            int quad = int.Parse(quads[i]);
            ipAddress += quad % 256 * (long)Math.Pow(256, 4 - i);
        }
        return ipAddress;
    }

    public void updateHwid(Hwid hwid)
    {
        this._hwid = hwid;

        using var dbContext = new DBContext();
        dbContext.Accounts.Where(x => x.Id == accId).ExecuteUpdate(x => x.SetProperty(y => y.Hwid, hwid.hwid));
    }

    public void updateMacs(string macData)
    {
        macs.addAll(Arrays.asList(macData.Split(", ")));
        var macString = string.Join(',', macs);
        try
        {
            using var dbContext = new DBContext();
            dbContext.Accounts.Where(x => x.Id == accId).ExecuteUpdate(x => x.SetProperty(y => y.Macs, macString));
        }
        catch (DbUpdateException e)
        {
            Log.Logger.Error(e.ToString());
        }
    }

    public void setAccID(int id)
    {
        this.accId = id;
    }

    public int getAccID()
    {
        return accId;
    }

    public void updateLoginState(int newState)
    {
        // rules out possibility of multiple account entries
        if (newState == LOGIN_LOGGEDIN)
        {
            SessionCoordinator.getInstance().updateOnlineClient(this);
        }

        using var dbContext = new DBContext();
        dbContext.Accounts.Where(x => x.Id == getAccID()).ExecuteUpdate(x => x.SetProperty(y => y.Loggedin, (sbyte)newState).SetProperty(y => y.Lastlogin, DateTimeOffset.Now));

        if (newState == LOGIN_NOTLOGGEDIN)
        {
            loggedIn = false;
            serverTransition = false;
            setAccID(0);
        }
        else
        {
            serverTransition = (newState == LOGIN_SERVER_TRANSITION);
            loggedIn = !serverTransition;
        }
    }

    public int getLoginState()
    {  // 0 = LOGIN_NOTLOGGEDIN, 1= LOGIN_SERVER_TRANSITION, 2 = LOGIN_LOGGEDIN
        try
        {
            using var dbContext = new DBContext();
            var data = dbContext.Accounts.Where(x => x.Id == getAccID()).Select(x => new { x.Loggedin, x.Lastlogin, x.Birthday }).FirstOrDefault();
            if (data == null)
            {
                throw new Exception("getLoginState - MapleClient AccID: " + getAccID());
            }

            birthday = data.Birthday;

            int state = data.Loggedin;
            if (state == LOGIN_SERVER_TRANSITION)
            {
                if (data.Lastlogin!.Value.AddSeconds(30).ToUnixTimeMilliseconds() < Server.getInstance().getCurrentTime())
                {
                    int accountId = accId;
                    state = LOGIN_NOTLOGGEDIN;
                    updateLoginState(LOGIN_NOTLOGGEDIN);   // ACCID = 0, issue found thanks to Tochi & K u ssss o & Thora & Omo Oppa
                    this.setAccID(accountId);
                }
            }
            if (state == LOGIN_LOGGEDIN)
            {
                loggedIn = true;
            }
            else if (state == LOGIN_SERVER_TRANSITION)
            {
                dbContext.Accounts.Where(x => x.Id == getAccID()).ExecuteUpdate(x => x.SetProperty(y => y.Loggedin, 0));
            }
            else
            {
                loggedIn = false;
            }

            return state;
        }
        catch (DbUpdateException e)
        {
            loggedIn = false;
            Log.Logger.Error(e.ToString());
            throw new Exception("login state");
        }
    }


    public bool checkBirthDate(DateTime date)
    {
        return date.Date == birthday.GetValueOrDefault().Date;
    }

    private void removePartyPlayer(World wserv)
    {
        MapleMap map = player.getMap();
        var party = player.getParty();
        int idz = player.getId();

        if (party != null)
        {
            PartyCharacter chrp = new PartyCharacter(player);
            chrp.setOnline(false);
            wserv.updateParty(party.getId(), PartyOperation.LOG_ONOFF, chrp);
            if (party.getLeader().getId() == idz && map != null)
            {
                PartyCharacter? lchr = null;
                foreach (PartyCharacter pchr in party.getMembers())
                {
                    if (pchr != null && pchr.getId() != idz && (lchr == null || lchr.getLevel() <= pchr.getLevel()) && map.getCharacterById(pchr.getId()) != null)
                    {
                        lchr = pchr;
                    }
                }
                if (lchr != null)
                {
                    wserv.updateParty(party.getId(), PartyOperation.CHANGE_LEADER, lchr);
                }
            }
        }
    }

    private void removePlayer(World wserv, bool serverTransition)
    {
        try
        {
            player.setDisconnectedFromChannelWorld();
            player.notifyMapTransferToPartner(-1);
            player.removeIncomingInvites();
            player.cancelAllBuffs(true);

            player.closePlayerInteractions();
            player.closePartySearchInteractions();

            if (!serverTransition)
            {    // thanks MedicOP for detecting an issue with party leader change on changing channels
                removePartyPlayer(wserv);

                var eim = player.getEventInstance();
                if (eim != null)
                {
                    eim.playerDisconnected(player);
                }

                if (player.getMonsterCarnival() != null)
                {
                    player.getMonsterCarnival()!.playerDisconnected(getPlayer().getId());
                }

                if (player.getAriantColiseum() != null)
                {
                    player.getAriantColiseum().playerDisconnected(getPlayer());
                }
            }

            if (player.getMap() != null)
            {
                int mapId = player.getMapId();
                player.getMap().removePlayer(player);
                if (MapId.isDojo(mapId))
                {
                    this.getChannelServer().freeDojoSectionIfEmpty(mapId);
                }

                if (player.getMap().getHPDec() > 0)
                {
                    getWorldServer().removePlayerHpDecrease(player);
                }
            }

        }
        catch (Exception t)
        {
            _logger.Error("Account stuck", t);
        }
    }

    public void disconnect(bool shutdown, bool cashshop)
    {
        if (canDisconnect())
        {
            ThreadManager.getInstance().newTask(() => disconnectInternal(shutdown, cashshop));
        }
    }

    public void forceDisconnect()
    {
        if (canDisconnect())
        {
            disconnectInternal(true, false);
        }
    }

    object checkCanDisconnectLock = new object();
    private bool canDisconnect()
    {
        lock (checkCanDisconnectLock)
        {
            if (disconnecting)
            {
                return false;
            }

            disconnecting = true;
            return true;
        }
    }

    private void disconnectInternal(bool shutdown, bool cashshop)
    {
        //once per Client instance
        if (player != null && player.isLoggedin() && player.getClient() != null)
        {
            int messengerid = player.getMessenger()?.getId() ?? 0;
            //int fid = player.getFamilyId();
            BuddyList bl = player.getBuddylist();
            MessengerCharacter chrm = new MessengerCharacter(player, 0);
            var chrg = player.getMGC();
            var guild = player.getGuild();

            player.cancelMagicDoor();

            World wserv = getWorldServer();   // obviously wserv is NOT null if this player was online on it
            try
            {
                removePlayer(wserv, this.serverTransition);

                if (!(channel == -1 || shutdown))
                {
                    if (!cashshop)
                    {
                        if (!this.serverTransition)
                        { // meaning not changing channels
                            if (messengerid > 0)
                            {
                                wserv.leaveMessenger(messengerid, chrm);
                            }
                            /*      
                            if (fid > 0) {
                                    Family family = worlda.getFamily(fid);
                                    family.
                            }
                            */

                            player.forfeitExpirableQuests();    //This is for those quests that you have to stay logged in for a certain amount of time

                            if (guild != null)
                            {
                                Server server = Server.getInstance();
                                server.setGuildMemberOnline(player, false, player.getClient().getChannel());
                                player.sendPacket(GuildPackets.showGuildInfo(player));
                            }
                            if (bl != null)
                            {
                                wserv.loggedOff(player.getName(), player.getId(), channel, player.getBuddylist().getBuddyIds());
                            }
                        }
                    }
                    else
                    {
                        if (!this.serverTransition)
                        { // if dc inside of cash shop.
                            if (bl != null)
                            {
                                wserv.loggedOff(player.getName(), player.getId(), channel, player.getBuddylist().getBuddyIds());
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Account stuck");
            }
            finally
            {
                if (!this.serverTransition)
                {
                    if (chrg != null)
                    {
                        chrg.setCharacter(null);
                    }
                    wserv.removePlayer(player);
                    //getChannelServer().removePlayer(player); already being done

                    player.saveCooldowns();
                    player.cancelAllDebuffs();
                    player.saveCharToDB(true);

                    player.logOff();
                    if (YamlConfig.config.server.INSTANT_NAME_CHANGE)
                    {
                        player.doPendingNameChange();
                    }
                    clear();
                }
                else
                {
                    getChannelServer().removePlayer(player);

                    player.saveCooldowns();
                    player.cancelAllDebuffs();
                    player.saveCharToDB();
                }
            }
        }

        SessionCoordinator.getInstance().closeSession(this, false);

        if (!serverTransition && isLoggedIn())
        {
            updateLoginState(Client.LOGIN_NOTLOGGEDIN);

            clear();
        }
        else
        {
            if (!Server.getInstance().hasCharacteridInTransition(this))
            {
                updateLoginState(Client.LOGIN_NOTLOGGEDIN);
            }

            engines.Clear();
        }
    }

    private void clear()
    {
        // player hard reference removal thanks to Steve (kaito1410)
        if (this.player != null)
        {
            this.player.empty(true); // clears schedules and stuff
        }

        Server.getInstance().unregisterLoginState(this);

        this.accountName = null;
        this.macs = null;
        this._hwid = null;
        this.birthday = null;
        this.engines = null;
        this.player = null;
    }

    public void setCharacterOnSessionTransitionState(int cid)
    {
        this.updateLoginState(Client.LOGIN_SERVER_TRANSITION);
        this.inTransition = true;
        Server.getInstance().setCharacteridInTransition(this, cid);
    }

    public int getChannel()
    {
        return channel;
    }

    public Channel getChannelServer()
    {
        return Server.getInstance().getChannel(world, channel);
    }

    public World getWorldServer()
    {
        return Server.getInstance().getWorld(world);
    }

    public Channel getChannelServer(byte channel)
    {
        return Server.getInstance().getChannel(world, channel);
    }

    public bool deleteCharacter(int cid, int senderAccId)
    {
        try
        {
            Character chr = Character.loadCharFromDB(cid, this, false);

            int? partyid = chr.getWorldServer().getCharacterPartyid(cid);
            if (partyid != null)
            {
                this.setPlayer(chr);

                Party party = chr.getWorldServer().getParty(partyid.Value);
                chr.setParty(party);
                chr.getMPC();
                chr.leaveParty();   // thanks Vcoc for pointing out deleted characters would still stay in a party

                this.setPlayer(null);
            }

            return Character.deleteCharFromDB(chr, senderAccId);
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex.ToString());
            return false;
        }
    }

    public string getAccountName()
    {
        return accountName;
    }

    public void setAccountName(string a)
    {
        this.accountName = a;
    }

    public void setChannel(int channel)
    {
        this.channel = channel;
    }

    public int getWorld()
    {
        return world;
    }

    public void setWorld(int world)
    {
        this.world = world;
    }

    public void pongReceived()
    {
        lastPong = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }

    public void checkIfIdle(IdleStateEvent evt)
    {
        long pingedAt = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        sendPacket(PacketCreator.getPing());
        TimerManager.getInstance().schedule(() =>
        {
            try
            {
                if (lastPong < pingedAt)
                {
                    if (ioChannel.Active)
                    {
                        _logger.Information("Disconnected {} due to idling. Reason: {}", remoteAddress, evt.State);
                        updateLoginState(Client.LOGIN_NOTLOGGEDIN);
                        disconnectSession();
                    }
                }
            }
            catch (NullReferenceException e)
            {
                Log.Logger.Error(e.ToString());
            }
        }, TimeSpan.FromSeconds(15));
    }

    public HashSet<string> getMacs()
    {
        return macs.ToHashSet();
    }

    public int getGMLevel()
    {
        return gmlevel;
    }

    public void setGMLevel(int level)
    {
        gmlevel = level;
    }

    public void setScriptEngine(string name, V8ScriptEngine e)
    {
        engines.AddOrUpdate(name, e);
    }

    public V8ScriptEngine? getScriptEngine(string name)
    {
        return engines.GetValueOrDefault(name);
    }

    public void removeScriptEngine(string name)
    {
        engines.Remove(name);
    }

    public NPCConversationManager getCM()
    {
        return NPCScriptManager.getInstance().getCM(this);
    }

    public QuestActionManager getQM()
    {
        return QuestScriptManager.getInstance().getQM(this);
    }

    public bool acceptToS()
    {
        if (accountName == null)
        {
            return true;
        }

        bool disconnect = false;
        try
        {
            using var dbContext = new DBContext();
            disconnect = dbContext.Accounts.Where(x => x.Id == accId).Select(x => x.Tos).FirstOrDefault();
            dbContext.Accounts.Where(x => x.Id == accId).ExecuteUpdate(x => x.SetProperty(y => y.Tos, true));
        }
        catch (DbUpdateException e)
        {
            Log.Logger.Error(e.ToString());
        }
        return disconnect;
    }

    public void checkChar(int accid)
    {  /// issue with multiple chars from same account login found by shavit, resinate
        if (!YamlConfig.config.server.USE_CHARACTER_ACCOUNT_CHECK)
        {
            return;
        }

        foreach (World w in Server.getInstance().getWorlds())
        {
            foreach (Character chr in w.getPlayerStorage().getAllCharacters())
            {
                if (accid == chr.getAccountID())
                {
                    _logger.Warning("Chr {} has been removed from world {}. Possible Dupe attempt.", chr.getName(), GameConstants.WORLD_NAMES[w.getId()]);
                    chr.getClient().forceDisconnect();
                    w.getPlayerStorage().removePlayer(chr.getId());
                }
            }
        }
    }

    public int getVotePoints()
    {
        int points = 0;
        try
        {
            using var dbContext = new DBContext();
            votePoints = dbContext.Accounts.Where(x => x.Id == accId).Select(x => new { x.Votepoints }).FirstOrDefault()?.Votepoints ?? 0;
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
        }
        votePoints = points;
        return votePoints;
    }

    public void addVotePoints(int points)
    {
        votePoints += points;
        saveVotePoints();
    }

    public void useVotePoints(int points)
    {
        if (points > votePoints)
        {
            //Should not happen, should probably log this
            return;
        }
        votePoints -= points;
        saveVotePoints();
        MapleLeafLogger.log(player, false, points.ToString());
    }

    private void saveVotePoints()
    {
        using var _dbContext = new DBContext();
        _dbContext.Accounts.Where(x => x.Id == accId).ExecuteUpdate(x => x.SetProperty(y => y.Votepoints, votePoints));
    }

    public void lockClient()
    {
        Monitor.Enter(lockObj);
    }

    public void unlockClient()
    {
        Monitor.Exit(lockObj);
    }

    public bool tryacquireClient()
    {
        if (actionsSemaphore.WaitOne())
        {
            lockClient();
            return true;
        }
        else
        {
            return false;
        }
    }

    public void releaseClient()
    {
        unlockClient();
        actionsSemaphore.Release();
    }

    public bool tryacquireEncoder()
    {
        if (actionsSemaphore.WaitOne())
        {
            Monitor.Enter(encoderLock);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void unlockEncoder()
    {
        Monitor.Exit(encoderLock);
        actionsSemaphore.Release();
    }

    private static bool checkHash(string hash, string type, string password)
    {
        try
        {
            return HashDigest.HashByType(type, password).ToHexString().Equals(hash);
        }
        catch (Exception e)
        {
            throw new Exception("Encoding the string failed", e);
        }
    }

    public short getAvailableCharacterSlots()
    {
        return (short)Math.Max(0, characterSlots - Server.getInstance().getAccountCharacterCount(accId));
    }

    public short getAvailableCharacterWorldSlots()
    {
        return (short)Math.Max(0, characterSlots - Server.getInstance().getAccountWorldCharacterCount(accId, world));
    }

    public short getAvailableCharacterWorldSlots(int world)
    {
        return (short)Math.Max(0, characterSlots - Server.getInstance().getAccountWorldCharacterCount(accId, world));
    }

    public short getCharacterSlots()
    {
        return characterSlots;
    }

    public void setCharacterSlots(sbyte slots)
    {
        characterSlots = slots;
    }

    public bool canGainCharacterSlot()
    {
        return characterSlots < 15;
    }

    object gainChrSlotLock = new object();
    public bool gainCharacterSlot()
    {
        lock (gainChrSlotLock)
        {
            if (canGainCharacterSlot())
            {
                using var dbContext = new DBContext();
                ++this.characterSlots;
                dbContext.Accounts.Where(x => x.Id == accId).ExecuteUpdate(x => x.SetProperty(y => y.Characterslots, this.characterSlots));
                return true;
            }
            return false;
        }

    }

    public sbyte getGReason()
    {
        using var dbContext = new DBContext();
        return dbContext.Accounts.Where(x => x.Id == accId).Select(x => new { x.Greason }).FirstOrDefault()?.Greason ?? 0;
    }

    public sbyte getGender()
    {
        return gender;
    }

    public void setGender(sbyte m)
    {
        this.gender = m;
        using var dbContext = new DBContext();
        dbContext.Accounts.Where(x => x.Id == accId).ExecuteUpdate(x => x.SetProperty(y => y.Gender, gender));
    }

    private void announceDisableServerMessage()
    {
        if (!this.getWorldServer().registerDisabledServerMessage(player.getId()))
        {
            sendPacket(PacketCreator.serverMessage(""));
        }
    }

    public void announceServerMessage()
    {
        sendPacket(PacketCreator.serverMessage(this.getChannelServer().getServerMessage()));
    }

    object announceBossHPLock = new object();
    public void announceBossHpBar(Monster mm, int mobHash, Packet packet)
    {
        lock (announceBossHPLock)
        {
            long timeNow = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            int targetHash = player.getTargetHpBarHash();

            if (mobHash != targetHash)
            {
                if (timeNow - player.getTargetHpBarTime() >= 5 * 1000)
                {
                    // is there a way to INTERRUPT this annoying thread running on the client that drops the boss bar after some time at every attack?
                    announceDisableServerMessage();
                    sendPacket(packet);

                    player.setTargetHpBarHash(mobHash);
                    player.setTargetHpBarTime(timeNow);
                }
            }
            else
            {
                announceDisableServerMessage();
                sendPacket(packet);

                player.setTargetHpBarTime(timeNow);
            }
        }
    }

    private object announcerLock = new object();
    public void sendPacket(Packet packet)
    {
        Monitor.Enter(announcerLock);
        try
        {
            ioChannel.WriteAndFlushAsync(packet).Wait();
        }
        finally
        {
            Monitor.Exit(announcerLock);
        }
    }

    public void announceHint(string msg, int length)
    {
        sendPacket(PacketCreator.sendHint(msg, length, 10));
        sendPacket(PacketCreator.enableActions());
    }

    public void changeChannel(int channel)
    {
        Server server = Server.getInstance();
        if (player.isBanned())
        {
            disconnect(false, false);
            return;
        }
        if (!player.isAlive() || FieldLimit.CANNOTMIGRATE.check(player.getMap().getFieldLimit()))
        {
            sendPacket(PacketCreator.enableActions());
            return;
        }
        else if (MiniDungeonInfo.isDungeonMap(player.getMapId()))
        {
            sendPacket(PacketCreator.serverNotice(5, "Changing channels or entering Cash Shop or MTS are disabled when inside a Mini-Dungeon."));
            sendPacket(PacketCreator.enableActions());
            return;
        }

        string[] socket = Server.getInstance().getInetSocket(this, getWorld(), channel);
        if (socket == null)
        {
            sendPacket(PacketCreator.serverNotice(1, "Channel " + channel + " is currently disabled. Try another channel."));
            sendPacket(PacketCreator.enableActions());
            return;
        }

        player.closePlayerInteractions();
        player.closePartySearchInteractions();

        player.unregisterChairBuff();
        server.getPlayerBuffStorage().addBuffsToStorage(player.getId(), player.getAllBuffs());
        server.getPlayerBuffStorage().addDiseasesToStorage(player.getId(), player.getAllDiseases());
        player.setDisconnectedFromChannelWorld();
        player.notifyMapTransferToPartner(-1);
        player.removeIncomingInvites();
        player.cancelAllBuffs(true);
        player.cancelAllDebuffs();
        player.cancelBuffExpireTask();
        player.cancelDiseaseExpireTask();
        player.cancelSkillCooldownTask();
        player.cancelQuestExpirationTask();
        //Cancelling magicdoor? Nope
        //Cancelling mounts? Noty

        player.getInventory(InventoryType.EQUIPPED).SetChecked(false); //test
        player.getMap().removePlayer(player);
        player.getClient().getChannelServer().removePlayer(player);

        player.saveCharToDB();

        player.setSessionTransitionState();
        try
        {
            sendPacket(PacketCreator.getChannelChange(IPAddress.Parse(socket[0]), int.Parse(socket[1])));
        }
        catch (IOException e)
        {
            Log.Logger.Error(e.ToString());
        }
    }

    public long getSessionId()
    {
        return this.sessionId;
    }

    public bool canRequestCharlist()
    {
        return lastNpcClick + 877 < Server.getInstance().getCurrentTime();
    }

    public bool canClickNPC()
    {
        return lastNpcClick + 500 < Server.getInstance().getCurrentTime();
    }

    public void setClickedNPC()
    {
        lastNpcClick = Server.getInstance().getCurrentTime();
    }

    public void removeClickedNPC()
    {
        lastNpcClick = 0;
    }

    public int getVisibleWorlds()
    {
        return visibleWorlds;
    }

    public void requestedServerlist(int worlds)
    {
        visibleWorlds = worlds;
        setClickedNPC();
    }

    public void closePlayerScriptInteractions()
    {
        this.removeClickedNPC();
        NPCScriptManager.getInstance().dispose(this);
        QuestScriptManager.getInstance().dispose(this);
    }

    public bool attemptCsCoupon()
    {
        if (csattempt > 2)
        {
            resetCsCoupon();
            return false;
        }

        csattempt++;
        return true;
    }

    public void resetCsCoupon()
    {
        csattempt = 0;
    }

    public void enableCSActions()
    {
        sendPacket(PacketCreator.enableCSUse(player));
    }

    public bool canBypassPin()
    {
        return LoginBypassCoordinator.getInstance().canLoginBypass(_hwid, accId, false);
    }

    public bool canBypassPic()
    {
        return LoginBypassCoordinator.getInstance().canLoginBypass(_hwid, accId, true);
    }

    public int getLanguage()
    {
        return lang;
    }

    public void setLanguage(int lingua)
    {
        this.lang = lingua;
    }
}
