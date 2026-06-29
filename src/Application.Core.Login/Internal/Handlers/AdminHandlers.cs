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

            protected override Task HandleMessage(SetGmLevelRequest message)
            {
                return _server.AccountManager.SetGmLevel(message);
            }

            protected override SetGmLevelRequest Parse(ByteString content) => SetGmLevelRequest.Parser.ParseFrom(content);
        }
        internal class BanHandler : InternalSessionMasterHandler<BanRequest>
        {
            public BanHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.Ban;

            protected override Task HandleMessage(BanRequest message)
            {
                return _server.AccountBanManager.Ban(message);
            }

            protected override BanRequest Parse(ByteString content) => BanRequest.Parser.ParseFrom(content);
        }

        internal class UnbanHandler : InternalSessionMasterHandler<UnbanRequest>
        {
            public UnbanHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.Unban;

            protected override Task HandleMessage(UnbanRequest message)
            {
                return _server.AccountBanManager.Unban(message);
            }

            protected override UnbanRequest Parse(ByteString content) => UnbanRequest.Parser.ParseFrom(content);
        }

        internal class WarpPlayerHandler : InternalSessionMasterHandler<WrapPlayerByNameRequest>
        {
            public WarpPlayerHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.WarpPlayer;

            protected override Task HandleMessage(WrapPlayerByNameRequest message)
            {
                return _server.CrossServerService.WarpPlayerByName(message);
            }

            protected override WrapPlayerByNameRequest Parse(ByteString content) => WrapPlayerByNameRequest.Parser.ParseFrom(content);
        }

        internal class SummonPlayerHandler : InternalSessionMasterHandler<SummonPlayerByNameRequest>
        {
            public SummonPlayerHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.SummonPlayer;

            protected override Task HandleMessage(SummonPlayerByNameRequest message)
            {
                return _server.CrossServerService.SummonPlayerByName(message);
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

            protected override Task HandleMessage(SendReportRequest message)
            {
                return _messageService.AddReport(message);
            }

            protected override SendReportRequest Parse(ByteString content) => SendReportRequest.Parser.ParseFrom(content);
        }

        internal class SetAutobanIgnoreHandler : InternalSessionMasterHandler<ToggleAutoBanIgnoreRequest>
        {
            public SetAutobanIgnoreHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.SetAutobanIgnore;

            protected override Task HandleMessage(ToggleAutoBanIgnoreRequest message)
            {
                return _server.SystemManager.ToggleAutoBanIgnored(message);
            }

            protected override ToggleAutoBanIgnoreRequest Parse(ByteString content) => ToggleAutoBanIgnoreRequest.Parser.ParseFrom(content);
        }

        internal class SetMonitorHandler : InternalSessionMasterHandler<ToggleMonitorPlayerRequest>
        {
            public SetMonitorHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.SetMonitor;

            protected override Task HandleMessage(ToggleMonitorPlayerRequest message)
            {
                return _server.SystemManager.ToggleMonitor(message);
            }

            protected override ToggleMonitorPlayerRequest Parse(ByteString content) => ToggleMonitorPlayerRequest.Parser.ParseFrom(content);
        }

        internal class ReloadWorldEventsHandler : InternalSessionMasterHandler<ReloadEventsRequest>
        {
            public ReloadWorldEventsHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.ReloadWorldEvents;

            protected override Task HandleMessage(ReloadEventsRequest message)
            {
                return _server.Transport.BroadcastMessageN(ChannelRecvCode.HandleWorldEventReload, message);
            }

            protected override ReloadEventsRequest Parse(ByteString content) => ReloadEventsRequest.Parser.ParseFrom(content);
        }

        internal class SetTimerHandler : InternalSessionMasterHandler<SetTimer>
        {
            public SetTimerHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.SetTimer;

            protected override Task HandleMessage(SetTimer message)
            {
                return _server.Transport.BroadcastMessageN(ChannelRecvCode.HandleSetTimer, message);
            }

            protected override SetTimer Parse(ByteString content) => SetTimer.Parser.ParseFrom(content);
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

        internal class JailHandler : InternalSessionMasterHandler<CreateJailRequest>
        {
            public JailHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.Jail;

            protected override Task HandleMessage(CreateJailRequest message)
            {
                return _server.CharacterManager.JailPlayer(message);
            }
            protected override CreateJailRequest Parse(ByteString content) => CreateJailRequest.Parser.ParseFrom(content);
        }

        internal class UnjailHandler : InternalSessionMasterHandler<CreateUnjailRequest>
        {
            public UnjailHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.Unjail;

            protected override Task HandleMessage(CreateUnjailRequest message)
            {
                return _server.CharacterManager.UnjailPlayer(message);
            }
            protected override CreateUnjailRequest Parse(ByteString content) => CreateUnjailRequest.Parser.ParseFrom(content);
        }

        internal class AntiMacroNotifyHandler : InternalSessionMasterHandler<AntiMacroNotifyMessage>
        {
            public AntiMacroNotifyHandler(MasterServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelSendCode.AntiMacroNotify;

            protected override Task HandleMessage(AntiMacroNotifyMessage message)
            {
                return _server.ProcessAntiMacroPenalty(message);
            }

            protected override AntiMacroNotifyMessage Parse(ByteString content) => AntiMacroNotifyMessage.Parser.ParseFrom(content);
        }
    }
}
