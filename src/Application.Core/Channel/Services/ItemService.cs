using Application.Core.Channel.DataProviders;
using Application.Core.Channel.ResourceTransaction;
using Application.Core.Game.Items;
using Application.Core.Game.Relation;
using Application.Core.Model;
using Application.Core.ServerTransports;
using Application.Templates.Character;
using Application.Templates.Exceptions;
using Application.Templates.Item.Pet;
using AutoMapper;
using client.inventory;
using client.inventory.manipulator;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using server;
using System.Threading.Tasks;
using System.Transactions;
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
        readonly CashItemProvider _cashItemProvider;

        public ItemService(IMapper mapper,
            IChannelServerTransport transport,
            ILogger<ItemService> logger,
            WorldChannelServer server,
            CashItemProvider cashItemProvider)
        {
            _mapper = mapper;
            _transport = transport;
            _logger = logger;
            _server = server;
            _cashItemProvider = cashItemProvider;
        }

        public Item GenerateCouponItem(int itemId, short quantity)
        {
            CashItem it = new CashItem(77777777, itemId, 7777, ItemConstants.isPet(itemId) ? 30 : 0, quantity, true);
            return CashItem2Item(it);
        }
        public Item CashItem2Item(CashItem cashItem)
        {
            var abTemplate = ItemInformationProvider.getInstance().GetTrustTemplate(cashItem.getItemId()) ;
            Item item;

            if (abTemplate is PetItemTemplate petTemplate)
            {
                item = new Pet(petTemplate, 0, Yitter.IdGenerator.YitIdHelper.NextId());
            }
            else if (abTemplate is EquipTemplate equipTemplate)
            {
                item = ItemInformationProvider.getInstance().getEquipById(cashItem.getItemId());
            }
            else
            {
                item = Item.CreateVirtualItem(cashItem.getItemId(), cashItem.getCount());
            }

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
                if (abTemplate is PetItemTemplate subPet)
                    item.setExpiration(_server.getCurrentTime() + (long)TimeSpan.FromDays(subPet.Life).TotalMilliseconds);
                else
                    item.setExpiration(_server.getCurrentTime() + (long)TimeSpan.FromDays(cashItem.Period).TotalMilliseconds);
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
                var dataList = _mapper.Map<GiftModel[]>(_transport.LoadPlayerGifts(new ItemProto.GetMyGiftsRequest { MasterId = chr.Id }).List);
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


        internal async Task UseCash_TV(IPlayer player, Item item, string? victim, List<string> messages, int tvType, bool showEar)
        {
            if (player.TscRequest != null)
            {
                return;
            }
            var request = new ItemProto.CreateTVMessageRequest
            {
                MasterId = player.Id,
                PartnerName = victim,
                Type = tvType,
                ShowEar = showEar,
            };
            request.MessageList.AddRange(messages);

            player.TscRequest = new ResourceConsumeBuilder().ConsumeItem(item, 1).Build();
            await _transport.BroadcastTV(request);
        }

        public void OnBroadcastTVFinished(Empty data)
        {
            foreach (var ch in _server.Servers.Values)
            {
                ch.broadcastPacket(PacketCreator.removeTV());
            }
        }

        internal async Task UseCash_ItemMegaphone(IPlayer player, Item costItem, Item? item, string message, bool isWishper)
        {
            if (player.TscRequest != null)
            {
                return;
            }
            var request = new ItemProto.UseItemMegaphoneRequest
            {
                MasterId = player.Id,
                Message = message,
                Item = _mapper.Map<Dto.ItemDto>(item),
                IsWishper = isWishper,
            };

            player.TscRequest = new ResourceConsumeBuilder().ConsumeItem(costItem, 1).Build();
            await _transport.SendItemMegaphone(request);
        }

        public void BuyCashItem(IPlayer chr, int cashType, CashItem cItem)
        {
            chr.BuyCashItem(cashType, cItem, () =>
            {
                var data = _transport.SendBuyCashItem(new CashProto.BuyCashItemRequest
                {
                    MasterId = chr.Id,
                    CashItemId = cItem.getItemId(),
                    CashItemSn = cItem.getSN(),
                });
                if (data.Code != 0)
                {
                    chr.sendPacket(PacketCreator.showCashShopMessage((byte)data.Code));
                    return false;
                }

                BuyCashItemCallback(chr, cItem, data);
                return true;
            });
        }

        void BuyCashItemCallback(IPlayer chr, CashItem cItem, CashProto.BuyCashItemResponse data)
        {
            if (data.GiftInfo != null)
            {
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
        }

        public void BuyCashItemForGift(IPlayer chr, int cashType, CashItem cItem, string toName, string message, bool createRing = false)
        {
            chr.BuyCashItem(cashType, cItem, () =>
            {
                var data = _transport.SendBuyCashItem(new CashProto.BuyCashItemRequest
                {
                    MasterId = chr.Id,
                    CashItemId = cItem.getItemId(),
                    CashItemSn = cItem.getSN(),
                    GiftInfo = new CashProto.GiftInfo
                    {
                        Message = message,
                        Recipient = toName,
                        CreateRing = createRing
                    }
                });
                if (data.Code != 0)
                {
                    chr.sendPacket(PacketCreator.showCashShopMessage((byte)data.Code));
                    return false;
                }

                BuyCashItemCallback(chr, cItem, data);
                return true;
            });
        }

        public bool RegisterNameChange(IPlayer chr, string newName)
        {
            Dto.NameChangeResponse res = _transport.ReigsterNameChange(new Dto.NameChangeRequest { MasterId = chr.Id, NewName = newName });
            if (res.Code == 0)
            {
                chr.Name = newName;
                return true;
            }

            return false;
        }

        internal void UseCdk(IPlayer chr, string cdk)
        {
            UseCdkResponseCode code = UseCdkResponseCode.Success;
            if (!chr.Client.attemptCsCoupon())
            {
                code = UseCdkResponseCode.TooManyError;
            }

            ItemProto.UseCdkResponse res = new ItemProto.UseCdkResponse();
            if (code == UseCdkResponseCode.Success)
            {
                res = _transport.UseCdk(new ItemProto.UseCdkRequest { MasterId = chr.Id, Cdk = cdk });
                code = (UseCdkResponseCode)res.Code;
            }

            if (code != UseCdkResponseCode.Success)
            {
                chr.sendPacket(PacketCreator.showCashShopMessage((byte)code));
            }
            else
            {
                chr.Client.resetCsCoupon();
                List<Item> cashItems = new();
                List<ItemQuantity> items = new();
                int nxCredit = 0;
                int maplePoints = 0;
                int nxPrepaid = 0;
                int mesos = 0;

                foreach (var pair in res.Items)
                {
                    int quantity = pair.Quantity;

                    CashShop cs = chr.getCashShop();
                    var itemType = (CdkItemType)pair.Type;
                    switch (itemType)
                    {
                        case CdkItemType.Meso:
                            chr.gainMeso(quantity, false); //mesos
                            mesos += quantity;
                            break;
                        case CdkItemType.NxCredit:
                            cs.gainCash(1, quantity);    //nxCredit
                            nxCredit += quantity;
                            break;
                        case CdkItemType.MaplePoint:
                            cs.gainCash(2, quantity);    //maplePoint
                            maplePoints += quantity;
                            break;
                        case CdkItemType.NxPrepaid:
                            cs.gainCash(4, quantity);    //nxPrepaid
                            nxPrepaid += quantity;
                            break;
                        case CdkItemType.Unknown:
                            cs.gainCash(1, quantity);
                            nxCredit += quantity;
                            cs.gainCash(4, (quantity / 5000));
                            nxPrepaid += quantity / 5000;
                            break;

                        default:
                            int item = pair.ItemId;

                            short qty;
                            if (quantity > short.MaxValue)
                            {
                                qty = short.MaxValue;
                            }
                            else if (quantity < short.MinValue)
                            {
                                qty = short.MinValue;
                            }
                            else
                            {
                                qty = (short)quantity;
                            }

                            if (ItemInformationProvider.getInstance().isCash(item))
                            {
                                Item it = GenerateCouponItem(item, qty);

                                cs.addToInventory(it);
                                cashItems.Add(it);
                            }
                            else
                            {
                                InventoryManipulator.addById(chr.Client, item, qty, "");
                                items.Add(new(item, qty));
                            }
                            break;
                    }
                }
                if (cashItems.Count > 255)
                {
                    cashItems = cashItems.Take(255).ToList();
                }
                if (nxCredit != 0 || nxPrepaid != 0)
                { //coupon packet can only show maple points (afaik)
                    chr.sendPacket(PacketCreator.showBoughtQuestItem(0));
                }
                else
                {
                    chr.sendPacket(PacketCreator.showCouponRedeemedItems(chr.Client.AccountEntity!.Id, maplePoints, mesos, cashItems, items));
                }
                chr.Client.enableCSActions();
            }
        }
    }
}
