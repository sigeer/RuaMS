using Application.Core.Channel.Commands;
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

            protected override void HandleMessage(ReloadEventsResponse res)
            {
                _server.PushChannelCommand(new InvokeReloadEventsCommand());
                _server.PushChannelCommand(new InvokeDropMessageCommand(res.Request.MasterId, 5, "Reloaded Events"));
            }

            protected override ReloadEventsResponse Parse(ByteString data) => ReloadEventsResponse.Parser.ParseFrom(data);
        }

        public class SummonPlayer : InternalSessionChannelHandler<SummonPlayerByNameResponse>
        {
            public SummonPlayer(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.SummonPlayer;

            protected override void HandleMessage(SummonPlayerByNameResponse res)
            {
                _server.PushChannelCommand(new InvokeSummonPlayerCommand(res));
            }

            protected override SummonPlayerByNameResponse Parse(ByteString data) => SummonPlayerByNameResponse.Parser.ParseFrom(data);
        }

        public class WarpPlayer : InternalSessionChannelHandler<WrapPlayerByNameResponse>
        {
            public WarpPlayer(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.WarpPlayer;

            protected override void HandleMessage(WrapPlayerByNameResponse res)
            {
                _server.PushChannelCommand(new InvokeWarpPlayerCommand(res));
            }

            protected override WrapPlayerByNameResponse Parse(ByteString data) => WrapPlayerByNameResponse.Parser.ParseFrom(data);
        }

        public class MonitorChanged : InternalSessionChannelHandler<ToggleMonitorPlayerResponse>
        {
            public MonitorChanged(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.InvokeMonitor;

            protected override void HandleMessage(ToggleMonitorPlayerResponse res)
            {
                if (res.Code == 0)
                {
                    _server.PushChannelCommand(
                        new InvokeDropMessageCommand(res.Request.MasterId, -1, res.Request.TargetName + " is " + (res.IsMonitored ? "now being monitored." : "no longer being monitored.")));
                }
                else
                    _server.PushChannelCommand(
                        new InvokeDropMessageCommand(res.Request.MasterId, 5, $"未找到玩家：{res.Request.TargetName}"));
            }

            protected override ToggleMonitorPlayerResponse Parse(ByteString data) => ToggleMonitorPlayerResponse.Parser.ParseFrom(data);
        }

        public class Ban : InternalSessionChannelHandler<BanResponse>
        {
            public Ban(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.BanPlayer;

            protected override void HandleMessage(BanResponse data)
            {
                _server.PushChannelCommand(new InvokeBanPlayerCallbackCommand(data));
            }

            protected override BanResponse Parse(ByteString data) => BanResponse.Parser.ParseFrom(data);
        }

        public class Unban : InternalSessionChannelHandler<UnbanResponse>
        {
            public Unban(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.Unban;

            protected override void HandleMessage(UnbanResponse res)
            {
                if (res.Code == 0)
                {
                    _server.PushChannelCommand(new InvokeDropMessageCommand(res.Request.OperatorId, 5, "Unbanned " + res.Request.Victim));
                }
            }

            protected override UnbanResponse Parse(ByteString data) => UnbanResponse.Parser.ParseFrom(data);
        }

        public class Jail : InternalSessionChannelHandler<CreateJailResponse>
        {
            public Jail(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.Jail;

            protected override void HandleMessage(CreateJailResponse res)
            {
                _server.PushChannelCommand(new InvokeJailCallbackCommand(res));
            }

            protected override CreateJailResponse Parse(ByteString data) => CreateJailResponse.Parser.ParseFrom(data);
        }

        public class Unjail : InternalSessionChannelHandler<CreateUnjailResponse>
        {
            public Unjail(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.Unjail;

            protected override void HandleMessage(CreateUnjailResponse res)
            {
                _server.PushChannelCommand(new InvokeUnjailCallbackCommand(res));
            }

            protected override CreateUnjailResponse Parse(ByteString data) => CreateUnjailResponse.Parser.ParseFrom(data);
        }
    }
}
