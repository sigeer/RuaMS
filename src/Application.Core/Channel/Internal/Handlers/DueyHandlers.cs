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
        internal class CreateHandler : InternalSessionChannelHandler<CreatePackageResponse>
        {
            public CreateHandler(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => ChannelRecvCode.CreateDueyPackage;

            protected override Task HandleAsync(CreatePackageResponse data, CancellationToken cancellationToken = default)
            {
                var dueyResponseCode = (SendDueyItemResponseCode)data.Code;
                var chr = _server.FindPlayerById(data.Request.SenderId);
                if (chr != null)
                {
                    if (dueyResponseCode == SendDueyItemResponseCode.Success)
                    {
                        chr.sendPacket(DueyPacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_SEND_SUCCESSFULLY_SENT.getCode()));
                    }
                    if (dueyResponseCode == SendDueyItemResponseCode.SameAccount)
                    {
                        chr.sendPacket(DueyPacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_SEND_SAMEACC_ERROR.getCode()));
                    }
                    if (dueyResponseCode == SendDueyItemResponseCode.CharacterNotExisted)
                    {
                        chr.sendPacket(DueyPacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_SEND_NAME_DOES_NOT_EXIST.getCode()));
                    }
                }

                if (dueyResponseCode == SendDueyItemResponseCode.Success)
                {
                    var receiver = _server.FindPlayerById(data.Package.ReceiverId);
                    if (receiver != null)
                    {
                        receiver.sendPacket(DueyPacketCreator.sendDueyParcelReceived(data.Package.SenderName, data.Package.Type));
                    }
                }

                return Task.CompletedTask;
            }

            protected override CreatePackageResponse Parse(ByteString content) => CreatePackageResponse.Parser.ParseFrom(content);
        }

        internal class RemoveHandler : InternalSessionChannelHandler<RemovePackageResponse>
        {
            public RemoveHandler(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => ChannelRecvCode.DeleteDueyPackage;

            protected override Task HandleAsync(RemovePackageResponse data, CancellationToken cancellationToken = default)
            {
                if (data.Code == 0)
                {
                    var chr = _server.FindPlayerById(data.Request.MasterId);
                    if (chr != null)
                        chr.sendPacket(DueyPacketCreator.removeItemFromDuey(!data.Request.ByReceived, data.Request.PackageId));
                }

                return Task.CompletedTask;
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

            public override int MessageId => ChannelRecvCode.LoadDueyPackage;

            protected override Task HandleAsync(GetPlayerDueyPackageResponse data, CancellationToken cancellationToken = default)
            {
                var chr = _server.FindPlayerById(data.ReceiverId);
                if (chr != null)
                {
                    var dataList = _mapper.Map<DueyPackageObject[]>(data.List);
                    chr.sendPacket(DueyPacketCreator.sendDuey(DueyProcessorActions.TOCLIENT_OPEN_DUEY.getCode(), dataList));
                }

                return Task.CompletedTask;
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

            public override int MessageId => ChannelRecvCode.TakeDueyPackage;

            protected override async Task HandleAsync(TakeDueyPackageResponse data, CancellationToken cancellationToken = default)
            {
                var chr = _server.FindPlayerById(data.Package.ReceiverId);
                if (chr == null)
                    return;

                if (data.Code == 0)
                {
                    var dp = _mapper.Map<DueyPackageObject>(data.Package);

                    var dpItem = dp.Item;

                    if (!chr.canHoldMeso(dp.Mesos))
                    {
                        chr.sendPacket(DueyPacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_RECV_UNKNOWN_ERROR.getCode()));
                        await _server.Transport.TakeDueyPackageCommit(new DueyDto.TakeDueyPackageCommit { MasterId = chr.Id, PackageId = dp.PackageId, Success = false });
                    }

                    if (dpItem != null)
                    {
                        if (!chr.CanHoldUniquesOnly(dpItem.getItemId()))
                        {
                            chr.sendPacket(DueyPacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_RECV_RECEIVER_WITH_UNIQUE.getCode()));
                            await _server.Transport.TakeDueyPackageCommit(new DueyDto.TakeDueyPackageCommit { MasterId = chr.Id, PackageId = dp.PackageId, Success = false });
                            return;
                        }

                        if (!chr.canHold(dpItem.getItemId(), dpItem.getQuantity()))
                        {
                            chr.sendPacket(DueyPacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_RECV_NO_FREE_SLOTS.getCode()));
                            await _server.Transport.TakeDueyPackageCommit(new DueyDto.TakeDueyPackageCommit { MasterId = chr.Id, PackageId = dp.PackageId, Success = false });
                            return;
                        }
                    }


                    _ = _server.Transport.TakeDueyPackageCommit(new DueyDto.TakeDueyPackageCommit { MasterId = chr.Id, PackageId = dp.PackageId, Success = true });
                    _distributeService.Distribute(chr, dpItem == null ? [] : [dpItem], dp.Mesos, 0, 0, "包裹满了");
                }
                else
                {
                    chr.sendPacket(DueyPacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_RECV_UNKNOWN_ERROR.getCode()));
                    if (data.Code == 1)
                    {
                        _logger.LogWarning("Chr {CharacterName} tried to receive package from duey with id {PackageId}", chr.Name, data.Request.PackageId);
                    }
                    if (data.Code == 2)
                    {
                        _logger.LogWarning("Chr {CharacterName} tried to receive package from duey with receiverId {PackageId}", chr.Name, data.Request.PackageId);
                    }
                }
            }

            protected override TakeDueyPackageResponse Parse(ByteString content) => TakeDueyPackageResponse.Parser.ParseFrom(content);
        }

        internal class LoginNotifyHandler : InternalSessionChannelHandler<DueyNotificationDto>
        {
            public LoginNotifyHandler(WorldChannelServer server) : base(server)
            {
            }

            public override int MessageId => ChannelRecvCode.LoginNotifyDueyPackage;

            protected override async Task HandleAsync(DueyNotificationDto data, CancellationToken cancellationToken = default)
            {
                var receiver = _server.FindPlayerById(data.ReceiverId);
                if (receiver != null)
                {
                    receiver.sendPacket(DueyPacketCreator.sendDueyParcelNotification(data.Type));
                }
            }

            protected override DueyNotificationDto Parse(ByteString content) => DueyNotificationDto.Parser.ParseFrom(content);
        }
    }
}
