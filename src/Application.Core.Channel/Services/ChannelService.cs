using Application.Core.Datas;
using Application.Core.Game.Life;
using Application.Core.Game.Players;
using Application.Core.Game.TheWorld;
using Application.Core.Managers;
using Application.Core.ServerTransports;
using Application.Shared.Dto;
using AutoMapper;
using client.inventory;
using server;
using static server.CashShop;
using Application.Shared.Items;

namespace Application.Core.Channel.Services
{
    internal class ChannelService : IChannelService
    {
        readonly IChannelServerTransport _tranport;
        readonly CharacterService _characteService;
        readonly IWorldChannel _server;
        readonly IMapper _mapper;

        public ChannelService(IChannelServerTransport tranport, CharacterService characteService, IWorldChannel server, IMapper mapper)
        {
            _tranport = tranport;
            _characteService = characteService;
            _server = server;
            _mapper = mapper;
        }
        public void SetPlayerOnlined(int id)
        {
            _tranport.SetPlayerOnlined(id, _server.getId());
        }
        public CharacterValueObject? GetPlayerData(string clientSession, int cid)
        {
            return _tranport.GetPlayerData(clientSession, cid);
        }
        public void RemovePlayerIncomingInvites(int id)
        {
            _tranport.SendRemovePlayerIncomingInvites(id);
        }

        public void SaveChar(Player player, bool isLogoff = false)
        {
            var dto = _characteService.Deserialize(player);
            if (isLogoff)
            {
                dto.Channel = 0;
            }
            _tranport.SendPlayerObject(dto);
        }

        public void SaveBuff(IPlayer player)
        {
            _tranport.SendBuffObject(player.getId(), _characteService.DeserializeBuff(player));
        }

        public PlayerBuffSaveDto GetBuffFromStorage(IPlayer player)
        {
            return _tranport.GetBuffObject(player.Id);
        }

        public Item GenerateCouponItem(int itemId, short quantity)
        {
            CashItem it = new CashItem(77777777, itemId, 7777, ItemConstants.isPet(itemId) ? 30 : 0, quantity, true);
            return CashItem2Item(it);
        }

        public Item CashItem2Item(CashItem cashItem)
        {
            Item item;

            int petid = -1;
            if (ItemConstants.isPet(cashItem.getItemId()))
            {
                petid = ItemManager.CreatePet(cashItem.getItemId());
            }

            if (ItemConstants.getInventoryType(cashItem.getItemId()).Equals(InventoryType.EQUIP))
            {
                item = ItemInformationProvider.getInstance().getEquipById(cashItem.getItemId());
            }
            else
            {
                item = new Item(cashItem.getItemId(), 0, cashItem.getCount(), petid);
            }

            if (ItemConstants.EXPIRING_ITEMS)
            {
                if (cashItem.Period == 1)
                {
                    switch (cashItem.getItemId())
                    {
                        case ItemId.DROP_COUPON_2X_4H:
                        case ItemId.EXP_COUPON_2X_4H: // 4 Hour 2X coupons, the period is 1, but we don't want them to last a day.
                            item.setExpiration(_server.getCurrentTime() + (long)TimeSpan.FromHours(4).TotalMilliseconds);
                            /*
                            } else if(itemId == 5211047 || itemId == 5360014) { // 3 Hour 2X coupons, unused as of now
                                    item.setExpiration(Server.getInstance().getCurrentTime() + HOURS.toMillis(3));
                            */
                            break;
                        case ItemId.EXP_COUPON_3X_2H:
                            item.setExpiration(_server.getCurrentTime() + (long)TimeSpan.FromHours(2).TotalMilliseconds);
                            break;
                        default:
                            item.setExpiration(_server.getCurrentTime() + (long)TimeSpan.FromDays(1).TotalMilliseconds);
                            break;
                    }
                }
                else
                {
                    item.setExpiration(_server.getCurrentTime() + (long)TimeSpan.FromDays(cashItem.Period).TotalMilliseconds);
                }
            }

            item.setSN(cashItem.getSN());
            return item;
        }

        public Dictionary<int, List<DropEntry>> RequestAllReactorDrops()
        {
            return _mapper.Map<Dictionary<int, List<DropEntry>>>(_tranport.RequestAllReactorDrops());
        }
    }
}
