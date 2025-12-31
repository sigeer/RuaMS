using Application.Resources.Messages;
using Application.Shared.Internal;
using Application.Shared.Message;
using Config;
using Dto;
using Google.Protobuf;
using SystemProto;
using tools;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class AdminHandlers
    {
        public class ReloadWorldEvents : InternalSessionChannelHandler<ReloadEventsResponse>
        {
            public ReloadWorldEvents(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => ChannelRecvCode.HandleWorldEventReload;

            protected override Task HandleAsync(ReloadEventsResponse res, CancellationToken cancellationToken = default)
            {
                IPlayer? sender = null;
                foreach (var ch in _server.Servers.Values)
                {
                    ch.reloadEventScriptManager();

                    if (sender == null)
                    {
                        sender = ch.Players.getCharacterById(res.Request.MasterId);
                        sender?.dropMessage(5, "Reloaded Events");
                    }
                }
                return Task.CompletedTask;
            }

            protected override ReloadEventsResponse Parse(ByteString data) => ReloadEventsResponse.Parser.ParseFrom(data);
        }

        public class SummonPlayer : InternalSessionChannelHandler<SummonPlayerByNameResponse>
        {
            public SummonPlayer(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => ChannelRecvCode.SummonPlayer;

            protected override Task HandleAsync(SummonPlayerByNameResponse res, CancellationToken cancellationToken = default)
            {
                var chr = _server.FindPlayerById(res.Request.MasterId);
                if (chr != null)
                {
                    if (res.Code != 0)
                    {
                        chr.Yellow(nameof(ClientMessage.PlayerNotOnlined), res.Request.Victim);
                        return Task.CompletedTask;
                    }
                }

                var summoned = _server.FindPlayerById(res.VictimId);
                if (summoned != null)
                {
                    if (summoned.getEventInstance() == null)
                    {
                        _server.AdminService.WarpPlayerByName(summoned, res.WarpToName);
                    }
                }
                return Task.CompletedTask;
            }

            protected override SummonPlayerByNameResponse Parse(ByteString data) => SummonPlayerByNameResponse.Parser.ParseFrom(data);
        }

        public class WarpPlayer : InternalSessionChannelHandler<WrapPlayerByNameResponse>
        {
            public WarpPlayer(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => ChannelRecvCode.WarpPlayer;

            protected override Task HandleAsync(WrapPlayerByNameResponse res, CancellationToken cancellationToken = default)
            {
                var chr = _server.FindPlayerById(res.Request.MasterId);
                if (chr != null)
                {
                    if (res.Code != 0)
                    {
                        chr.Yellow(nameof(ClientMessage.PlayerNotOnlined), res.Request.Victim);
                    }
                    else
                    {
                        chr.Client.ChangeChannel(res.TargetChannel);
                    }
                }
                return Task.CompletedTask;
            }

            protected override WrapPlayerByNameResponse Parse(ByteString data) => WrapPlayerByNameResponse.Parser.ParseFrom(data);
        }

        public class MonitorChanged : InternalSessionChannelHandler<ToggleMonitorPlayerResponse>
        {
            public MonitorChanged(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => ChannelRecvCode.InvokeMonitor;

            protected override Task HandleAsync(ToggleMonitorPlayerResponse res, CancellationToken cancellationToken = default)
            {
                var chr = _server.FindPlayerById(res.Request.MasterId);
                if (chr != null)
                {
                    if (res.Code == 0)
                    {
                        chr.yellowMessage(res.Request.TargetName + " is " + (res.IsMonitored ? "now being monitored." : "no longer being monitored."));
                    }
                    else
                    {
                        chr.dropMessage($"未找到玩家：{res.Request.TargetName}");
                    }
                }
                return Task.CompletedTask;
            }

            protected override ToggleMonitorPlayerResponse Parse(ByteString data) => ToggleMonitorPlayerResponse.Parser.ParseFrom(data);
        }

        public class Ban : InternalSessionChannelHandler<BanResponse>
        {
            public Ban(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => ChannelRecvCode.BanPlayer;

            protected override Task HandleAsync(BanResponse data, CancellationToken cancellationToken = default)
            {
                var masterChr = _server.FindPlayerById(data.Request.OperatorId);
                if (masterChr != null)
                {
                    if (data.Code != 0)
                    {
                        masterChr.sendPacket(PacketCreator.getGMEffect(6, 1));
                    }
                    else
                    {
                        masterChr.sendPacket(PacketCreator.getGMEffect(4, 0));
                    }
                }


                var chr = _server.FindPlayerById(data.VictimId);
                if (chr != null)
                {
                    chr.Yellow(nameof(ClientMessage.Ban_NoticePlayer), data.OperatorName);
                    chr.yellowMessage(chr.GetMessageByKey(ClientMessage.BanReason) + data.Request.ReasonDesc);

                    Timer? timer = null;
                    timer = new System.Threading.Timer(_ =>
                    {
                        chr.Client.CloseSession();

                        timer?.Dispose();
                    }, null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
                }
                return Task.CompletedTask;
            }

            protected override BanResponse Parse(ByteString data) => BanResponse.Parser.ParseFrom(data);
        }

        public class Unban : InternalSessionChannelHandler<UnbanResponse>
        {
            public Unban(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => ChannelRecvCode.Unban;

            protected override Task HandleAsync(UnbanResponse res, CancellationToken cancellationToken = default)
            {
                var masterChr = _server.FindPlayerById(res.Request.OperatorId);
                if (masterChr != null)
                {
                    if (res.Code == 0)
                    {
                        masterChr.message("Unbanned " + res.Request.Victim);
                    }
                }


                return Task.CompletedTask;
            }

            protected override UnbanResponse Parse(ByteString data) => UnbanResponse.Parser.ParseFrom(data);
        }
    }
}
