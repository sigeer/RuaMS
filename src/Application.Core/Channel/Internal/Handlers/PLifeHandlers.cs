using Application.Shared.Internal;
using Application.Shared.Message;
using Dto;
using Google.Protobuf;
using LifeProto;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class PLifeHandlers
    {

        public class Create : InternalSessionChannelHandler<CreatePLifeRequest>
        {
            public Create(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnPlifeCreated;

            protected override Task HandleAsync(CreatePLifeRequest res, CancellationToken cancellationToken = default)
            {
                _server.DataService.OnPLifeCreated(res);
                return Task.CompletedTask;
            }

            protected override CreatePLifeRequest Parse(ByteString data) => CreatePLifeRequest.Parser.ParseFrom(data);
        }

        public class Remove : InternalSessionChannelHandler<RemovePLifeResponse>
        {
            public Remove(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.OnPlifeRemoved;

            protected override Task HandleAsync(RemovePLifeResponse res, CancellationToken cancellationToken = default)
            {
                _server.DataService.OnPLifeRemoved(res);
                return Task.CompletedTask;
            }

            protected override RemovePLifeResponse Parse(ByteString data) => RemovePLifeResponse.Parser.ParseFrom(data);
        }
    }
}
