using Application.Core.Game.Life;
using Application.Core.Game.TheWorld;
using Application.Core.Scripting.Infrastructure;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using net.packet;
using net.server.coordinator.session;
using scripting;
using scripting.Event;
using scripting.npc;
using scripting.quest;

namespace Application.Core.Game
{
    public class OfflineClient : IClient
    {
        public string ClientInfo => $"OfflineClient";
        public IPlayer? Character => null;

        public bool acceptToS()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void addVotePoints(int points)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void announceBossHpBar(Monster mm, int mobHash, Packet packet)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void announceHint(string msg, int length)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void announceServerMessage()
        {
            throw new BusinessCharacterOfflineException();
        }

        public bool attemptCsCoupon()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void banHWID()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void banMacs()
        {
            throw new BusinessCharacterOfflineException();
        }

        public bool canBypassPic()
        {
            throw new BusinessCharacterOfflineException();
        }

        public bool canBypassPin()
        {
            throw new BusinessCharacterOfflineException();
        }

        public bool canClickNPC()
        {
            throw new BusinessCharacterOfflineException();
        }

        public bool canGainCharacterSlot()
        {
            throw new BusinessCharacterOfflineException();
        }

        public bool canRequestCharlist()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void changeChannel(int channel)
        {
            throw new BusinessCharacterOfflineException();
        }

        public bool checkBirthDate(DateTime date)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void checkChar(int accid)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void checkIfIdle(IdleStateEvent evt)
        {
            throw new BusinessCharacterOfflineException();
        }

        public bool checkPic(string other)
        {
            throw new BusinessCharacterOfflineException();
        }

        public bool checkPin(string other)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void closePlayerScriptInteractions()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void closeSession()
        {
            throw new BusinessCharacterOfflineException();
        }

        public bool deleteCharacter(int cid, int senderAccId)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void disconnect(bool shutdown, bool cashshop)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void disconnectSession()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void enableCSActions()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void ExceptionCaught(IChannelHandlerContext ctx, Exception cause)
        {
            throw new BusinessCharacterOfflineException();
        }

        public int finishLogin()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void forceDisconnect()
        {
            throw new BusinessCharacterOfflineException();
        }

        public bool gainCharacterSlot()
        {
            throw new BusinessCharacterOfflineException();
        }

        public AbstractPlayerInteraction getAbstractPlayerInteraction()
        {
            throw new BusinessCharacterOfflineException();
        }

        public int getAccID()
        {
            throw new BusinessCharacterOfflineException();
        }

        public string getAccountName()
        {
            throw new BusinessCharacterOfflineException();
        }

        public short getAvailableCharacterSlots()
        {
            throw new BusinessCharacterOfflineException();
        }

        public short getAvailableCharacterWorldSlots()
        {
            throw new BusinessCharacterOfflineException();
        }

        public short getAvailableCharacterWorldSlots(int world)
        {
            throw new BusinessCharacterOfflineException();
        }

        public int getChannel()
        {
            throw new BusinessCharacterOfflineException();
        }

        public IWorldChannel getChannelServer()
        {
            throw new BusinessCharacterOfflineException();
        }

        public IWorldChannel getChannelServer(byte channel)
        {
            throw new BusinessCharacterOfflineException();
        }

        public short getCharacterSlots()
        {
            throw new BusinessCharacterOfflineException();
        }

        public NPCConversationManager? getCM()
        {
            throw new BusinessCharacterOfflineException();
        }

        public EventManager getEventManager(string evt)
        {
            throw new BusinessCharacterOfflineException();
        }

        public sbyte getGender()
        {
            throw new BusinessCharacterOfflineException();
        }

        public int getGMLevel()
        {
            throw new BusinessCharacterOfflineException();
        }

        public sbyte getGReason()
        {
            throw new BusinessCharacterOfflineException();
        }

        public Hwid getHwid()
        {
            throw new BusinessCharacterOfflineException();
        }

        public int getLanguage()
        {
            throw new BusinessCharacterOfflineException();
        }

        public long getLastPacket()
        {
            throw new BusinessCharacterOfflineException();
        }

        public int getLoginState()
        {
            throw new BusinessCharacterOfflineException();
        }

        public HashSet<string> getMacs()
        {
            throw new BusinessCharacterOfflineException();
        }

        public string getPic()
        {
            throw new BusinessCharacterOfflineException();
        }

        public string getPin()
        {
            throw new BusinessCharacterOfflineException();
        }

        public IPlayer? getPlayer()
        {
            throw new BusinessCharacterOfflineException();
        }

        public QuestActionManager? getQM()
        {
            throw new BusinessCharacterOfflineException();
        }

        public string getRemoteAddress()
        {
            throw new BusinessCharacterOfflineException();
        }

        public IEngine? getScriptEngine(string name)
        {
            throw new BusinessCharacterOfflineException();
        }

        public long getSessionId()
        {
            throw new BusinessCharacterOfflineException();
        }

        public DateTimeOffset? getTempBanCalendar()
        {
            throw new BusinessCharacterOfflineException();
        }

        public DateTimeOffset? getTempBanCalendarFromDB()
        {
            throw new BusinessCharacterOfflineException();
        }

        public int getVisibleWorlds()
        {
            throw new BusinessCharacterOfflineException();
        }

        public int getVotePoints()
        {
            throw new BusinessCharacterOfflineException();
        }

        public int getWorld()
        {
            throw new BusinessCharacterOfflineException();
        }

        public IWorld getWorldServer()
        {
            throw new BusinessCharacterOfflineException();
        }

        public bool hasBannedHWID()
        {
            throw new BusinessCharacterOfflineException();
        }

        public bool hasBannedIP()
        {
            throw new BusinessCharacterOfflineException();
        }

        public bool hasBannedMac()
        {
            throw new BusinessCharacterOfflineException();
        }

        public bool hasBeenBanned()
        {
            throw new BusinessCharacterOfflineException();
        }

        public bool isInTransition()
        {
            throw new BusinessCharacterOfflineException();
        }

        public bool isLoggedIn()
        {
            throw new BusinessCharacterOfflineException();
        }

        public List<string> loadCharacterNames(int worldId)
        {
            throw new BusinessCharacterOfflineException();
        }

        public List<IPlayer> loadCharacters(int serverId)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void lockClient()
        {
            throw new BusinessCharacterOfflineException();
        }

        public int login(string login, string pwd, Hwid nibbleHwid)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void pongReceived()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void releaseClient()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void removeClickedNPC()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void removeScriptEngine(string name)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void requestedServerlist(int worlds)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void resetCsCoupon()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void sendCharList(int server)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void sendPacket(Packet packet)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void setAccID(int id)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void setAccountName(string a)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void setChannel(int channel)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void setCharacterOnSessionTransitionState(int cid)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void setCharacterSlots(sbyte slots)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void setClickedNPC()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void setGender(sbyte m)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void setGMLevel(int level)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void setHwid(Hwid? hwid)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void setLanguage(int lingua)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void setPic(string pic)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void setPin(string pin)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void setPlayer(IPlayer? player)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void setScriptEngine(string name, IEngine e)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void setWorld(int world)
        {
            throw new BusinessCharacterOfflineException();
        }

        public bool tryacquireClient()
        {
            throw new BusinessCharacterOfflineException();
        }

        public bool tryacquireEncoder()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void unlockClient()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void unlockEncoder()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void updateHwid(Hwid hwid)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void updateLastPacket()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void updateLoginState(int newState)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void updateMacs(string macData)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void UserEventTriggered(IChannelHandlerContext ctx, object evt)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void useVotePoints(int points)
        {
            throw new BusinessCharacterOfflineException();
        }

        public override string ToString()
        {
            return ClientInfo;
        }
    }
}
