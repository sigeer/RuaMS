using Application.Core.Login.Services;
using Application.Shared.Message;
using Google.Protobuf;

namespace Application.Core.Login.Internal.Handlers
{
    internal class LoginHandlers
    {
        internal class CompletLoginHandler : InternalSessionMasterHandler<ProtoService.CompleteLoginRequest>
        {
            readonly LoginService _loginService;
            public CompletLoginHandler(MasterServer server, LoginService loginService) : base(server)
            {
                _loginService = loginService;
            }

            public override int MessageId => (int)ChannelSendCode.CompleteLogin;

            protected override Task HandleMessage(ProtoService.CompleteLoginRequest message)
            {
                return _loginService.SetPlayerLogedIn(message.CharacterId, message.Channel);
            }
            protected override ProtoService.CompleteLoginRequest Parse(ByteString data)
            {
                return ProtoService.CompleteLoginRequest.Parser.ParseFrom(data);
            }
        }
    }
}
