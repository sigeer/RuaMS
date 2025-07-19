using Application.Core.Channel.DataProviders;
using Application.Core.Channel.Services;
using Application.Core.Game.Trades;
using AutoMapper;
using client.autoban;
using client.inventory.manipulator;
using client.inventory;
using ItemProto;
using System.Collections.Concurrent;
using tools;
using Microsoft.Extensions.Logging;
using server;

namespace Application.Core.Channel
{
    public class PlayerShopManager
    {
        private ConcurrentDictionary<int, IPlayerShop> activeMerchants = new();
        private ConcurrentDictionary<int, IPlayerShop> playerShopData = new();

        readonly IMapper _mapper;
        readonly ILogger<PlayerShopManager> _logger;
        readonly WorldChannel _worldChannel;

        public PlayerShopManager(IMapper mapper, ILogger<PlayerShopManager> logger, WorldChannel worldChannel)
        {
            _mapper = mapper;
            _logger = logger;
            _worldChannel = worldChannel;
        }

        public bool RegisterShop(IPlayerShop shop)
        {
            if (shop.Type == PlayerShopType.PlayerShop)
                return playerShopData.TryAdd(shop.OwnerId, shop);

            if (shop.Type == PlayerShopType.HiredMerchant)
                return activeMerchants.TryAdd(shop.OwnerId, shop);

            throw new BusinessNotsupportException($"不支持的ShopType{shop.Type}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shop"></param>
        /// <param name="needStore">需要保存到弗兰德里</param>
        public void UnregisterShop(IPlayerShop shop, bool needStore = true)
        {
            if (shop.Type == PlayerShopType.PlayerShop)
            {
                if (playerShopData.TryRemove(shop.OwnerId, out var hm))
                {
                    SyncPlayerShop(hm, SyncPlayerShopOperation.Close);
                }
                return;
            }

            if (shop.Type == PlayerShopType.HiredMerchant)
            {
                if (activeMerchants.TryRemove(shop.OwnerId, out var hm))
                {
                    SyncPlayerShop(hm, needStore ? SyncPlayerShopOperation.Close : SyncPlayerShopOperation.CloseWithoutStore);
                }
                return;
            }

        }

        public List<IPlayerShop> GetAllShops()
        {
            return activeMerchants.Values.Where(x => x.Status.Is(PlayerShopStatus.Opening)).Concat(playerShopData.Values).ToList();
        }

        public IPlayerShop? GetPlayerShop(PlayerShopType type, int ownerid)
        {
            if (type == PlayerShopType.PlayerShop)
                return playerShopData.TryGetValue(ownerid, out var value) ? value : null;
            if (type == PlayerShopType.HiredMerchant)
                return activeMerchants.TryGetValue(ownerid, out var value) ? value : null;
            return null;
        }

        public bool RemoveCommodity(IPlayer chr, int slotIndex)
        {
            var shop = chr.VisitingShop;
            if (shop == null)
                return false;

            if (shop.OwnerId != chr.Id)
                return false;

            if (!shop.Status.Is(PlayerShopStatus.Maintenance))
            {
                chr.sendPacket(PacketCreator.serverNotice(1, "You can't take it with the store open."));
                return false;
            }

            if (slotIndex >= shop.Commodity.Count || slotIndex < 0)
            {
                AutobanFactory.PACKET_EDIT.alert(chr, chr.getName() + " tried to packet edit with a player shop.");
                _logger.LogWarning("Chr {CharacterName} tried to remove item at slot {Slot}", chr.getName(), slotIndex);
                chr.Client.Disconnect(true, false);
                return false;
            }

            shop.takeItemBack(slotIndex, chr);
            return true;
        }

        public bool AddCommodity(IPlayer chr, InventoryType ivType, Item ivItem, short perBundle, short bundles, int price)
        {
            var shop = chr.VisitingShop;
            if (shop == null)
                return false;

            if (shop.OwnerId != chr.Id)
                return false;

            Item sellItem = ivItem.copy();
            if (!ItemConstants.isRechargeable(ivItem.getItemId()))
            {
                sellItem.setQuantity(perBundle);
            }
            PlayerShopItem shopItem = new PlayerShopItem(sellItem, bundles, price);
            if (!shop.Status.Is(PlayerShopStatus.Maintenance) || !shop.AddCommodity(shopItem))
            {
                // thanks Vcoc for pointing an exploit with unlimited shop slots
                chr.sendPacket(PacketCreator.serverNotice(1, "You can't sell it anymore."));
                return false;
            }

            if (ItemConstants.isRechargeable(shopItem.getItem().getItemId()))
            {
                InventoryManipulator.removeFromSlot(chr.Client, ivType, ivItem.getPosition(), ivItem.getQuantity(), true);
            }
            else
            {
                InventoryManipulator.removeFromSlot(chr.Client, ivType, ivItem.getPosition(), (short)(bundles * perBundle), true);
            }

            if (shop is PlayerShop ps)
                chr.sendPacket(PacketCreator.getPlayerShopItemUpdate(ps));
            else if (shop is HiredMerchant hm)
                chr.sendPacket(PacketCreator.updateHiredMerchant(hm, chr));
            return true;
        }

        public void BuyItem(IPlayer buyer, int itemIndex, int quantity)
        {
            var shop = buyer.VisitingShop;
            if (shop == null)
                return;

            try
            {
                if (shop.TradeLock())
                {
                    if (buyer.Id == shop.OwnerId)
                    {
                        buyer.sendPacket(PacketCreator.enableActions());
                        return;
                    }

                    PlayerShopItem pItem = shop.Commodity.get(itemIndex);

                    if (quantity < 1 || !pItem.isExist() || pItem.getBundles() < quantity)
                    {
                        buyer.sendPacket(PacketCreator.enableActions());
                        return;
                    }

                    Item newItem = pItem.getItem().copy();

                    newItem.setQuantity((short)(pItem.getItem().getQuantity() * quantity));
                    if (newItem.getInventoryType().Equals(InventoryType.EQUIP) && newItem.getQuantity() > 1)
                    {
                        buyer.sendPacket(PacketCreator.enableActions());
                        return;
                    }


                    KarmaManipulator.toggleKarmaFlagToUntradeable(newItem);

                    int price = Math.Min(pItem.getPrice() * quantity, int.MaxValue);
                    var mesoCheck = shop.MesoCheck(price);
                    if (mesoCheck != null)
                    {
                        buyer.dropMessage(1, mesoCheck);
                        buyer.sendPacket(PacketCreator.enableActions());
                        return;
                    }

                    if (buyer.getMeso() >= price)
                    {
                        if (InventoryManipulator.checkSpace(buyer.Client, newItem.getItemId(), newItem.getQuantity(), newItem.getOwner())
                            && InventoryManipulator.addFromDrop(buyer.Client, newItem, false))
                        {
                            buyer.GainMeso(-price, false);
                            price -= TradeManager.GetFee(price);  // thanks BHB for pointing out trade fees not applying here

                            shop.GainMeso(price);

                            shop.InsertSoldHistory(itemIndex, new SoldItem(buyer.getName(), pItem.getItem().getItemId(), newItem.getQuantity(), price));

                            pItem.setBundles((short)(pItem.getBundles() - quantity));
                            if (pItem.getBundles() < 1)
                            {
                                pItem.setDoesExist(false);

                                shop.OnCommoditySellout();
                            }

                            //if (shop is HiredMerchant hm)
                            //{
                            //    if (hm.SoldNotify)
                            //        hm.announceItemSold(newItem, price, hm.getQuantityLeft(pItem.getItem().getItemId()));
                            //}

                            shop.BroadcastShopItemUpdate();
                        }
                        else
                        {
                            buyer.dropMessage(1, "Your inventory is full. Please clear a slot before buying this item.");
                            buyer.sendPacket(PacketCreator.enableActions());
                            return;
                        }
                    }
                    else
                    {
                        buyer.dropMessage(1, "You don't have enough mesos to purchase this item.");
                        buyer.sendPacket(PacketCreator.enableActions());
                        return;
                    }

                    SyncPlayerShop(shop, SyncPlayerShopOperation.UpdateByTrade);
                }
            }
            finally
            {
                shop.TradeUnlock();
            }
        }

        public void NewPlayerShop(IPlayerShop? shop)
        {
            if (shop == null)
                return;

            if (shop.ChannelServer.PlayerShopManager.RegisterShop(shop))
            {
                SyncPlayerShop(shop);
            }
        }

        public bool CloseByPlayer(IPlayer chr)
        {
            var shop = chr.VisitingShop;
            if (shop == null)
                return false;

            if (shop.OwnerId != chr.Id)
                return false;

            shop.Close();

            bool needStore = true;
            if (shop.Type == PlayerShopType.HiredMerchant)
                needStore = !shop.Retrieve(chr);

            shop.ChannelServer.PlayerShopManager.UnregisterShop(shop, needStore);
            return true;
        }


        /// <summary>
        /// 每过一段时间同步所有数据到master供搜索
        /// </summary>
        /// <param name="shop"></param>
        /// <param name="isDestroyed">关店时true</param>
        public void SyncPlayerShop(IPlayerShop shop, SyncPlayerShopOperation operation = SyncPlayerShopOperation.Update)
        {
            _worldChannel.Container.Transport.SyncPlayerShop(GenrateSyncRequest(shop, operation));
        }

        private ItemProto.SyncPlayerShopRequest GenrateSyncRequest(IPlayerShop shop, SyncPlayerShopOperation operation = SyncPlayerShopOperation.Update)
        {
            var request = new ItemProto.SyncPlayerShopRequest()
            {
                OwnerId = shop.OwnerId,
                Channel = shop.Channel,
                MapId = shop.getMap().Id,
                Title = shop.Title,
                Meso = shop.Mesos,
                Type = (int)shop.Type,
                Operation = (int)operation,
                MapObjectId = shop.getObjectId()
            };
            request.Items.AddRange(_mapper.Map<ItemProto.PlayerShopItemDto[]>(shop.Commodity.Where(x => x.getBundles() > 0)));
            return request;
        }

        public void OnHiredMerchantItemBuy(ItemProto.NotifyItemPurchasedResponse data)
        {
            var owner = _worldChannel.Container.FindPlayerById(data.OwnerId);
            if (owner != null)
            {
                string qtyStr = data.Quantity > 1 ? " x " + data.Quantity : "";
                owner.dropMessage(6,
                    $"[Hired Merchant] Item '{ItemInformationProvider.getInstance().getName(data.ItemId)}'{qtyStr} has been sold for {data.GainedMeso} mesos. ({data.Left} left)");
            }

        }

        internal void Dispose()
        {
            List<SyncPlayerShopRequest> requests = [];
            foreach (var hm in activeMerchants.Values)
            {
                hm.Close();

                requests.Add(GenrateSyncRequest(hm, SyncPlayerShopOperation.Close));
            }



            // 个人商店
            foreach (var shop in playerShopData.Values.OfType<PlayerShop>())
            {
                shop.Close();

                requests.Add(GenrateSyncRequest(shop, SyncPlayerShopOperation.Close));
            }

            var request = new ItemProto.BatchSyncPlayerShopRequest();
            request.List.AddRange(requests);
            _worldChannel.Container.Transport.BatchSyncPlayerShop(request);
        }
    }
}
