using Application.Shared.Message;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class DisconnectAllHandler : InternalSessionHandler<Empty>
    {
        public DisconnectAllHandler(WorldChannelServer server) : base(server)
        {
        }

        public override int MessageId => ChannelRecvCode.DisconnectAll;

        protected override Task HandleAsync(Empty message, CancellationToken cancellationToken = default)
        {
            foreach (var chr in _server.PlayerStorage.getAllCharacters())
            {
                if (!chr.isGM())
                {
                    chr.Client.Disconnect(false);
                }
            }
            return Task.CompletedTask;
        }

        protected override Empty Parse(ByteString content) => Empty.Parser.ParseFrom(content);
    }
}
