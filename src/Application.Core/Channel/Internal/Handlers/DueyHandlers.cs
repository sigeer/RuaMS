using Application.Core.Channel.Commands.Duey;
using Application.Core.Channel.DueyService;
using Application.Core.Channel.Net.Packets;
using Application.Core.Channel.Services;
using Application.Shared.Message;
using AutoMapper;
using DueyDto;
using Google.Protobuf;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;
using SystemProto;
using XmlWzReader;

namespace Application.Core.Channel.Internal.Handlers
{
    internal class DueyHandlers
    {
        internal class CreateHandler : InternalSessionChannelHandler<CreatePackageBroadcast>
        {
            public CreateHandler(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.CreateDueyPackage;

            protected override void HandleMessage(CreatePackageBroadcast data)
            {
                _server.PushChannelCommand(new InvokeDueyCreateCallbackCommand(data));
            }

            protected override CreatePackageBroadcast Parse(ByteString content) => CreatePackageBroadcast.Parser.ParseFrom(content);
        }

        internal class RemoveHandler : InternalSessionChannelHandler<RemovePackageResponse>
        {
            public RemoveHandler(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.DeleteDueyPackage;

            protected override void HandleMessage(RemovePackageResponse data)
            {
                _server.PushChannelCommand(new InvokeDueyRemoveCallbackCommand(data));
            }

            protected override RemovePackageResponse Parse(ByteString content) => RemovePackageResponse.Parser.ParseFrom(content);
        }

        internal class GetHandler : InternalSessionChannelHandler<GetPlayerDueyPackageResponse>
        {
            readonly IMapper _mapper;
            public GetHandler(WorldChannelServer server, IMapper mapper) : base(server)
            {
                _mapper = mapper;
            }

            public override int MessageId => (int)ChannelRecvCode.LoadDueyPackage;

            protected override void HandleMessage(GetPlayerDueyPackageResponse data)
            {
                _server.PushChannelCommand(new InvokeDueyOpenCallbackCommand(data));
            }

            protected override GetPlayerDueyPackageResponse Parse(ByteString content) => GetPlayerDueyPackageResponse.Parser.ParseFrom(content);
        }

        internal class TakeHandler : InternalSessionChannelHandler<TakeDueyPackageResponse>
        {
            readonly IMapper _mapper;
            readonly IItemDistributeService _distributeService;
            readonly ILogger<TakeHandler> _logger;
            public TakeHandler(WorldChannelServer server, IMapper mapper, IItemDistributeService itemDistributeService, ILogger<TakeHandler> logger) : base(server)
            {
                _mapper = mapper;
                _distributeService = itemDistributeService;
                _logger = logger;
            }

            public override int MessageId => (int)ChannelRecvCode.TakeDueyPackage;

            protected override void HandleMessage(TakeDueyPackageResponse data)
            {
                _server.PushChannelCommand(new InvokeDueyTakeCallbackCommand(data));
            }

            protected override TakeDueyPackageResponse Parse(ByteString content) => TakeDueyPackageResponse.Parser.ParseFrom(content);
        }

        internal class LoginNotifyHandler : InternalSessionChannelHandler<DueyNotificationDto>
        {
            public LoginNotifyHandler(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => (int)ChannelRecvCode.LoginNotifyDueyPackage;

            protected override void HandleMessage(DueyNotificationDto data)
            {
                _server.PushChannelCommand(new InvokeDueyNotifyCallbackCommand(data));
            }

            protected override DueyNotificationDto Parse(ByteString content) => DueyNotificationDto.Parser.ParseFrom(content);
        }
    }
}
