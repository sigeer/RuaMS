using Application.Core.Game.Players;
using Application.Resources.Messages;
using Application.Shared.Internal;
using Application.Shared.Message;
using Config;
using Dto;
using Google.Protobuf;
using JailProto;
using System.Numerics;
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

            public override int MessageId => (int)ChannelRecvCode.HandleWorldEventReload;

            protected override Task HandleAsync(ReloadEventsResponse res, CancellationToken cancellationToken = default)
            {
                Player? sender = null;
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

            public override int MessageId => (int)ChannelRecvCode.SummonPlayer;

            protected override async Task HandleAsync(SummonPlayerByNameResponse res, CancellationToken cancellationToken = default)
            {
                var chr = _server.FindPlayerById(res.Request.MasterId);
                if (chr != null)
                {
                    if (res.Code != 0)
                    {
                        chr.Yellow(nameof(ClientMessage.PlayerNotOnlined), res.Request.Victim);
                        return;
                    }
                }

                var summoned = _server.FindPlayerById(res.VictimId);
                if (summoned != null)
                {
                    if (summoned.getEventInstance() == null)
                    {
                        await _server.AdminService.WarpPlayerByName(summoned, res.WarpToName);
                    }
                }
                return;
            }

            protected override SummonPlayerByNameResponse Parse(ByteString data) => SummonPlayerByNameResponse.Parser.ParseFrom(data);
        }

        public class WarpPlayer : InternalSessionChannelHandler<WrapPlayerByNameResponse>
        {
            public WarpPlayer(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.WarpPlayer;

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

            public override int MessageId => (int)ChannelRecvCode.InvokeMonitor;

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

            public override int MessageId => (int)ChannelRecvCode.BanPlayer;

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

            public override int MessageId => (int)ChannelRecvCode.Unban;

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

        public class Jail : InternalSessionChannelHandler<CreateJailResponse>
        {
            public Jail(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.Jail;

            protected override Task HandleAsync(CreateJailResponse res, CancellationToken cancellationToken = default)
            {
                var masterChr = _server.FindPlayerById(res.Request.MasterId);
                if (res.Code != 0)
                {
                    if (masterChr != null)
                    {
                        masterChr.Pink(nameof(ClientMessage.PlayerNotFound));
                    }
                    return Task.CompletedTask;
                }

                var targetChr = _server.FindPlayerById(res.TargetId);
                if (targetChr != null)
                {
                    targetChr.addJailExpirationTime(res.Request.Minutes * 60000);
                }

                if (masterChr!= null)
                {
                    if (res.IsExtend)
                        masterChr.Pink(nameof(ClientMessage.Jail_ExtendResult), res.Request.TargetName, res.Request.Minutes.ToString());
                    else
                        masterChr.Pink(nameof(ClientMessage.Jail_Result), res.Request.TargetName, res.Request.Minutes.ToString());
                }

                return Task.CompletedTask;
            }

            protected override CreateJailResponse Parse(ByteString data) => CreateJailResponse.Parser.ParseFrom(data);
        }

        public class Unjail : InternalSessionChannelHandler<CreateUnjailResponse>
        {
            public Unjail(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.Unjail;

            protected override Task HandleAsync(CreateUnjailResponse res, CancellationToken cancellationToken = default)
            {
                var masterChr = _server.FindPlayerById(res.Request.MasterId);
                if (res.Code != 0)
                {
                    if (masterChr != null)
                    {
                        if (res.Code == 1)
                        {
                            masterChr.Pink(nameof(ClientMessage.PlayerNotFound));
                        }
                        else if (res.Code == 2)
                        {
                            masterChr.Pink(nameof(ClientMessage.UnjailCommand_AlreadyFree));
                        }   
                    }
                    return Task.CompletedTask;
                }

                var targetChr = _server.FindPlayerById(res.TargetId);
                if (targetChr != null)
                {
                    targetChr.removeJailExpirationTime();
                    targetChr.Pink(nameof(ClientMessage.Unjail_Notify));
                }

                if (masterChr != null)
                {
                    masterChr.Yellow(nameof(ClientMessage.Command_Done), masterChr.getLastCommandMessage());
                }

                return Task.CompletedTask;
            }

            protected override CreateUnjailResponse Parse(ByteString data) => CreateUnjailResponse.Parser.ParseFrom(data);
        }
    }
}
