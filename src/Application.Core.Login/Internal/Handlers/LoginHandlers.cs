using Application.Core.Login.Services;
using Application.Shared.Message;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using ServiceProto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Login.Internal.Handlers
{
    internal class LoginHandlers
    {
        internal class CompletLoginHandler : InternalSessionMasterHandler<CompleteLoginRequest>
        {
            readonly LoginService _loginService;
            public CompletLoginHandler(MasterServer server, LoginService loginService) : base(server)
            {
                _loginService = loginService;
            }

            public override int MessageId => ChannelSendCode.CompleteLogin;

            protected override async Task HandleAsync(CompleteLoginRequest message, CancellationToken cancellationToken = default)
            {
                await _loginService.SetPlayerLogedIn(message.CharacterId, message.Channel);
            }
            protected override CompleteLoginRequest Parse(ByteString data)
            {
                return CompleteLoginRequest.Parser.ParseFrom(data);
            }
        }
    }
}
