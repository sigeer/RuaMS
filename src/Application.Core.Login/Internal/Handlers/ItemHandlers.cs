using Application.Core.Login.Services;
using Application.Shared.Message;
using Google.Protobuf;
using ItemProto;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Text;
using SystemProto;

namespace Application.Core.Login.Internal.Handlers
{
    internal class ItemHandlers
    {
        internal class UseItemTVHandler : InternalSessionMasterHandler<CreateTVMessageRequest>
        {
            readonly ItemService _service;
            public UseItemTVHandler(MasterServer server, ItemService itemService) : base(server)
            {
                _service = itemService;
            }

            public override int MessageId => (int)ChannelSendCode.UseItemTV;

            protected override async Task HandleAsync(CreateTVMessageRequest message, CancellationToken cancellationToken = default)
            {
                await _service.BroadcastTV(message);
            }

            protected override CreateTVMessageRequest Parse(ByteString content) => CreateTVMessageRequest.Parser.ParseFrom(content);
        }

        internal class UseItemMegaphoneHandler : InternalSessionMasterHandler<UseItemMegaphoneRequest>
        {
            readonly ItemService _service;
            public UseItemMegaphoneHandler(MasterServer server, ItemService itemService) : base(server)
            {
                _service = itemService;
            }

            public override int MessageId => (int)ChannelSendCode.UseItemMegaphone;

            protected override async Task HandleAsync(UseItemMegaphoneRequest message, CancellationToken cancellationToken = default)
            {
                await _service.BroadcastItemMegaphone(message);
            }

            protected override UseItemMegaphoneRequest Parse(ByteString content) => UseItemMegaphoneRequest.Parser.ParseFrom(content);
        }
    }
}
