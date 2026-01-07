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

            protected override async Task HandleAsync(CreatePLifeRequest message, CancellationToken cancellationToken = default)
            {
                await _server.ResourceDataManager.CreatePLife(message);
            }

            protected override CreatePLifeRequest Parse(ByteString content) => CreatePLifeRequest.Parser.ParseFrom(content);
        }

        internal class RemoveHandler : InternalSessionMasterHandler<RemovePLifeRequest>
        {
            public RemoveHandler(MasterServer server) : base(server)
            { }

            public override int MessageId => (int)ChannelSendCode.RemovePLife;

            protected override async Task HandleAsync(RemovePLifeRequest message, CancellationToken cancellationToken = default)
            {
                await _server.ResourceDataManager.RemovePLife(message);
            }

            protected override RemovePLifeRequest Parse(ByteString content) => RemovePLifeRequest.Parser.ParseFrom(content);
        }
    }
}
