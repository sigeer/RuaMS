using Application.Core.Login.Services;
using Application.Shared.Message;
using Config;
using Dto;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using JailProto;
using MessageProto;
using SystemProto;

namespace Application.Core.Login.Internal.Handlers
{
    internal class AdminHandlers
    {
        internal class SetGmLevelHandler : InternalSessionMasterHandler<SetGmLevelRequest>
        {
            public SetGmLevelHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.SetGmLevel;

            protected override void HandleMessage(SetGmLevelRequest message)
            {
                _ = _server.AccountManager.SetGmLevel(message);
            }

            protected override SetGmLevelRequest Parse(ByteString content) => SetGmLevelRequest.Parser.ParseFrom(content);
        }
        internal class BanHandler : InternalSessionMasterHandler<BanRequest>
        {
            public BanHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.Ban;

            protected override void HandleMessage(BanRequest message)
            {
                _ = _server.AccountBanManager.Ban(message);
            }

            protected override BanRequest Parse(ByteString content) => BanRequest.Parser.ParseFrom(content);
        }

        internal class UnbanHandler : InternalSessionMasterHandler<UnbanRequest>
        {
            public UnbanHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.Unban;

            protected override void HandleMessage(UnbanRequest message)
            {
                _ = _server.AccountBanManager.Unban(message);
            }

            protected override UnbanRequest Parse(ByteString content) => UnbanRequest.Parser.ParseFrom(content);
        }

        internal class WarpPlayerHandler : InternalSessionMasterHandler<WrapPlayerByNameRequest>
        {
            public WarpPlayerHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.WarpPlayer;

            protected override void HandleMessage(WrapPlayerByNameRequest message)
            {
                _ = _server.CrossServerService.WarpPlayerByName(message);
            }

            protected override WrapPlayerByNameRequest Parse(ByteString content) => WrapPlayerByNameRequest.Parser.ParseFrom(content);
        }

        internal class SummonPlayerHandler : InternalSessionMasterHandler<SummonPlayerByNameRequest>
        {
            public SummonPlayerHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.SummonPlayer;

            protected override void HandleMessage(SummonPlayerByNameRequest message)
            {
                _ = _server.CrossServerService.SummonPlayerByName(message);
            }

            protected override SummonPlayerByNameRequest Parse(ByteString content) => SummonPlayerByNameRequest.Parser.ParseFrom(content);
        }

        internal class SendReportPlayerHandler : InternalSessionMasterHandler<SendReportRequest>
        {
            readonly MessageService _messageService;
            public SendReportPlayerHandler(MasterServer server, MessageService messageService) : base(server)
            {
                _messageService = messageService;
            }

            public override int MessageId => (int)ChannelSendCode.SendReport;

            protected override void HandleMessage(SendReportRequest message)
            {
                _ = _messageService.AddReport(message);
            }

            protected override SendReportRequest Parse(ByteString content) => SendReportRequest.Parser.ParseFrom(content);
        }

        internal class SetAutobanIgnoreHandler : InternalSessionMasterHandler<ToggleAutoBanIgnoreRequest>
        {
            public SetAutobanIgnoreHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.SetAutobanIgnore;

            protected override void HandleMessage(ToggleAutoBanIgnoreRequest message)
            {
                _ = _server.SystemManager.ToggleAutoBanIgnored(message);
            }

            protected override ToggleAutoBanIgnoreRequest Parse(ByteString content) => ToggleAutoBanIgnoreRequest.Parser.ParseFrom(content);
        }

        internal class SetMonitorHandler : InternalSessionMasterHandler<ToggleMonitorPlayerRequest>
        {
            public SetMonitorHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.SetMonitor;

            protected override void HandleMessage(ToggleMonitorPlayerRequest message)
            {
                _ = _server.SystemManager.ToggleMonitor(message);
            }

            protected override ToggleMonitorPlayerRequest Parse(ByteString content) => ToggleMonitorPlayerRequest.Parser.ParseFrom(content);
        }

        internal class ReloadWorldEventsHandler : InternalSessionMasterHandler<ReloadEventsRequest>
        {
            public ReloadWorldEventsHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.ReloadWorldEvents;

            protected override void HandleMessage(ReloadEventsRequest message)
            {
                _ = _server.Transport.BroadcastMessageN(ChannelRecvCode.HandleWorldEventReload, message);
            }

            protected override ReloadEventsRequest Parse(ByteString content) => ReloadEventsRequest.Parser.ParseFrom(content);
        }

        internal class SetTimerHandler : InternalSessionMasterHandler<SetTimer>
        {
            public SetTimerHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.SetTimer;

            protected override void HandleMessage(SetTimer message)
            {
                _ = _server.Transport.BroadcastMessageN(ChannelRecvCode.HandleSetTimer, message);
            }

            protected override SetTimer Parse(ByteString content) => SetTimer.Parser.ParseFrom(content);
        }

        internal class RemoveTimerHandler : InternalSessionMasterEmptyHandler
        {
            public RemoveTimerHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.RemoveTimer;

            protected override void HandleMessage(Empty message)
            {
                _ = _server.Transport.BroadcastMessageN(ChannelRecvCode.HandleRemoveTimer);
            }
        }

        internal class JailHandler : InternalSessionMasterHandler<CreateJailRequest>
        {
            public JailHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.Jail;

            protected override void HandleMessage(CreateJailRequest message)
            {
                _ = _server.CharacterManager.JailPlayer(message);
            }
            protected override CreateJailRequest Parse(ByteString content) => CreateJailRequest.Parser.ParseFrom(content);
        }

        internal class UnjailHandler : InternalSessionMasterHandler<CreateUnjailRequest>
        {
            public UnjailHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.Unjail;

            protected override void HandleMessage(CreateUnjailRequest message)
            {
                _ = _server.CharacterManager.UnjailPlayer(message);
            }
            protected override CreateUnjailRequest Parse(ByteString content) => CreateUnjailRequest.Parser.ParseFrom(content);
        }
    }
}
