using Application.Core.Channel;
using Application.Core.Channel.DataProviders;
using Application.Core.Game.Items;
using Application.Core.Model;
using Application.Core.ServerTransports;
using Application.Shared.Items;
using AutoMapper;
using client.inventory;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Mysqlx.Crud;
using server;
using tools;
using static server.CashShop;

namespace Application.Core.Channel.Services
{
    public class ItemService
    {
        readonly IMapper _mapper;
        readonly IChannelServerTransport _transport;
        readonly ILogger<ItemService> _logger;
        readonly WorldChannelServer _server;
        readonly ItemTransactionService _itemStore;
        readonly CashItemProvider _cashItemProvider;

        public ItemService(IMapper mapper, 
            IChannelServerTransport transport, 
            ILogger<ItemService> logger, 
            WorldChannelServer server, 
            ItemTransactionService itemStore, 
            CashItemProvider cashItemProvider)
        {
            _mapper = mapper;
            _transport = transport;
            _logger = logger;
            _server = server;
            _itemStore = itemStore;
            _cashItemProvider = cashItemProvider;
        }

        public Item GenerateCouponItem(int itemId, short quantity)
        {
            CashItem it = new CashItem(77777777, itemId, 7777, ItemConstants.isPet(itemId) ? 30 : 0, quantity, true);
            return CashItem2Item(it);
        }
        public Item CashItem2Item(CashItem cashItem)
        {
            Item item;

            if (ItemConstants.isPet(cashItem.getItemId()))
            {
                item = new Pet(cashItem.getItemId(), 0, Yitter.IdGenerator.YitIdHelper.NextId());
            }
            else if (ItemConstants.getInventoryType(cashItem.getItemId()).Equals(InventoryType.EQUIP))
            {
                item = ItemInformationProvider.getInstance().getEquipById(cashItem.getItemId());
            }
            else
            {
                item = new Item(cashItem.getItemId(), 0, cashItem.getCount());
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

        internal void AddCashItemBought(int sn)
        {
            _transport.AddCashItemBought(sn);
        }

        public List<Item> getPackage(int itemId)
        {
            return _cashItemProvider.GetPackage(itemId).Select(x => CashItem2Item(_cashItemProvider.GetItemTrust(x))).ToList();
        }

        public List<SpecialCashItem> GetSpecialCashItems()
        {
            return _mapper.Map<List<SpecialCashItem>>(_transport.RequestSpecialCashItems().Items);
        }

        public List<ItemMessagePair> LoadPlayerGifts(IPlayer chr)
        {
            var cashShop = chr.getCashShop();
            if (cashShop == null)
                return [];

            List<ItemMessagePair> gifts = new();
            try
            {
                var dataList = _mapper.Map<GiftModel[]>(_transport.LoadPlayerGifts(chr.Id));
                foreach (var rs in dataList)
                {
                    cashShop.Notes++;
                    var cItem = _cashItemProvider.GetItemTrust(rs.Sn);
                    Item item = CashItem2Item(cItem);
                    Equip? equip = null;
                    item.setGiftFrom(rs.From);
                    if (item.getInventoryType().Equals(InventoryType.EQUIP))
                    {
                        equip = (Equip)item;
                        equip.Ring = rs.Ring;
                        gifts.Add(new ItemMessagePair(equip, rs.Message));
                    }
                    else
                    {
                        gifts.Add(new(item, rs.Message));
                    }

                    if (_cashItemProvider.isPackage(cItem.getItemId()))
                    {
                        var packages = _cashItemProvider.GetPackage(cItem.getItemId()).Select(x => CashItem2Item(_cashItemProvider.GetItemTrust(x)));
                        //Packages never contains a ring
                        foreach (Item packageItem in packages)
                        {
                            packageItem.setGiftFrom(rs.From);
                            cashShop.addToInventory(packageItem);
                        }
                    }
                    else
                    {
                        cashShop.addToInventory(equip == null ? item : equip);
                    }
                }

                _transport.ClearGifts(dataList.Select(x => x.Id).ToArray());
            }
            catch (Exception sqle)
            {
                _logger.LogError(sqle.ToString());
            }

            return gifts;
        }

        public CashShopSurpriseResult? OpenCashShopSurprise(CashShop cs, long cashId)
        {
            // 移除了这里的锁
            Item? maybeCashShopSurprise = cs.getItemByCashId(cashId);
            if (maybeCashShopSurprise == null ||
                    maybeCashShopSurprise.getItemId() != ItemId.CASH_SHOP_SURPRISE)
            {
                return null;
            }

            Item cashShopSurprise = maybeCashShopSurprise;
            if (cashShopSurprise.getQuantity() <= 0)
            {
                return null;
            }

            if (cs.getItemsSize() >= 100)
            {
                return null;
            }

            var cashItemReward = _cashItemProvider.getRandomCashItem();
            if (cashItemReward == null)
            {
                return null;
            }

            short newQuantity = (short)(cashShopSurprise.getQuantity() - 1);
            cashShopSurprise.setQuantity(newQuantity);
            if (newQuantity <= 0)
            {
                cs.removeFromInventory(cashShopSurprise);
            }

            Item itemReward = CashItem2Item(cashItemReward);
            cs.addToInventory(itemReward);

            return new CashShopSurpriseResult(cashShopSurprise, itemReward);
        }


        internal void UseCash_TV(IPlayer player, Item item, string? victim, List<string> messages, int tvType, bool showEar)
        {
            var request = new ItemProto.CreateTVMessageRequest
            {
                MasterId = player.Id,
                ToName = victim,
                Type = tvType,
                ShowEar = showEar,
                Transaction = _itemStore.BeginTransaction(player, [item])
            };
            request.MessageList.AddRange(messages);
            _transport.BroadcastTV(request);
        }

        public void OnBroadcastTV(ItemProto.CreateTVMessageResponse data)
        {
            if (data.Code == 0)
            {
                var noticeMsg = string.Join(" ", data.Data.MessageList);
                foreach (var ch in _server.Servers.Values)
                {
                    foreach (var chr in ch.Players.getAllCharacters())
                    {
                        chr.sendPacket(PacketCreator.enableTV());
                        chr.sendPacket(PacketCreator.sendTV(data.Data.Master, data.Data.MessageList.ToArray(), data.Data.Type <= 2 ? data.Data.Type : data.Data.Type - 3, data.Data.MasterPartner));

                        if (data.Data.Type >= 3)
                        {
                            chr.sendPacket(PacketCreator.serverNotice(3, data.Data.Master.Channel, CharacterViewDtoUtils.GetPlayerNameWithMedal(data.Data.Master) + " : " + noticeMsg, data.ShowEar));
                        }
                    }
                }
            }
            else
            {
                var master = _server.FindPlayerById(data.MasterId);
                if (master != null)
                {
                    master.dropMessage(1, "MapleTV is already in use.");
                }

            }

            _itemStore.HandleTransaction(data.Transaction);
        }

        public void OnBroadcastTVFinished(Empty data)
        {
            foreach (var ch in _server.Servers.Values)
            {
                ch.broadcastPacket(PacketCreator.removeTV());
            }
        }

        internal void UseCash_ItemMegaphone(IPlayer player, Item costItem, Item? item, string message, bool isWishper)
        {
            var transaction = _itemStore.BeginTransaction(player, [costItem]);

            var request = new ItemProto.UseItemMegaphoneRequest
            {
                MasterId = player.Id,
                Message = message,
                Item = _mapper.Map<Dto.ItemDto>(item),
                IsWishper = isWishper,
                Transaction = transaction
            };
            _transport.SendItemMegaphone(request);
        }

        public void OnItemMegaphon(ItemProto.UseItemMegaphoneResponse data)
        {
            if (data.Code == 0)
            {
                foreach (var ch in _server.Servers.Values)
                {
                    ch.broadcastPacket(PacketCreator.itemMegaphone(data.Data.Message, data.Data.IsWishper, data.Data.SenderChannel, _mapper.Map<Item>(data.Data.Item)));
                }
            }
            else
            {

            }

            _itemStore.HandleTransaction(data.Transaction);
        }
    }
}
