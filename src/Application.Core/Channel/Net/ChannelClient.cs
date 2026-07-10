using Application.Core.Channel.Commands;
using Application.Resources.Messages;
using Application.Shared.Net.Logging;
using Application.Utility.Performance;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using scripting.npc;
using tools;

namespace Application.Core.Channel.Net
{
    public class ChannelClient : ClientBase, IChannelClient
    {
        IPacketProcessor<IChannelClient> _packetProcessor;
        public ChannelClient(long sessionId, WorldChannel currentServer, IChannel nettyChannel, IPacketProcessor<IChannelClient> packetProcessor, ILogger<IClientBase> log)
            : base(sessionId, currentServer, nettyChannel, log)
        {
            CurrentServer = currentServer;
            _packetProcessor = packetProcessor;
        }

        public override bool IsOnlined => Character != null;

        public Player? Character { get; private set; }

        public Player OnlinedCharacter => Character ?? throw new BusinessCharacterOfflineException();

        public WorldChannel CurrentServer { get; }

        public int Channel => CurrentServer.getId();
        public NPCConversationManager? NPCConversationManager { get; set; }

        public override int AccountId => AccountEntity?.Id ?? -2;

        public override string AccountName => AccountEntity?.Name ?? "";
        public override int AccountGMLevel => AccountEntity?.GMLevel ?? 0;
        public ClientCulture CurrentCulture { get; set; } = new ClientCulture(0);
        public override void SetCharacterOnSessionTransitionState(int cid)
        {
            IsServerTransition = true;
        }

        bool _isDisconnecting = false;
        public async Task Disconnect(bool isShutdown, bool fromCashShop = false)
        {
            if (_isDisconnecting)
                return;

            _isDisconnecting = true;
            //once per Client instance
            if (Character != null && Character.isLoggedin() && Character.getClient() != null)
            {
                try
                {
                    await RemovePlayer(Character, IsServerTransition);

                    if (Channel != -1 && !isShutdown)
                    {
                        if (!IsServerTransition)
                        {
                            if (!fromCashShop)
                            {
                                await Character.forfeitExpirableQuests();    //This is for those quests that you have to stay logged in for a certain amount of time

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
                    await CurrentServer.RemovePlayerDeep(Character);

                    if (!IsServerTransition)
                        await Character.logOff();
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
                CurrentServer.Node.Transport.SendAccountLogout(AccountEntity.Id);
            }
            else
            {
                //比如切换频道  但是还没有成功进入新频道
                if (YamlConfig.config.server.USE_IP_VALIDATION && !CurrentServer.Node.HasCharacteridInTransition(GetSessionRemoteHost()))
                {
                    CurrentServer.Node.Transport.SendAccountLogout(AccountEntity.Id);
                    IsServerTransition = false;
                }
            }
            await DisposeAsync();

            _isDisconnecting = false;
        }

        /// <summary>
        /// 清理，取消各种玩家状态
        /// </summary>
        /// <param name="player"></param>
        /// <param name="serverTransition"></param>
        private static async Task RemovePlayer(Player player, bool serverTransition)
        {
            await player.cancelMagicDoor();
            await player.cancelAllBuffs(true);
            player.cancelAllDebuffs();
            await player.unregisterChairBuff();

            await player.closePlayerInteractions();
            player.closePartySearchInteractions();

            if (!serverTransition)
            {
                var eim = player.getEventInstance();
                if (eim != null)
                {
                    await eim.playerDisconnected(player);
                }

                await player.Bag.ClearWhenLogout();
            }

            if (player.getMap() != null)
            {
                int mapId = player.getMapId();
                await player.getMap().removePlayer(player);
                if (MapId.isDojo(mapId))
                {
                    await player.getChannelServer().freeDojoSectionIfEmpty(mapId);
                }
            }
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            // player hard reference removal thanks to Steve (kaito1410)
            if (this.Character != null)
            {
                await this.Character.DisposeAsync(); // clears schedules and stuff
            }

            this.AccountEntity = null;
            this.Hwid = null;
            this.Character = null;
        }



        public override async Task ForceDisconnect()
        {
            await Disconnect(true);
        }

        protected override async Task CloseSessionInternal()
        {
            if (AccountEntity == null)
                return;

            CurrentServer.ClientStorage.RemoveClient(AccountEntity!.Id);

            await Disconnect(false);
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, InPacket msg)
        {
            base.ChannelRead0(ctx, msg);
            if (Character == null || Character.getCashShop().isOpened())
            {
                CurrentServer.Send(w =>
                {
                    _ = ProcessPacket(msg);
                });
            }
            else
            {
                Character.MapModel.Send(m =>
                {
                    _ = ProcessPacket(msg);
                });
            }
        }
        public override async Task ProcessPacket(InPacket packet)
        {
            short opcode = packet.readShort();
            var handler = _packetProcessor.GetPacketHandler(opcode);

            if (YamlConfig.config.server.USE_DEBUG_SHOW_RCVD_PACKET && !LoggingUtil.isIgnoredRecvPacket(opcode))
            {
                log.LogDebug("Received packet id {Code}", opcode);
            }

            using var activity = GameMetrics.ActivitySource.StartActivity("ProcessPacket");
            activity?.SetTag("AccountId", AccountId);
            activity?.SetTag("Handler", handler?.ToString());

            if (handler != null)
            {
                if (handler.ValidateState(this))
                {
                    CurrentServer.NodeService.MonitorManager.LogPacketIfMonitored(this, opcode, packet.getBytes());
                    await handler.HandlePacket(packet, this);
                }
            }
            else
            {
                log.LogError("没有找到{OPCode}对应的Handler", OpcodeConstants.recvOpcodeNames.GetValueOrDefault(opcode, opcode.ToString()));
                throw new BusinessNotsupportException();
            }
        }

        public bool GainCharacterSlot()
        {
            return CurrentServer.Node.Transport.GainCharacterSlot(AccountId);
        }

        public async Task ChangeChannel(int channel)
        {
            if (Character == null)
                return;

            if (Character.isBanned())
            {
                await Disconnect(false, false);
                return;
            }
            if (!Character.isAlive() || FieldLimit.CANNOTMIGRATE.check(Character.getMap().getFieldLimit()))
            {
                await SendPacket(PacketCreator.enableActions());
                return;
            }
            else if (MiniDungeonInfo.isDungeonMap(Character.getMapId()))
            {
                await Character.Pink(nameof(ClientMessage.ChangeChannel_MiniDungeon));
                await SendPacket(PacketCreator.enableActions());
                return;
            }

            var socket = CurrentServer.GetChannelEndPoint(channel);
            if (socket == null)
            {
                await Character.Popup(nameof(ClientMessage.ChangeChannel_ChannelDisabled), channel.ToString());
                await SendPacket(PacketCreator.enableActions());
                return;
            }

            try
            {
                await Character.SyncCharAsync(trigger: Shared.Events.SyncCharacterTrigger.PreEnterChannel);
                await CurrentServer.Send(new PlayerPreEnterChannelCommand(Character.Id, socket, true));
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
        public async Task LeaveCashShop()
        {
            if (Character == null)
                return;

            if (!Character.getCashShop().isOpened())
            {
                await Disconnect(false, false);
                return;
            }
            var socket = CurrentServer.getIP();
            if (socket == null)
            {
                await OnlinedCharacter.enableCSActions();
                return;
            }
            Character.getCashShop().open(false);

            await Character.SyncCharAsync(trigger: Shared.Events.SyncCharacterTrigger.PreEnterChannel);
            await CurrentServer.Send(new PlayerPreEnterChannelCommand(Character.Id, socket, false));
        }

        public void SetPlayer(Player? player)
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
