using Application.Core.Login;
using Application.Core.Login.Internal;
using Application.Module.Marriage.Common;
using Google.Protobuf;
using MarriageProto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Module.Marriage.Master.Internal
{
    internal class SpouseChatHandler : InternalSessionMasterHandler<MarriageProto.SendSpouseChatRequest>
    {
        readonly MarriageManager _marriageManager;
        public SpouseChatHandler(MasterServer server, MarriageManager marriageManager) : base(server)
        {
            _marriageManager = marriageManager;
        }

        public override int MessageId => ChannelSend.SpouseChat;

        protected override async Task HandleMessage(SendSpouseChatRequest res, CancellationToken cancellationToken = default)
        {
            await _marriageManager.SpouseChat(res);
        }

        protected override SendSpouseChatRequest Parse(ByteString data) => SendSpouseChatRequest.Parser.ParseFrom(data);
    }
}
