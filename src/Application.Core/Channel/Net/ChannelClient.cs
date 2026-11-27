using Application.Core.Channel.Commands;
using Application.Core.Game.Trades;
using Application.Resources.Messages;
using Application.Shared.Login;
using Application.Shared.MapObjects.Players;
using Application.Shared.Net.Logging;
using Application.Utility.Performance;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using scripting.npc;
using System.Runtime.ConstrainedExecution;
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
        public int CharacterId { get; private set; }

        public ITradeServer? TradeServer { get; set; }
        public WorldChannel CurrentServer { get; }

        public int Channel => CurrentServer.Id;
        public NPCConversationManager? NPCConversationManager { get; set; }

        public override int AccountId => AccountEntity?.Id ?? -2;

        public override string AccountName => AccountEntity?.Name ?? "";
        public override int AccountGMLevel => AccountEntity?.GMLevel ?? 0;
        public IClientCulture CurrentCulture { get; set; } = new ClientCulture(0);
        public override async Task SetCharacterOnSessionTransitionState(int cid)
        {
            IsServerTransition = true;

            await CurrentServer.Node.UpdateAccountState(AccountId, (sbyte)LoginStage.PlayerServerTransition);
            CurrentServer.Node.SetCharacteridInTransition(GetSessionRemoteHost(), CharacterId);
        }

        bool _isDisconnecting = false;
        public async Task Disconnect(bool isShutdown, bool fromCashShop = false)
        {
            if (_isDisconnecting)
                return;

            try
            {
                _isDisconnecting = true;

                if (!IsServerTransition)
                {
                    // 登出不能依赖 LeaveWorld 成功：它内部有多个可抛点（移出地图、事件副本、背包清理、存 buff），
                    // 一旦抛在这里而跳过 UpdateAccountState，账号会停在已登录态、角色在中心服一直是「在线」。
                    try
                    {
                        await LeaveWorld(false);
                    }
                    catch (Exception e)
                    {
                        log.LogError(e, "LeaveWorld failed");
                    }

                    await CurrentServer.Node.Transport.UpdateAccountState(AccountId, LoginStage.LOGIN_NOTLOGGEDIN);
                }
            }
            catch (Exception e)
            {
                log.LogError(e, "Account stuck");
            }
            finally
            {
                // 商城/拍卖行里的角色已经不在 Players 里，LeaveWorld 走过去也会直接返回，
                // 所以这里必须单独摘除，否则 PlayersAway 会一直留着这个已经断开的会话
                // （canUninstall 永远 false、在线数居高不下、关服时还会对已释放的 client 调 ForceDisconnect）。
                // 只摘自己这一条：同一角色的新会话可能已经重新登记，晚到的旧 client 不能把它删掉。
                if (CurrentServer.PlayersAway.TryRemove(new KeyValuePair<int, IChannelClient>(CharacterId, this)))
                {
                    AddOnlinePlayerCount(-1);
                }

                try
                {
                    await DisposeAsync();
                }
                finally
                {
                    // DisposeAsync 里可能再次触发 Disconnect（CloseSessionInternal），所以复位要放在它之后；
                    // 但它自己抛异常时也必须复位，否则这个 client 之后的所有 Disconnect 都会静默失效
                    _isDisconnecting = false;
                }
            }
        }

        /// <summary>
        /// 在线人数统计。加上 Channel 标签，和 <see cref="WorldChannel.addPlayer"/> 里的 +1 保持一致。
        /// </summary>
        void AddOnlinePlayerCount(int delta)
        {
            GameMetrics.OnlinePlayerCount.Add(delta, new KeyValuePair<string, object?>("Channel", CurrentServer.InstanceName));
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

            await Disconnect(false);
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, InPacket msg)
        {
            base.ChannelRead0(ctx, msg);
            if (Character == null || Character.IsInExtraServer())
            {
                CurrentServer.Send(w =>
                {
                    _ = ProcessPacketAsync(msg);
                });
            }
            else
            {
                Character.MapModel.Send(m =>
                {
                    _ = ProcessPacketAsync(msg);
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
                await Character.Pink(ClientMessage.ChangeChannel_MiniDungeon);
                await SendPacket(PacketCreator.enableActions());
                return;
            }

            var socket = CurrentServer.GetChannelEndPoint(channel);
            if (socket == null)
            {
                await Character.Popup(ClientMessage.ChangeChannel_ChannelDisabled, channel.ToString());
                await SendPacket(PacketCreator.enableActions());
                return;
            }

            await CurrentServer.Send(new PlayerPreEnterChannelCommand(Character.Id, socket, true));
        }

        public async Task<bool> EnterTradeServer(ITradeServer server)
        {
            // 没有角色时 CharacterId 为 0，写进 PlayersAway 会留下一条永远清不掉的脏数据。
            if (CharacterId == 0)
                return false;

            // 已经在商城/拍卖行里（或残留了旧会话），不能再挂第二份
            if (!CurrentServer.PlayersAway.TryAdd(CharacterId, this))
                return false;

            AddOnlinePlayerCount(1);
            TradeServer = server;

            // 进入商城/拍卖行等同于一次切服：之后会重新拉取角色数据，所以这里不注销
            // 事件副本、也不清理 ExpireOnLogout 物品（由 LeaveWorld 按 IsServerTransition 判断）。
            IsServerTransition = true;
            try
            {
                await LeaveWorld();
                return true;
            }
            catch
            {
                // 角色没能脱离世界就回滚，否则会留下「标记在商城、人还在世界里」的半完成状态
                if (CurrentServer.PlayersAway.TryRemove(CharacterId, out _))
                {
                    AddOnlinePlayerCount(-1);
                }
                TradeServer = null;

                // LeaveWorld 可能已经完成了一半（人已离开地图、但没进商城），这种会话不能留着：
                // 复位切服标记后断开客户端，让它走一次正常的登出清理（否则账号会停在已登录态、角色停在旧频道）
                IsServerTransition = false;
                await Disconnect(false, false);
                throw;
            }
            finally
            {
                IsServerTransition = false;
            }
        }

        /// <summary>
        /// 离开商城
        /// </summary>
        public async Task LeaveTradeServer()
        {
            if (TradeServer == null)
            {
                // 会话标记已经丢了（比如重连时拿到的是旧连接），这个客户端没有别的出路
                await Disconnect(false, false);
                return;
            }

            await CurrentServer.Send(new PlayerPreEnterChannelCommand(CharacterId, CurrentServer.getIP(), false));
        }

        /// <summary>
        /// 离开主世界（切换频道，进入商城，拍卖行）
        /// </summary>
        /// <param name="saveBuff">是否保存 buff（正常登出不保存）</param>
        public async Task LeaveWorld(bool saveBuff = true)
        {
            var chr = CurrentServer.Players.RemovePlayer(CharacterId);
            if (chr != null)
            {
                AddOnlinePlayerCount(-1);

                await chr.getMap().removePlayer(chr);

                if (!chr.Client.IsServerTransition)
                {
                    var eim = chr.getEventInstance();
                    if (eim != null)
                    {
                        await eim.playerDisconnected(chr);
                    }

                    await chr.Bag.ClearWhenLogout();
                }

                await chr.SyncCharAsync(trigger: Shared.Events.SyncCharacterTrigger.LeaveChannel);

                await chr.closePlayerInteractions();
                chr.closePartySearchInteractions();

                await chr.unregisterChairBuff();
                if (saveBuff)
                {
                    CurrentServer.NodeService.DataService.SaveBuff(chr);
                }

                await chr.CancelAllBuffs();
                chr.cancelAllDebuffs();
            }
        }


        public void SetPlayer(Player? player)
        {
            Character = player;
            CharacterId = Character?.Id ?? 0;
        }

        public void SetAccount(AccountInfoModel accountEntity)
        {
            AccountEntity = accountEntity;
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
