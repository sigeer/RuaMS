using Application.Shared.Internal;
using Application.Shared.Message;
using Dto;
using Google.Protobuf;
using LifeProto;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Login.Internal.Handlers
{
    internal class PLifeHandlers
    {
        internal class CreateHandler : InternalSessionMasterHandler<CreatePLifeRequest>
        {
            public CreateHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.CreatePLife;

            protected override void HandleMessage(CreatePLifeRequest message)
            {
                _ = _server.ResourceDataManager.CreatePLife(message);
            }

            protected override CreatePLifeRequest Parse(ByteString content) => CreatePLifeRequest.Parser.ParseFrom(content);
        }

        internal class RemoveHandler : InternalSessionMasterHandler<RemovePLifeRequest>
        {
            public RemoveHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.RemovePLife;

            protected override void HandleMessage(RemovePLifeRequest message)
            {
                _ = _server.ResourceDataManager.RemovePLife(message);
            }

            protected override RemovePLifeRequest Parse(ByteString content) => RemovePLifeRequest.Parser.ParseFrom(content);
        }
    }
}
