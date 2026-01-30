using Application.Core.Channel.Commands;
using Application.Core.Channel.DataProviders;
using Application.Core.Game.Items;
using Application.Core.Game.Relation;
using Application.Templates.Item.Pet;
using client.inventory;
using client.inventory.manipulator;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        private int itemEffect;
        private ScheduledFuture? itemExpireTask = null;
        public void cancelExpirationTask()
        {
            if (itemExpireTask != null)
            {
                itemExpireTask.cancel(false);
                itemExpireTask = null;
            }
        }

        public void ClearExpiredItems()
        {
            bool deletedCoupon = false;

            long expiration, currenttime = Client.CurrentServer.Node.getCurrentTime();
            foreach (var skill in getSkills())
            {
                if (skill.Value.expiration != -1 && skill.Value.expiration < currenttime)
                {
                    changeSkillLevel(skill.Key, -1, 0, -1);
                }
            }
            List<Item> toberemove = new();
            foreach (Inventory inv in Bag.GetValues())
            {
                foreach (Item item in inv.list())
                {
                    expiration = item.getExpiration();

                    if (expiration != -1 && (expiration < currenttime) && ((item.getFlag() & ItemConstants.LOCK) == ItemConstants.LOCK))
                    {
                        short lockObj = item.getFlag();
                        lockObj &= ~(ItemConstants.LOCK);
                        item.setFlag(lockObj); //Probably need a check, else people can make expiring items into permanent items...
                        item.setExpiration(-1);
                        forceUpdateItem(item);   //TEST :3
                    }
                    else if (expiration != -1 && expiration < currenttime)
                    {
                        if (!ItemConstants.isPet(item.getItemId()))
                        {
                            sendPacket(PacketCreator.itemExpired(item.getItemId()));
                            toberemove.Add(item);
                            if (ItemConstants.isRateCoupon(item.getItemId()))
                            {
                                deletedCoupon = true;
                            }
                        }
                        else if (item is Pet pet)
                        {
                            if (pet != null)
                            {
                                unequipPet(pet, true);
                            }

                            if (ItemConstants.isExpirablePet(item.getItemId()))
                            {
                                sendPacket(PacketCreator.itemExpired(item.getItemId()));
                                toberemove.Add(item);
                            }
                            else
                            {
                                item.setExpiration(-1);
                                forceUpdateItem(item);
                            }
                        }
                    }
                }

                if (toberemove.Count > 0)
                {
                    foreach (Item item in toberemove)
                    {
                        Bag.RemoveFromSlot(inv.getType(), item.getPosition(), item.getQuantity(), true);
                    }

                    ItemInformationProvider ii = ItemInformationProvider.getInstance();
                    foreach (Item item in toberemove)
                    {
                        List<int> toadd = new();
                        var replace = ii.GetReplaceItemTemplate(item.getItemId());
                        if (replace != null)
                        {
                            toadd.Add(replace.ItemId);
                            if (!string.IsNullOrEmpty(replace.Message))
                            {
                                Notice(replace.Message);
                            }
                        }
                        foreach (int itemid in toadd)
                        {
                            InventoryManipulator.addById(Client, itemid, 1);
                        }
                    }

                    toberemove.Clear();
                }

                if (deletedCoupon)
                {
                    updateCouponRates();
                }
            }
        }

        public void expirationTask()
        {
            if (itemExpireTask == null)
            {
                itemExpireTask = Client.CurrentServer.Node.TimerManager.register(new NamedRunnable($"Player:{Id},{GetHashCode()}_ItemExpireTask", () =>
                {
                    Client.CurrentServer.Post(new PlayerItemExpiredCommand(this));
                }), 60_000);
            }
        }

        public void setInventory(InventoryType type, Inventory inv)
        {
            Bag.SetValue(type, inv);
        }
        public Inventory getInventory(InventoryType type)
        {
            return Bag[type];
        }

        public byte getSlots(int type)
        {
            return (byte)(type == InventoryType.CASH.getType() ? 96 : Bag[type].getSlotLimit());
        }

        public bool canGainSlots(int type, int slots)
        {
            slots += Bag[type].getSlotLimit();
            return slots <= 96;
        }
        public bool gainSlots(int type, int slots)
        {
            return gainSlots(type, slots, true);
        }
        public bool gainSlots(int type, int slots, bool update)
        {
            int newLimit = gainSlotsInternal(type, slots);
            if (newLimit != -1)
            {
                this.saveCharToDB();
                if (update)
                {
                    sendPacket(PacketCreator.updateInventorySlotLimit(type, newLimit));
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private int gainSlotsInternal(int type, int slots)
        {
            if (canGainSlots(type, slots))
            {
                int newLimit = Bag[type].getSlotLimit() + slots;
                Bag[type].setSlotLimit(newLimit);
                return newLimit;
            }
            else
            {
                return -1;
            }
        }

        public int countItem(int itemid)
        {
            return Bag[ItemConstants.getInventoryType(itemid)].countById(itemid);
        }

        public bool canHold(int itemid, int quantity = 1)
        {
            return Inventory.checkSpot(this, Item.CreateVirtualItem(itemid, (short)quantity));
        }

        public bool canHoldUniques(List<int> itemids)
        {
            return itemids.All(x => CanHoldUniquesOnly(x));
        }

        public bool CanHoldUniquesOnly(int itemId)
        {
            return !ItemInformationProvider.getInstance().isPickupRestricted(itemId) || !haveItem(itemId);
        }

        /// <summary>
        /// 是否拥有某个道具（不确定道具类型时）
        /// </summary>
        /// <param name="itemid"></param>
        /// <returns></returns>
        public bool haveItem(int itemid)
        {
            return haveItemWithId(itemid, ItemConstants.isEquipment(itemid));
        }

        /// <summary>
        /// 是否拥有某个道具
        /// </summary>
        /// <param name="itemid"></param>
        /// <param name="checkEquipped"></param>
        /// <returns></returns>
        public bool haveItemWithId(int itemid, bool checkEquipped = true)
        {
            return (Bag[ItemConstants.getInventoryType(itemid)].findById(itemid) != null)
                    || (checkEquipped && Bag[InventoryType.EQUIPPED].findById(itemid) != null);
        }

        public bool haveItemEquipped(int itemid)
        {
            return (Bag[InventoryType.EQUIPPED].findById(itemid) != null);
        }

        public bool haveCleanItem(int itemid)
        {
            return getCleanItemQuantity(itemid, ItemConstants.isEquipment(itemid)) > 0;
        }

        public bool HasEmptySlotByItem(int itemId)
        {
            return getInventory(ItemConstants.getInventoryType(itemId)).getNextFreeSlot() > -1;
        }

        public bool hasEmptySlot(sbyte invType)
        {
            return getInventory(InventoryTypeUtils.getByType(invType)).getNextFreeSlot() > -1;
        }

        public int getItemQuantity(int itemid, bool checkEquipped)
        {
            int count = Bag[ItemConstants.getInventoryType(itemid)].countById(itemid);
            if (checkEquipped)
            {
                count += Bag[InventoryType.EQUIPPED].countById(itemid);
            }
            return count;
        }

        public int getCleanItemQuantity(int itemid, bool checkEquipped)
        {
            int count = Bag[ItemConstants.getInventoryType(itemid)].countNotOwnedById(itemid);
            if (checkEquipped)
            {
                count += Bag[InventoryType.EQUIPPED].countNotOwnedById(itemid);
            }
            return count;
        }

        public void setItemEffect(int itemEffect)
        {
            this.itemEffect = itemEffect;
        }

        public int getItemEffect()
        {
            return itemEffect;
        }


        #region meso
        public bool canHoldMeso(int gain)
        {
            // thanks lucasziron for pointing out a need to check space availability for mesos on player transactions
            long nextMeso = (long)MesoValue.get() + gain;
            return nextMeso <= int.MaxValue;
        }
        [Obsolete("使用 GainMeso")]

        public void gainMeso(int gain, bool show = true, bool enableActions = false, bool inChat = false)
        {
            long nextMeso;

            nextMeso = (long)MesoValue.get() + gain;  // thanks Thora for pointing integer overflow here
            if (nextMeso > int.MaxValue)
            {
                gain -= (int)(nextMeso - int.MaxValue);
            }
            else if (nextMeso < 0)
            {
                gain = -MesoValue.get();
            }
            nextMeso = MesoValue.addAndGet(gain);

            if (gain != 0)
            {
                updateSingleStat(Stat.MESO, (int)nextMeso, enableActions);
                if (show)
                {
                    sendPacket(PacketCreator.getShowMesoGain(gain, inChat));
                }
            }
            else
            {
                sendPacket(PacketCreator.enableActions());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gain"></param>
        /// <param name="d"></param>
        /// <param name="enableActions"></param>
        /// <returns>是否成功</returns>
        public bool TryGainMeso(int gain, GainItemShow d = GainItemShow.NotShown, bool enableActions = false)
        {
            bool canGainMeso = false;
            long nextMeso;

            nextMeso = (long)MesoValue.get() + gain;
            canGainMeso = nextMeso <= int.MaxValue || nextMeso >= 0;
            if (canGainMeso)
            {
                nextMeso = MesoValue.addAndGet(gain);
            }
            else
            {
                return false;
            }

            if (canGainMeso)
            {
                updateSingleStat(Stat.MESO, (int)nextMeso, enableActions);
                GainMesoShowMessage(gain, d);
            }
            else
            {
                sendPacket(PacketCreator.enableActions());
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gain"></param>
        /// <param name="d"></param>
        /// <param name="enableActions"></param>
        /// <returns>超出上限后无法拾取的数量</returns>
        public int GainMeso(int gain, GainItemShow d = GainItemShow.NotShown, bool enableActions = false)
        {
            int notGained = 0;
            long nextMeso;

            nextMeso = (long)MesoValue.get() + gain;
            if (nextMeso > int.MaxValue)
            {
                notGained = (int)(nextMeso - int.MaxValue);
                gain -= notGained;
            }
            else if (nextMeso < 0)
            {
                notGained = (int)nextMeso;
                gain = -MesoValue.get();
            }

            nextMeso = MesoValue.addAndGet(gain);

            if (gain != 0)
            {
                updateSingleStat(Stat.MESO, (int)nextMeso, enableActions);
                GainMesoShowMessage(gain, d);
            }
            else
            {
                sendPacket(PacketCreator.enableActions());
            }
            return notGained;
        }
        #endregion

        void GainItemShowMessage(int itemId, short quantity, GainItemShow d = GainItemShow.NotShown)
        {
            switch (d)
            {
                case Shared.Constants.Item.GainItemShow.NotShown:
                    break;
                case Shared.Constants.Item.GainItemShow.ShowInChat:
                    sendPacket(PacketCreator.getShowItemGain(itemId, quantity, true));
                    break;
                case Shared.Constants.Item.GainItemShow.ShowInMessage:
                    sendPacket(PacketCreator.getShowItemGain(itemId, quantity, false));
                    break;
                default:
                    break;
            }
        }

        void GainMesoShowMessage(int meso, GainItemShow d = GainItemShow.NotShown)
        {
            switch (d)
            {
                case Shared.Constants.Item.GainItemShow.NotShown:
                    break;
                case Shared.Constants.Item.GainItemShow.ShowInChat:
                    sendPacket(PacketCreator.getShowMesoGain(meso, true));
                    break;
                case Shared.Constants.Item.GainItemShow.ShowInMessage:
                    sendPacket(PacketCreator.getShowMesoGain(meso, false));
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemId">道具Id</param>
        /// <param name="quantity">数量，可负数</param>
        /// <param name="randomStats">item为装备时有效：随机属性</param>
        /// <param name="show">显示</param>
        /// <param name="expires">道具过期时间。单位ms，-1不会过期</param>
        /// <returns>获得的道具</returns>
        public Item? GainItem(int itemId, short quantity, bool randomStats, GainItemShow show = GainItemShow.NotShown, long expires = -1)
        {
            if (quantity == 0)
            {
                return null;
            }

            Item? item = null;

            var invType = ItemConstants.getInventoryType(itemId);
            if (quantity >= 0)
            {
                if (!InventoryManipulator.checkSpace(Client, itemId, quantity, ""))
                {
                    dropMessage(1, "Your inventory is full. Please remove an item from your " + invType.ToString() + " inventory.");
                    return null;
                }

                ItemInformationProvider ii = ItemInformationProvider.getInstance();

                bool addItemResult = false;
                if (item == null)
                {
                    if (invType == InventoryType.EQUIP)
                    {
                        item = ii.getEquipById(itemId);

                        if (item != null)
                        {
                            Equip it = (Equip)item;
                            if (ItemConstants.isAccessory(item.getItemId()) && it.getUpgradeSlots() <= 0)
                            {
                                it.setUpgradeSlots(3);
                            }

                            if (YamlConfig.config.server.USE_ENHANCED_CRAFTING == true && getCS() == true)
                            {
                                if (!(isGM() && YamlConfig.config.server.USE_PERFECT_GM_SCROLL))
                                {
                                    it.setUpgradeSlots(it.getUpgradeSlots() + 1);
                                }
                                item = ItemInformationProvider.getInstance().scrollEquipWithId(it, ItemId.CHAOS_SCROll_60, true, ItemId.CHAOS_SCROll_60, isGM());
                            }

                            if (randomStats)
                            {
                                ii.randomizeStats(it);
                            }
                        }
                    }
                    else
                    {
                        item = Item.CreateVirtualItem(itemId, quantity);
                    }
                }

                if (expires >= 0)
                {
                    item!.setExpiration(Client.CurrentServer.Node.getCurrentTime() + expires);
                }

                addItemResult = InventoryManipulator.addFromDrop(Client, item!, false);
                if (!addItemResult)
                    return null;
            }
            else
            {
                InventoryManipulator.removeById(Client, invType, itemId, -quantity, true, false);
            }

            GainItemShowMessage(itemId, quantity, show);

            return item;
        }

        public Pet? EvolvePet(Pet from, long expires)
        {
            var abTemplate = ItemInformationProvider.getInstance().GetTrustTemplate(from.getItemId());
            if (abTemplate is PetItemTemplate petTemplate)
            {
                if (from != null)
                {
                    var evolved = new Pet(petTemplate, 0, Yitter.IdGenerator.YitIdHelper.NextId());

                    Point pos = getPosition();
                    pos.Y -= 12;
                    evolved.setPos(pos);
                    evolved.setFh(getMap().Footholds.FindBelowFoothold(evolved.getPos()).getId());
                    evolved.setStance(0);
                    evolved.Summoned = true;

                    var fromDefaultName = Client.CurrentCulture.GetItemName(from.getItemId());
                    evolved.Name = from.Name != fromDefaultName ? from.Name : fromDefaultName;
                    evolved.Tameness = from.Tameness;
                    evolved.Fullness = from.Fullness;
                    evolved.Level = from.Level;
                    evolved.setExpiration(Client.CurrentServer.Node.getCurrentTime() + expires);

                    return evolved;
                }
            }
            return null;
        }

        public Ring? GetRingFromTotal(RingSourceModel? ring)
        {
            if (ring == null)
                return null;

            if (Id == ring.CharacterId1)
                return new Ring(ring.Id, ring.RingId1, ring.RingId2, ring.CharacterId2, ring.ItemId, ring.CharacterName2);
            if (Id == ring.CharacterId2)
                return new Ring(ring.Id, ring.RingId2, ring.RingId1, ring.CharacterId1, ring.ItemId, ring.CharacterName1);

            Log.Fatal("Character{CharacterId} 加载了不属于他的RingSourceId = {RingSourceId}", Id, ring.Id);
            return null;
        }

        public void BuyCashItem(int cashType, CashItem cItem, Func<bool> condition)
        {
            if (cItem.getPrice() > CashShopModel.getCash(cashType))
                return;

            if (!condition.Invoke())
                return;

            CashShopModel.BuyCashItem(cashType, cItem);
            sendPacket(PacketCreator.showCash(this));
        }
    }
}
