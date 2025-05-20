using Application.Core.Channel.Services;
using Application.Core.Game.Life;
using Application.Core.Game.Players;
using Application.Core.Game.TheWorld;
using Application.Core.Net;
using Application.Core.Scripting.Infrastructure;
using Application.EF;
using Application.EF.Entities;
using Application.Scripting;
using Application.Shared.Characters;
using Application.Shared.Constants;
using Application.Shared.Login;
using Application.Shared.Net;
using Application.Utility.Configs;
using Application.Utility.Exceptions;
using client.inventory;
using constants.id;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using net.packet;
using net.packet.logging;
using net.server;
using net.server.guild;
using net.server.world;
using scripting;
using scripting.Event;
using scripting.npc;
using server.maps;
using System.Diagnostics;
using System.Text.RegularExpressions;
using tools;

namespace Application.Core.Channel.Net
{
    public class ChannelClient : ClientBase, IChannelClient
    {
        public AccountDto AccountEntity { get; set; }
        IPacketProcessor<IChannelClient> _packetProcessor;
        public EngineStorage ScriptEngines { get; set; } = new EngineStorage();

        readonly CharacterService _chrSrv;
        public ChannelClient(long sessionId, IWorldChannel currentServer, IChannel nettyChannel, IPacketProcessor<IChannelClient> packetProcessor, ILogger<IClientBase> log, CharacterService characterService)
            : base(sessionId, currentServer, nettyChannel, log)
        {
            CurrentServer = currentServer;
            _packetProcessor = packetProcessor;

            _chrSrv = characterService;
        }

        public override bool IsOnlined => Character != null;

        public IPlayer? Character { get; private set; }

        public IPlayer OnlinedCharacter => Character ?? throw new BusinessCharacterOfflineException();

        public new IWorldChannel CurrentServer { get; }

        /// <summary>
        /// CashShop
        /// </summary>
        public bool IsAwayWorld { get; private set; }
        public int Channel => IsAwayWorld ? -1 : ActualChannel;
        public int ActualChannel => CurrentServer.getId();
        public NPCConversationManager? NPCConversationManager { get; set; }

        public override int AccountId => AccountEntity.Id;

        public override string AccountName => AccountName;
        public override int AccountGMLevel => AccountEntity?.GMLevel ?? 0;

        public override void SetCharacterOnSessionTransitionState(int cid)
        {
            CurrentServer.UpdateAccountState(AccountEntity!.Id, LoginStage.PlayerServerTransition);
            IsServerTransition = true;
            CurrentServer.SetCharacteridInTransition(GetSessionRemoteHost(), cid);
        }

        bool _isDisconnecting = false;
        public void Disconnect(bool isShutdown, bool fromCashShop = false)
        {
            if (_isDisconnecting)
                return;

            _isDisconnecting = true;
            //once per Client instance
            if (Character != null && Character.isLoggedin() && Character.getClient() != null)
            {
                int messengerid = Character.Messenger?.getId() ?? 0;
                //int fid = OnlinedCharacter.getFamilyId();
                MessengerCharacter chrm = new MessengerCharacter(Character, 0);

                var guild = Character.GuildModel;

                Character.cancelMagicDoor();

                var wserv = Server.getInstance().getWorld(0);   // obviously wserv is NOT null if this player was online on it
                try
                {
                    RemovePlayer(Character, wserv, IsServerTransition);

                    if (Channel != -1 && !isShutdown)
                    {
                        if (!IsServerTransition)
                        {
                            if (!fromCashShop)
                            {
                                // meaning not changing channels
                                if (messengerid > 0)
                                {
                                    wserv.leaveMessenger(messengerid, chrm);
                                }


                                Character.forfeitExpirableQuests();    //This is for those quests that you have to stay logged in for a certain amount of time

                                if (guild != null)
                                {
                                    guild.setOnline(Character.Id, false, CurrentServer.getId());
                                    Character.sendPacket(GuildPackets.showGuildInfo(Character));
                                }
                            }

                            if (Character.BuddyList.Count > 0)
                            {
                                CurrentServer.UpdateBuddyByLoggedOff(Character.Id, CurrentServer.getId(), Character.BuddyList.getBuddyIds());
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.LogError(e, "Account stuck");
                }
                finally
                {
                    if (!IsServerTransition)
                    {
                        wserv.removePlayer(Character);
                        //getChannelServer().removePlayer(player); already being done
                        Character.logOff();
                        if (YamlConfig.config.server.INSTANT_NAME_CHANGE)
                        {
                            Character.doPendingNameChange();
                        }

                    }
                    else
                    {
                        CurrentServer.removePlayer(Character);
                    }
                    Character.saveCooldowns();
                    Character.cancelAllDebuffs();

                    CurrentServer.SendPlayerObject(Character);
                }
            }

            CurrentServer.ClientStorage.RemoveClient(AccountEntity!.Id);

            // 为什么不直接  updateLoginState(AccountStage.LOGIN_NOTLOGGEDIN);
            // 如果正在切换频道，这边把状态改成了LOGIN_NOTLOGGEDIN，PlayerLoggedin无法找到正在切换频道的账户

            // 为什么要用CurrentServer.HasCharacteridInTransition判断而不是IsServerTransition
            // 比如切换频道，IsServerTransition=true，如果使用HasCharacteridInTransition则可以很好的知道PlayerLoggedinHandler的流程情况，
            // 反之如果只是卡顿，旧client在这里退出了登录，新client在PlayerLoggedinHandler里也会变成登出态
            if (!IsServerTransition && IsOnlined)
            {
                CurrentServer.Transport.SendAccountLogout(AccountEntity.Id);
            }
            else
            {
                //比如切换频道  但是还没有成功进入新频道
                if (YamlConfig.config.server.USE_IP_VALIDATION && !CurrentServer.HasCharacteridInTransition(GetSessionRemoteHost()))
                {
                    CurrentServer.Transport.SendAccountLogout(AccountEntity.Id);
                    IsServerTransition = false;
                }
            }
            Dispose();
            log.LogDebug($"disconnect: " + new StackTrace().ToString());

            _isDisconnecting = false;
        }

        private void RemovePlayer(IPlayer player, IWorld wserv, bool serverTransition)
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
                {
                    // thanks MedicOP for detecting an issue with party leader change on changing channels
                    RemovePartyPlayer(player, wserv);

                    var eim = player.getEventInstance();
                    if (eim != null)
                    {
                        eim.playerDisconnected(player);
                    }

                    player.getMonsterCarnival()?.playerDisconnected(player);

                    player.getAriantColiseum()?.playerDisconnected(player);
                }

                if (player.getMap() != null)
                {
                    int mapId = player.getMapId();
                    player.getMap().removePlayer(player);
                    if (MapId.isDojo(mapId))
                    {
                        player.getChannelServer().freeDojoSectionIfEmpty(mapId);
                    }

                    if (player.getMap().getHPDec() > 0)
                    {
                        player.getChannelServer().CharacterHpDecreaseController.removePlayerHpDecrease(player);
                    }
                }

            }
            catch (Exception t)
            {
                log.LogError(t, "Account stuck");
            }
        }

        private void RemovePartyPlayer(IPlayer player, IWorld wserv)
        {
            var map = player.getMap();
            var party = player.getParty();
            int idz = player.getId();

            if (party != null)
            {
                wserv.updateParty(party.getId(), PartyOperation.LOG_ONOFF, player);
                if (party.getLeader().getId() == idz && map != null)
                {
                    IPlayer? lchr = null;
                    foreach (var pchr in party.getMembers())
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


        public override void Dispose()
        {
            // player hard reference removal thanks to Steve (kaito1410)
            if (this.Character != null)
            {
                this.Character.Dispose(); // clears schedules and stuff
            }

            this.AccountEntity = null;
            this.Hwid = null;
            this.Character = null;
            this.ScriptEngines.Dispose();
            this.packetChannel.Writer.TryComplete();
        }



        public override void ForceDisconnect()
        {
            Disconnect(true);
        }

        protected override void CloseSessionInternal()
        {
            if (AccountEntity == null)
                return;

            CurrentServer.ClientStorage.RemoveClient(AccountEntity!.Id);

            // client freeze issues on session transition states found thanks to yolinlin, Omo Oppa, Nozphex
            if (!IsServerTransition)
            {
                Disconnect(false);
            }
        }

        protected override void ProcessPacket(InPacket packet)
        {
            short opcode = packet.readShort();
            var handler = _packetProcessor.GetPacketHandler(opcode);

            if (YamlConfig.config.server.USE_DEBUG_SHOW_RCVD_PACKET && !LoggingUtil.isIgnoredRecvPacket(opcode))
            {
                log.LogDebug("Received packet id {Code}", opcode);
            }

            if (handler != null && handler.ValidateState(this))
            {
                MonitoredChrLogger.logPacketIfMonitored(this, opcode, packet.getBytes());
                handler.HandlePacket(packet, this);
            }
        }

        public void enableCSActions()
        {
            sendPacket(PacketCreator.enableCSUse(OnlinedCharacter));
        }

        AbstractPlayerInteraction _pi;
        public AbstractPlayerInteraction getAbstractPlayerInteraction()
        {
            return _pi ??= new AbstractPlayerInteraction(this);
        }

        public IWorld getWorldServer()
        {
            return Server.getInstance().getWorld(0);
        }

        long lastNpcClick;
        public bool canClickNPC()
        {
            return lastNpcClick + 500 < Server.getInstance().getCurrentTime();
        }

        public void setClickedNPC()
        {
            lastNpcClick = CurrentServer.getCurrentTime();
        }

        public void removeClickedNPC()
        {
            lastNpcClick = 0;
        }

        public void OpenNpc(int npcid, string? script = null)
        {
            if (NPCConversationManager != null)
                return;

            removeClickedNPC();
            CurrentServer.NPCScriptManager.start(this, npcid, script, Character);
        }
        public void closePlayerScriptInteractions()
        {
            this.removeClickedNPC();
            NPCConversationManager?.dispose();
        }

        private void announceDisableServerMessage()
        {
            if (!this.getChannelServer().ServerMessageController.registerDisabledServerMessage(OnlinedCharacter.getId()))
            {
                sendPacket(PacketCreator.serverMessage(""));
            }
        }
        public void announceServerMessage()
        {
            sendPacket(PacketCreator.serverMessage(this.CurrentServer.WorldServerMessage));
        }

        public void announceHint(string msg, int length)
        {
            sendPacket(PacketCreator.sendHint(msg, length, 10));
            sendPacket(PacketCreator.enableActions());
        }

        object announceBossHPLock = new object();
        public void announceBossHpBar(Monster mm, int mobHash, Packet packet)
        {
            lock (announceBossHPLock)
            {
                long timeNow = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                int targetHash = OnlinedCharacter.getTargetHpBarHash();

                if (mobHash != targetHash)
                {
                    if (timeNow - OnlinedCharacter.getTargetHpBarTime() >= 5 * 1000)
                    {
                        // is there a way to INTERRUPT this annoying thread running on the client that drops the boss bar after some time at every attack?
                        announceDisableServerMessage();
                        sendPacket(packet);

                        OnlinedCharacter.setTargetHpBarHash(mobHash);
                        OnlinedCharacter.setTargetHpBarTime(timeNow);
                    }
                }
                else
                {
                    announceDisableServerMessage();
                    sendPacket(packet);

                    OnlinedCharacter.setTargetHpBarTime(timeNow);
                }
            }
        }

        public void setScriptEngine(string name, IEngine e)
        {
            ScriptEngines[name] = e;
        }

        public IEngine? getScriptEngine(string name)
        {
            return ScriptEngines[name];
        }

        public void removeScriptEngine(string name)
        {
            ScriptEngines.Remove(name);
        }

        public bool CanGainCharacterSlot()
        {
            return AccountEntity!.Characterslots < Limits.MaxCharacterSlots;
        }
        public bool GainCharacterSlot()
        {
            if (CanGainCharacterSlot())
            {
                AccountEntity!.Characterslots += 1;
                return true;
            }
            return false;
        }

        public void ChangeChannel(int channel)
        {
            if (Character == null)
                return;


            if (Character.isBanned())
            {
                Disconnect(false, false);
                return;
            }
            if (!Character.isAlive() || FieldLimit.CANNOTMIGRATE.check(Character.getMap().getFieldLimit()))
            {
                sendPacket(PacketCreator.enableActions());
                return;
            }
            else if (MiniDungeonInfo.isDungeonMap(Character.getMapId()))
            {
                sendPacket(PacketCreator.serverNotice(5, "Changing channels or entering Cash Shop or MTS are disabled when inside a Mini-Dungeon."));
                sendPacket(PacketCreator.enableActions());
                return;
            }

            var socket = CurrentServer.GetChannelEndPoint(channel);
            if (socket == null)
            {
                sendPacket(PacketCreator.serverNotice(1, "Channel " + channel + " is currently disabled. Try another channel."));
                sendPacket(PacketCreator.enableActions());
                return;
            }

            Character.closePlayerInteractions();
            Character.closePartySearchInteractions();

            Character.unregisterChairBuff();
            CurrentServer.StashCharacterBuff(Character);
            CurrentServer.StashCharacterDisease(Character);
            Character.setDisconnectedFromChannelWorld();
            Character.notifyMapTransferToPartner(-1);
            Character.removeIncomingInvites();
            Character.cancelAllBuffs(true);
            Character.cancelAllDebuffs();
            Character.cancelBuffExpireTask();
            Character.cancelDiseaseExpireTask();
            Character.cancelSkillCooldownTask();
            Character.cancelQuestExpirationTask();
            //Cancelling magicdoor? Nope
            //Cancelling mounts? Noty

            Character.getInventory(InventoryType.EQUIPPED).SetChecked(false); //test
            Character.getMap().removePlayer(Character);
            Character.getChannelServer().removePlayer(Character);
            Character.saveCharToDB();

            SetCharacterOnSessionTransitionState(Character.getId());
            try
            {
                sendPacket(PacketCreator.getChannelChange(socket));
            }
            catch (IOException e)
            {
                log.LogError(e.ToString());
            }
        }

        int csattempt = 0;
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

        public void SetPlayer(IPlayer? player)
        {
            Character = player;
        }

        public void SetAccount(AccountDto accountEntity)
        {
            AccountEntity = accountEntity;

            CurrentServer.ClientStorage.Register(accountEntity.Id, this);
        }

        public IWorldChannel getChannelServer() => CurrentServer;
        public int getChannel() => ActualChannel;

        public override int GetAvailableCharacterSlots()
        {
            return AccountEntity.Characterslots - CurrentServer.GetAccountCharcterCount(AccountEntity.Id);
        }

        public EventManager? getEventManager(string @event)
        {
            return CurrentServer.getEventSM().getEventManager(@event);
        }

        public bool CheckBirthday(DateTime date)
        {
            if (AccountEntity == null)
                return false;

            return date.Month == AccountEntity.Birthday.Month && date.Day == AccountEntity.Birthday.Day;
        }

        public bool CheckBirthday(int dateInt)
        {
            if (DateTime.TryParse(dateInt.ToString(), out var d))
                return CheckBirthday(d);
            return false;
        }

        public void BanMacs()
        {
            try
            {
                using var dbContext = new DBContext();
                List<string> filtered = dbContext.Macfilters.Select(x => x.Filter).ToList();

                foreach (string mac in GetMac())
                {
                    if (!filtered.Any(x => Regex.IsMatch(mac, x)))
                    {
                        dbContext.Macbans.Add(new Macban(mac, AccountEntity!.Id.ToString()));
                    }
                }
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                log.LogError(e.ToString());
            }
        }

        protected override HashSet<string> GetMac()
        {
            if (AccountEntity == null || string.IsNullOrEmpty(AccountEntity.Macs))
                return [];

            return AccountEntity.Macs.Split(",").Select(x => x.Trim()).ToHashSet();
        }

    }
}
