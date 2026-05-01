using Application.Core.Channel.DueyService;
using Application.Core.Channel.Net.Packets;
using Application.Core.Channel.Services;
using Application.Shared.Message;
using AutoMapper;
using DueyDto;
using Google.Protobuf;
using Microsoft.Extensions.Logging;

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
                _server.Broadcast(w =>
                {
                    w.getPlayerStorage().GetCharacterActor(data.Package.ReceiverId)?
                    .Send(m =>
                    {
                        m.getCharacterById(data.Package.ReceiverId)?.sendPacket(DueyPacketCreator.sendDueyParcelReceived(data.Package.SenderName, data.Package.Type));
                    });
                });
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
                if (data.Code == 0)
                {
                    _server.Broadcast(w =>
                    {
                        if (data.Code == 0)
                        {
                            var chr = w.getPlayerStorage().GetCharacterClientById(data.Request.MasterId);
                            if (chr != null)
                                chr.sendPacket(DueyPacketCreator.removeItemFromDuey(!data.Request.ByReceived, data.Request.PackageId));
                        }
                    });
                }
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
                _server.Broadcast(w =>
                {
                    w.getPlayerStorage().GetCharacterActor(data.ReceiverId)?
                        .Send(m =>
                        {
                            var chr = m.getCharacterById(data.ReceiverId);
                            if (chr != null)
                            {
                                var dataList = w.Mapper.Map<DueyPackageObject[]>(data.List);
                                chr.sendPacket(DueyPacketCreator.sendDuey(DueyProcessorActions.TOCLIENT_OPEN_DUEY.getCode(), dataList));
                            }
                        });
                });
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
                _server.Broadcast(w =>
                {
                    w.getPlayerStorage().GetCharacterActor(data.Package.ReceiverId)?.Send(m =>
                    {
                        var chr = m.getCharacterById(data.Package.ReceiverId);
                        if (chr != null)
                        {
                            if (data.Code == 0)
                            {
                                var dp = w.Mapper.Map<DueyPackageObject>(data.Package);

                                var dpItem = dp.Item;

                                if (!chr.canHoldMeso(dp.Mesos))
                                {
                                    chr.sendPacket(DueyPacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_RECV_UNKNOWN_ERROR.getCode()));
                                    _server.Transport.TakeDueyPackageCommit(new DueyDto.TakeDueyPackageCommit { MasterId = chr.Id, PackageId = dp.PackageId, Success = false });
                                    return;
                                }

                                if (dpItem != null)
                                {
                                    if (!chr.CanHoldUniquesOnly(dpItem.getItemId()))
                                    {
                                        chr.sendPacket(DueyPacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_RECV_RECEIVER_WITH_UNIQUE.getCode()));
                                        _server.Transport.TakeDueyPackageCommit(new DueyDto.TakeDueyPackageCommit { MasterId = chr.Id, PackageId = dp.PackageId, Success = false });
                                        return;
                                    }

                                    if (!chr.canHold(dpItem.getItemId(), dpItem.getQuantity()))
                                    {
                                        chr.sendPacket(DueyPacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_RECV_NO_FREE_SLOTS.getCode()));
                                        _server.Transport.TakeDueyPackageCommit(new DueyDto.TakeDueyPackageCommit { MasterId = chr.Id, PackageId = dp.PackageId, Success = false });
                                        return;
                                    }
                                }


                                _server.Transport.TakeDueyPackageCommit(new DueyDto.TakeDueyPackageCommit { MasterId = chr.Id, PackageId = dp.PackageId, Success = false });
                                _server.ItemDistributeService.Distribute(chr, dpItem == null ? [] : [dpItem], dp.Mesos, 0, 0, "包裹满了");
                            }
                            else
                            {
                                chr.sendPacket(DueyPacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_RECV_UNKNOWN_ERROR.getCode()));
                                if (data.Code == 1)
                                {
                                    chr.Log.Warning("Chr {CharacterName} tried to receive package from duey with id {PackageId}", chr.Name, data.Request.PackageId);
                                }
                                if (data.Code == 2)
                                {
                                    chr.Log.Warning("Chr {CharacterName} tried to receive package from duey with receiverId {PackageId}", chr.Name, data.Request.PackageId);
                                }
                            }
                        }
                    });
                });
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
                _server.Broadcast(w =>
                {
                    w.getPlayerStorage().GetCharacterActor(data.ReceiverId)?
                    .Send(m =>
                    {
                        m.getCharacterById(data.ReceiverId)?.sendPacket(DueyPacketCreator.sendDueyParcelNotification(data.Type));
                    });
                });
            }

            protected override DueyNotificationDto Parse(ByteString content) => DueyNotificationDto.Parser.ParseFrom(content);
        }
    }
}
