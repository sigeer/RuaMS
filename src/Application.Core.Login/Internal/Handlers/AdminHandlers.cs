using Application.Core.Login.Services;
using Application.Shared.Message;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Application.Core.Login.Internal.Handlers
{
    internal class AdminHandlers
    {
        internal class SetGmLevelHandler : InternalSessionMasterHandler<ProtoService.SetGmLevelRequest>
        {
            public SetGmLevelHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.SetGmLevel;

            protected override Task HandleMessage(ProtoService.SetGmLevelRequest message)
            {
                return _server.AccountManager.SetGmLevel(message);
            }

            protected override ProtoService.SetGmLevelRequest Parse(ByteString content) => ProtoService.SetGmLevelRequest.Parser.ParseFrom(content);
        }
        internal class BanHandler : InternalSessionMasterHandler<ProtoService.BanRequest>
        {
            public BanHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.Ban;

            protected override Task HandleMessage(ProtoService.BanRequest message)
            {
                return _server.AccountBanManager.Ban(message);
            }

            protected override ProtoService.BanRequest Parse(ByteString content) => ProtoService.BanRequest.Parser.ParseFrom(content);
        }

        internal class UnbanHandler : InternalSessionMasterHandler<ProtoService.UnbanRequest>
        {
            public UnbanHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.Unban;

            protected override Task HandleMessage(ProtoService.UnbanRequest message)
            {
                return _server.AccountBanManager.Unban(message);
            }

            protected override ProtoService.UnbanRequest Parse(ByteString content) => ProtoService.UnbanRequest.Parser.ParseFrom(content);
        }

        internal class WarpPlayerHandler : InternalSessionMasterHandler<ProtoService.WrapPlayerByNameRequest>
        {
            public WarpPlayerHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.WarpPlayer;

            protected override Task HandleMessage(ProtoService.WrapPlayerByNameRequest message)
            {
                return _server.CrossServerService.WarpPlayerByName(message);
            }

            protected override ProtoService.WrapPlayerByNameRequest Parse(ByteString content) => ProtoService.WrapPlayerByNameRequest.Parser.ParseFrom(content);
        }

        internal class SummonPlayerHandler : InternalSessionMasterHandler<ProtoService.SummonPlayerByNameRequest>
        {
            public SummonPlayerHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.SummonPlayer;

            protected override Task HandleMessage(ProtoService.SummonPlayerByNameRequest message)
            {
                return _server.CrossServerService.SummonPlayerByName(message);
            }

            protected override ProtoService.SummonPlayerByNameRequest Parse(ByteString content) => ProtoService.SummonPlayerByNameRequest.Parser.ParseFrom(content);
        }

        internal class SendReportPlayerHandler : InternalSessionMasterHandler<ProtoService.SendReportRequest>
        {
            readonly ReportService _messageService;
            public SendReportPlayerHandler(MasterServer server, ReportService messageService) : base(server)
            {
                _messageService = messageService;
            }

            public override int MessageId => (int)ChannelSendCode.SendReport;

            protected override Task HandleMessage(ProtoService.SendReportRequest message)
            {
                return _messageService.AddReport(message);
            }

            protected override ProtoService.SendReportRequest Parse(ByteString content) => ProtoService.SendReportRequest.Parser.ParseFrom(content);
        }

        internal class SetAutobanIgnoreHandler : InternalSessionMasterHandler<ProtoService.ToggleAutoBanIgnoreRequest>
        {
            public SetAutobanIgnoreHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.SetAutobanIgnore;

            protected override Task HandleMessage(ProtoService.ToggleAutoBanIgnoreRequest message)
            {
                return _server.SystemManager.ToggleAutoBanIgnored(message);
            }

            protected override ProtoService.ToggleAutoBanIgnoreRequest Parse(ByteString content) => ProtoService.ToggleAutoBanIgnoreRequest.Parser.ParseFrom(content);
        }

        internal class SetMonitorHandler : InternalSessionMasterHandler<ProtoService.ToggleMonitorPlayerRequest>
        {
            public SetMonitorHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.SetMonitor;

            protected override Task HandleMessage(ProtoService.ToggleMonitorPlayerRequest message)
            {
                return _server.SystemManager.ToggleMonitor(message);
            }

            protected override ProtoService.ToggleMonitorPlayerRequest Parse(ByteString content) => ProtoService.ToggleMonitorPlayerRequest.Parser.ParseFrom(content);
        }

        internal class ReloadWorldEventsHandler : InternalSessionMasterHandler<ProtoService.ReloadEventsRequest>
        {
            public ReloadWorldEventsHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.ReloadWorldEvents;

            protected override Task HandleMessage(ProtoService.ReloadEventsRequest message)
            {
                return _server.Transport.BroadcastMessageN(ChannelRecvCode.HandleWorldEventReload, message);
            }

            protected override ProtoService.ReloadEventsRequest Parse(ByteString content) => ProtoService.ReloadEventsRequest.Parser.ParseFrom(content);
        }

        internal class SetTimerHandler : InternalSessionMasterHandler<ProtoModel.SetTimer>
        {
            public SetTimerHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.SetTimer;

            protected override Task HandleMessage(ProtoModel.SetTimer message)
            {
                return _server.Transport.BroadcastMessageN(ChannelRecvCode.HandleSetTimer, message);
            }

            protected override ProtoModel.SetTimer Parse(ByteString content) => ProtoModel.SetTimer.Parser.ParseFrom(content);
        }

        internal class RemoveTimerHandler : InternalSessionMasterEmptyHandler
        {
            public RemoveTimerHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.RemoveTimer;

            protected override Task HandleMessage(Empty message)
            {
                return _server.Transport.BroadcastMessageN(ChannelRecvCode.HandleRemoveTimer);
            }
        }

        internal class JailHandler : InternalSessionMasterHandler<ProtoService.CreateJailRequest>
        {
            public JailHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.Jail;

            protected override Task HandleMessage(ProtoService.CreateJailRequest message)
            {
                return _server.CharacterManager.JailPlayer(message);
            }
            protected override ProtoService.CreateJailRequest Parse(ByteString content) => ProtoService.CreateJailRequest.Parser.ParseFrom(content);
        }

        internal class UnjailHandler : InternalSessionMasterHandler<ProtoService.CreateUnjailRequest>
        {
            public UnjailHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.Unjail;

            protected override Task HandleMessage(ProtoService.CreateUnjailRequest message)
            {
                return _server.CharacterManager.UnjailPlayer(message);
            }
            protected override ProtoService.CreateUnjailRequest Parse(ByteString content) => ProtoService.CreateUnjailRequest.Parser.ParseFrom(content);
        }

        internal class AntiMacroNotifyHandler : InternalSessionMasterHandler<ProtoModel.AntiMacroNotifyMessageProto>
        {
            public AntiMacroNotifyHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.AntiMacroNotify;

            protected override Task HandleMessage(ProtoModel.AntiMacroNotifyMessageProto message)
            {
                return _server.ProcessAntiMacroPenalty(message);
            }

            protected override ProtoModel.AntiMacroNotifyMessageProto Parse(ByteString content) => ProtoModel.AntiMacroNotifyMessageProto.Parser.ParseFrom(content);
        }
    }
}
