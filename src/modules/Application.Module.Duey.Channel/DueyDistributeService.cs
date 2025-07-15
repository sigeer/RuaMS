using Application.Core.Channel.Services;
using Application.Core.Game.Players;
using Application.Shared.Constants;
using AutoMapper;
using client.inventory;
using client.inventory.manipulator;

namespace Application.Module.Duey.Channel
{
    internal class DueyDistributeService : IItemDistributeService
    {
        readonly IChannelTransport _transport;
        readonly IMapper _mapper;

        public DueyDistributeService(IChannelTransport transport, IMapper mapper)
        {
            _transport = transport;
            _mapper = mapper;
        }

        void CreateDueyPackageFromSystem(int sendMesos, Item? item, string? sendMessage, string recipient, bool quick)
        {
            _transport.CreateDueyPackage(new DueyDto.CreatePackageRequest
            {
                SenderId = ServerConstants.SystemCId,
                SendMeso = sendMesos,
                SendMessage = sendMessage,
                ReceiverName = recipient,
                Quick = quick,
                Item = _mapper.Map<Dto.ItemDto>(item),
            });
        }
        public void Distribute(IPlayer chr, List<Item> items, int meso, int cashType, int cashValue, string? title = null)
        {
            bool needNotice = false;

            if (meso != 0)
            {
                if (chr.canHoldMeso(meso))
                {
                    chr.gainMeso(meso, false);
                }
                else
                {
                    CreateDueyPackageFromSystem(meso, null, title, chr.Name, true);
                    needNotice = true;
                }
            }

            foreach (var item in items)
            {
                if (chr.canHold(item.getItemId(), item.getQuantity()))
                    InventoryManipulator.addFromDrop(chr.Client, item, false);
                else
                {
                    needNotice = true;
                    CreateDueyPackageFromSystem(0, item, title, chr.Name, true);
                }
            }

            chr.getCashShop().gainCash(cashType, cashValue);

            if (needNotice)
            {
                chr.dropMessage($"你的背包满了，请通过Duey领取额外的物品！");
            }
        }
    }
}
