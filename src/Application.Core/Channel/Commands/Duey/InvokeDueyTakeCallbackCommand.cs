using Application.Core.Channel.DueyService;
using Application.Core.Channel.Net.Packets;
using DueyDto;

namespace Application.Core.Channel.Commands.Duey
{
    internal class InvokeDueyTakeCallbackCommand : IWorldChannelCommand
    {
        TakeDueyPackageResponse data;

        public InvokeDueyTakeCallbackCommand(TakeDueyPackageResponse data)
        {
            this.data = data;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(data.Package.ReceiverId);
            if (chr == null)
                return;

            if (data.Code == 0)
            {
                var dp = ctx.WorldChannel.Mapper.Map<DueyPackageObject>(data.Package);

                var dpItem = dp.Item;

                if (!chr.canHoldMeso(dp.Mesos))
                {
                    chr.sendPacket(DueyPacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_RECV_UNKNOWN_ERROR.getCode()));
                    ctx.WorldChannel.Node.Transport.TakeDueyPackageCommit(new DueyDto.TakeDueyPackageCommit { MasterId = chr.Id, PackageId = dp.PackageId, Success = false });
                    return;
                }

                if (dpItem != null)
                {
                    if (!chr.CanHoldUniquesOnly(dpItem.getItemId()))
                    {
                        chr.sendPacket(DueyPacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_RECV_RECEIVER_WITH_UNIQUE.getCode()));
                        ctx.WorldChannel.Node.Transport.TakeDueyPackageCommit(new DueyDto.TakeDueyPackageCommit { MasterId = chr.Id, PackageId = dp.PackageId, Success = false });
                        return;
                    }

                    if (!chr.canHold(dpItem.getItemId(), dpItem.getQuantity()))
                    {
                        chr.sendPacket(DueyPacketCreator.sendDueyMSG(DueyProcessorActions.TOCLIENT_RECV_NO_FREE_SLOTS.getCode()));
                        ctx.WorldChannel.Node.Transport.TakeDueyPackageCommit(new DueyDto.TakeDueyPackageCommit { MasterId = chr.Id, PackageId = dp.PackageId, Success = false });
                        return;
                    }
                }


                ctx.WorldChannel.Node.Transport.TakeDueyPackageCommit(new DueyDto.TakeDueyPackageCommit { MasterId = chr.Id, PackageId = dp.PackageId, Success = false });
                ctx.WorldChannel.NodeService.ItemDistributeService.Distribute(chr, dpItem == null ? [] : [dpItem], dp.Mesos, 0, 0, "包裹满了");
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
    }
}
