using Application.Core.Channel.ResourceTransaction;
using Application.Core.Game.Life;
using Application.Core.Scripting.Infrastructure;
using Application.Resources.Messages;
using Application.Shared.Events;
using Application.Shared.Languages;
using Application.Shared.Login;
using Application.Shared.Net.Logging;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using scripting;
using scripting.Event;
using scripting.npc;
using tools;

namespace Application.Core.Channel.Net
{
    public class ChannelClient : ClientBase, IChannelClient
    {
        IPacketProcessor<IChannelClient> _packetProcessor;
        public EngineStorage ScriptEngines { get; set; } = new EngineStorage();

        public ChannelClient(long sessionId, WorldChannel currentServer, IChannel nettyChannel, IPacketProcessor<IChannelClient> packetProcessor, ILogger<IClientBase> log)
            : base(sessionId, currentServer, nettyChannel, log)
        {
            CurrentServer = currentServer;
            _packetProcessor = packetProcessor;
        }

        public override bool IsOnlined => Character != null;

        public IPlayer? Character { get; private set; }

        public IPlayer OnlinedCharacter => Character ?? throw new BusinessCharacterOfflineException();

        public WorldChannel CurrentServer { get; }
        public WorldChannelServer CurrentServerContainer => CurrentServer.Container;

        public int Channel => CurrentServer.getId();
        public NPCConversationManager? NPCConversationManager { get; set; }

        public override int AccountId => AccountEntity?.Id ?? -2;

        public override string AccountName => AccountEntity?.Name ?? "";
        public override int AccountGMLevel => AccountEntity?.GMLevel ?? 0;
        public ClientCulture CurrentCulture { get; set; } = new ClientCulture();
        public override void SetCharacterOnSessionTransitionState(int cid)
        {
            IsServerTransition = true;
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
                try
                {
                    RemovePlayer(Character, IsServerTransition);

                    if (Channel != -1 && !isShutdown)
                    {
                        if (!IsServerTransition)
                        {
                            if (!fromCashShop)
                            {
                                Character.forfeitExpirableQuests();    //This is for those quests that you have to stay logged in for a certain amount of time

                                //if (guild != null)
                                //{
                                //    // 通过MasterServer广播
                                //    guild.setOnline(Character.Id, false, CurrentServer.getId());
                                //    // 都断开连接了这个包还有必要发？
                                //    Character.sendPacket(GuildPackets.showGuildInfo(Character));
                                //}
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
                    CurrentServerContainer.RemovePlayer(Character.Id);

                    if (!IsServerTransition)
                        Character.logOff();
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
                CurrentServerContainer.Transport.SendAccountLogout(AccountEntity.Id);
            }
            else
            {
                //比如切换频道  但是还没有成功进入新频道
                if (YamlConfig.config.server.USE_IP_VALIDATION && !CurrentServerContainer.HasCharacteridInTransition(GetSessionRemoteHost()))
                {
                    CurrentServerContainer.Transport.SendAccountLogout(AccountEntity.Id);
                    IsServerTransition = false;
                }
            }
            Dispose();

            _isDisconnecting = false;
        }

        /// <summary>
        /// 清理，取消各种玩家状态
        /// </summary>
        /// <param name="player"></param>
        /// <param name="serverTransition"></param>
        private static void RemovePlayer(IPlayer player, bool serverTransition)
        {
            player.cancelMagicDoor();
            player.setDisconnectedFromChannelWorld();
            player.cancelAllBuffs(true);
            player.cancelAllDebuffs();
            player.unregisterChairBuff();

            player.closePlayerInteractions();
            player.closePartySearchInteractions();

            ResourceManager.Cancel(player);

            if (!serverTransition)
            {
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
                    player.getChannelServer().Container.CharacterHpDecreaseManager.removePlayerHpDecrease(player);
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

            Disconnect(false);
        }

        protected override void ProcessPacket(InPacket packet)
        {
            short opcode = packet.readShort();
            var handler = _packetProcessor.GetPacketHandler(opcode);

            if (YamlConfig.config.server.USE_DEBUG_SHOW_RCVD_PACKET && !LoggingUtil.isIgnoredRecvPacket(opcode))
            {
                log.LogDebug("Received packet id {Code}", opcode);
            }

            if (handler != null)
            {
                if (handler.ValidateState(this))
                {
                    CurrentServerContainer.MonitorManager.LogPacketIfMonitored(this, opcode, packet.getBytes());
                    handler.HandlePacket(packet, this);
                }
            }
            else
            {
                log.LogError("没有找到{OPCode}对应的Handler", OpcodeConstants.recvOpcodeNames.GetValueOrDefault(opcode, opcode.ToString()));
                throw new BusinessNotsupportException();
            }
        }

        public void enableCSActions()
        {
            sendPacket(PacketCreator.enableCSUse(OnlinedCharacter));
        }

        AbstractPlayerInteraction? _pi;
        public AbstractPlayerInteraction getAbstractPlayerInteraction()
        {
            return _pi ??= new AbstractPlayerInteraction(this);
        }

        long lastNpcClick;
        public bool canClickNPC()
        {
            return lastNpcClick + 500 < CurrentServerContainer.getCurrentTime();
        }

        public void setClickedNPC()
        {
            lastNpcClick = CurrentServerContainer.getCurrentTime();
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
            if (!this.getChannelServer().Container.ServerMessageManager.registerDisabledServerMessage(OnlinedCharacter.getId()))
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
                long timeNow = CurrentServerContainer.getCurrentTime();
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
                Character.Pink(nameof(ClientMessage.ChangeChannel_MiniDungeon));
                sendPacket(PacketCreator.enableActions());
                return;
            }

            var socket = CurrentServer.GetChannelEndPoint(channel);
            if (socket == null)
            {
                Character.Popup(nameof(ClientMessage.ChangeChannel_ChannelDisabled), channel.ToString());
                sendPacket(PacketCreator.enableActions());
                return;
            }
            try
            {
                CurrentServer.Container.DataService.SaveBuff(Character);
                SetCharacterOnSessionTransitionState(Character.getId());
                Character.saveCharToDB(trigger: SyncCharacterTrigger.ChangeServer);
                sendPacket(PacketCreator.getChannelChange(socket));
            }
            catch (IOException e)
            {
                log.LogError(e.ToString());
            }
        }

        /// <summary>
        /// 离开商城
        /// </summary>
        /// <param name="c"></param>
        public void LeaveCashShop()
        {
            if (Character == null)
                return;

            if (!Character.getCashShop().isOpened())
            {
                Disconnect(false, false);
                return;
            }
            var socket = CurrentServer.getIP();
            if (socket == null)
            {
                enableCSActions();
                return;
            }
            Character.getCashShop().open(false);

            SetCharacterOnSessionTransitionState(Character.getId());
            Character.saveCharToDB(trigger: SyncCharacterTrigger.ChangeServer);
            sendPacket(PacketCreator.getChannelChange(socket));
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

        public void SetAccount(AccountCtrl accountEntity)
        {
            AccountEntity = accountEntity;

            CurrentServer.ClientStorage.Register(accountEntity.Id, this);
        }

        public WorldChannel getChannelServer() => CurrentServer;
        public int getChannel() => Channel;

        public EventManager? getEventManager(string @event)
        {
            return CurrentServer.getEventSM().getEventManager(@event);
        }

        public bool CheckBirthday(DateTime date)
        {
            return true;

            if (AccountEntity == null)
                return false;

            return date.Month == AccountEntity.Birthday.Month && date.Day == AccountEntity.Birthday.Day;
        }

        public bool CheckBirthday(int dateInt)
        {
            return true;

            if (DateTime.TryParseExact(dateInt.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var d))
                return CheckBirthday(d);
            return false;
        }
    }
}
