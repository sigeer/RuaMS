using Application.Core.Channel;
using Application.Core.Channel.DataProviders;
using Application.Core.Game.Items;
using Application.Core.Game.Relation;
using Application.Core.Model;
using Application.Core.ServerTransports;
using Application.Shared.Items;
using AutoMapper;
using client.inventory;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.Extensions.Logging;
using Mysqlx.Crud;
using Org.BouncyCastle.Cms;
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
                item = Item.CreateVirtualItem(cashItem.getItemId(), cashItem.getCount());
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
                var dataList = _mapper.Map<GiftModel[]>(_transport.LoadPlayerGifts(new ItemProto.GetMyGiftsRequest { MasterId = chr.Id}).List);
                foreach (var rs in dataList)
                {
                    cashShop.Notes++;
                    var cItem = _cashItemProvider.GetItemTrust(rs.Sn);
                    Item item = CashItem2Item(cItem);

                    item.setGiftFrom(rs.FromName);
                    if (item is Equip equip)
                    {
                        equip.SetRing(rs.Ring?.RingId2 ?? -1, rs.Ring);
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
                            packageItem.setGiftFrom(rs.FromName);
                            cashShop.addToInventory(packageItem);
                        }
                    }
                    else
                    {
                        cashShop.addToInventory(item);
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
            if (_itemStore.TryBeginTransaction(player, [item], 0, out var transaction))
            {
                var request = new ItemProto.CreateTVMessageRequest
                {
                    MasterId = player.Id,
                    ToName = victim,
                    Type = tvType,
                    ShowEar = showEar,
                    Transaction = transaction
                };
                request.MessageList.AddRange(messages);
                _transport.BroadcastTV(request);
            }

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
            if (_itemStore.TryBeginTransaction(player, [costItem], 0, out var transaction))
            {
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

        public void BuyCashItem(IPlayer chr, int cashType, CashItem cItem)
        {
            if (_itemStore.TryBuyCash(chr, cashType, cItem, out var transaction))
                _transport.SendBuyCashItem(new CashProto.BuyCashItemRequest
                {
                    MasterId = chr.Id,
                    CashItemId = cItem.getItemId(),
                    CashItemSn = cItem.getSN(),
                    Transaction = transaction
                });
        }

        public void BuyCashItemForGift(IPlayer chr, int cashType, CashItem cItem, string toName, string message, bool createRing = false)
        {
            if (_itemStore.TryBuyCash(chr, cashType, cItem, out var transaction))
                _transport.SendBuyCashItem(new CashProto.BuyCashItemRequest
                {
                    MasterId = chr.Id,
                    CashItemId = cItem.getItemId(),
                    CashItemSn = cItem.getSN(),
                    Transaction = transaction,
                    GiftInfo = new CashProto.GiftInfo
                    {
                        Message = message,
                        Recipient = toName,
                        CreateRing = createRing
                    }
                });
        }

        public void OnBuyCashItemCallback(CashProto.BuyCashItemResponse data)
        {
            var chr = _server.FindPlayerById(data.MasterId);
            if (chr != null)
            {
                if (data.Code != 0)
                {
                    chr.sendPacket(PacketCreator.showCashShopMessage((byte)data.Code));
                    return;
                }

                if (data.GiftInfo != null)
                {
                    var cItem = _cashItemProvider.getItem(data.Sn)!;
                    Item item = CashItem2Item(cItem);
                    if (data.GiftInfo.RingSource != null && CashItem2Item(cItem) is Equip equip)
                    {
                        var ring = _mapper.Map<RingSourceModel>(data.GiftInfo.RingSource);
                        equip.SetRing(ring.RingId1, ring);
                        chr.addPlayerRing(ring.GetRing(ring.RingId1));
                        // 原代码中 crush ring 用的是showBoughtCashItem
                        chr.sendPacket(PacketCreator.showBoughtCashRing(item, data.GiftInfo.Recipient, chr.Client.AccountId));
                        chr.getCashShop().addToInventory(item);
                    }


                    chr.sendPacket(PacketCreator.showGiftSucceed(data.GiftInfo.Recipient, cItem));
                }
                else
                {
                    var cItem = _cashItemProvider.getItem(data.Sn)!;
                    if (_cashItemProvider.isPackage(cItem.getItemId()))
                    {
                        var cashPackage = getPackage(cItem.getItemId());
                        foreach (var item in cashPackage)
                        {
                            chr.getCashShop().addToInventory(item);

                            chr.sendPacket(PacketCreator.showBoughtCashItem(item, chr.Client.AccountId));
                        }
                        chr.sendPacket(PacketCreator.showBoughtCashPackage(cashPackage, chr.Client.AccountId));
                    }
                    else
                    {
                        Item item = CashItem2Item(cItem);

                        chr.sendPacket(PacketCreator.showBoughtCashItem(item, chr.Client.AccountId));
                        chr.getCashShop().addToInventory(item);

                    }

                }

                chr.sendPacket(PacketCreator.showCash(chr));
            }
            _itemStore.HandleTransaction(data.Transaction);
        }
    }
}
