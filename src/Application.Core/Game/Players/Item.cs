using Application.Core.Channel.DataProviders;
using Application.Core.Client.inventory;
using Application.Core.Game.Items;
using Application.Core.Game.Relation;
using client.inventory;
using client.inventory.manipulator;
using System.Runtime.CompilerServices;
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

        public void expirationTask()
        {
            if (itemExpireTask == null)
            {
                itemExpireTask = Client.CurrentServerContainer.TimerManager.register(new NamedRunnable($"Player:{Id},{GetHashCode()}_ItemExpireTask", () =>
                {
                    bool deletedCoupon = false;

                    long expiration, currenttime = Client.CurrentServerContainer.getCurrentTime();
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
                                InventoryManipulator.removeFromSlot(Client, inv.getType(), item.getPosition(), item.getQuantity(), true);
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
                                        dropMessage(replace.Message);
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

                }), 60000);
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
            Bag[type].lockInventory();
            try
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
            finally
            {
                Bag[type].unlockInventory();
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
            ItemInformationProvider ii = ItemInformationProvider.getInstance();
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

        public void gainMeso(int gain, bool show = true, bool enableActions = false, bool inChat = false)
        {
            long nextMeso;
            Monitor.Enter(petLock);
            try
            {
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
            }
            finally
            {
                Monitor.Exit(petLock);
            }

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

        public bool TryGainMeso(int gain, bool show = true, bool enableActions = false, bool inChat = false)
        {
            bool canGainMeso = false;
            long nextMeso;
            Monitor.Enter(petLock);
            try
            {
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
            }
            finally
            {
                Monitor.Exit(petLock);
            }


            if (canGainMeso)
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

            return true;
        }

        public int GainMeso(int gain, bool show = true, bool enableActions = false, bool inChat = false)
        {
            int notGained = 0;
            long nextMeso;
            Monitor.Enter(petLock);
            try
            {
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
            }
            finally
            {
                Monitor.Exit(petLock);
            }

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
            return notGained;
        }
        #endregion

        /// <summary>
        /// 移除<paramref name="type"/>栏的<paramref name="position"/>上的物品<paramref name="quantity"/>（默认1）个
        /// </summary>
        /// <param name="type"></param>
        /// <param name="position"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public bool RemoveItemBySlot(InventoryType type, short position, short quantity = 1, bool fromDrop = true, bool consume = false)
        {
            Inventory cashInv = getInventory(type);
            cashInv.lockInventory();
            try
            {
                return RemoveItemBySlotCore(type, position, quantity, fromDrop, consume);
            }
            finally
            {
                cashInv.unlockInventory();
            }
        }

        public bool RemoveItemById(InventoryType type, int itemId, short quantity = 1, bool fromDrop = true, bool consume = false)
        {
            int removeQuantity = quantity;
            Inventory inv = Bag[type];

            if (inv.countById(itemId) < quantity)
                return false;

            int slotLimit = type == InventoryType.EQUIPPED ? 128 : inv.getSlotLimit();

            for (short i = 0; i <= slotLimit; i++)
            {
                var item = inv.getItem((short)(type == InventoryType.EQUIPPED ? -i : i));
                if (item != null)
                {
                    if (item.getItemId() == itemId || item.getCashId() == itemId)
                    {
                        if (removeQuantity <= item.getQuantity())
                        {
                            RemoveItemBySlotCore(type, item.getPosition(), (short)removeQuantity, fromDrop, consume);
                            removeQuantity = 0;
                            break;
                        }
                        else
                        {
                            removeQuantity -= item.getQuantity();
                            RemoveItemBySlotCore(type, item.getPosition(), item.getQuantity(), fromDrop, consume);
                        }
                    }
                }
            }
            if (removeQuantity > 0 && type != InventoryType.CANHOLD)
            {
                throw new BusinessException("[Hack] Not enough items available of Item:" + itemId + ", Quantity (After Quantity/Over Current Quantity): " + (quantity - removeQuantity) + "/" + quantity);
            }
            return true;
        }

        bool RemoveItemBySlotCore(InventoryType type, short slot, short quantity, bool fromDrop, bool consume = false)
        {
            Inventory inv = getInventory(type);
            var item = inv.getItem(slot);
            if (item == null)
            {
                // _logger.LogError("道具不存在, InventoryType={InventoryType}, Slot={Slot}", type, slot);
                return false;
            }

            if (item.getQuantity() < quantity)
            {
                // _logger.LogError("没有足够的道具, InventoryType={InventoryType}, Slot={Slot}, Quantity={Quantity}, Cost={Cost}", type, slot, item.getQuantity(), quantity);
                return false;
            }

            bool allowZero = consume && ItemConstants.isRechargeable(item.getItemId());

            if (type == InventoryType.EQUIPPED)
            {
                inv.lockInventory();
                try
                {
                    unequippedItem((Equip)item);
                    inv.removeItem(slot, quantity, allowZero);
                }
                finally
                {
                    inv.unlockInventory();
                }

                AnnounceModifyInventory(item, fromDrop, allowZero);
            }
            else
            {
                if (item is Pet petObj)
                {
                    // thanks Vcoc for finding a d/c issue with equipped pets and pets remaining on DB here
                    int petIdx = getPetIndex(petObj.PetId);
                    if (petIdx > -1)
                    {
                        unequipPet(petObj, true);
                    }

                    inv.removeItem(slot, quantity, allowZero);
                    if (type != InventoryType.CANHOLD)
                    {
                        AnnounceModifyInventory(item, fromDrop, allowZero);
                    }

                    // thanks Robin Schulz for noticing pet issues when moving pets out of inventory
                }
                else
                {
                    inv.removeItem(slot, quantity, allowZero);
                    if (type != InventoryType.CANHOLD)
                    {
                        AnnounceModifyInventory(item, fromDrop, allowZero);
                    }
                }
            }
            return true;
        }

        void AnnounceModifyInventory(Item item, bool fromDrop, bool allowZero)
        {
            if (item.getQuantity() == 0 && !allowZero)
            {
                sendPacket(PacketCreator.modifyInventory(fromDrop, [new ModifyInventory(3, item)]));
            }
            else
            {
                sendPacket(PacketCreator.modifyInventory(fromDrop, [new ModifyInventory(1, item)]));
            }
        }

        public Item? GainItem(int itemId, short quantity, bool randomStats, bool showMessage, long expires = -1, Pet? from = null)
        {
            Item? item = null;

            var invType = ItemConstants.getInventoryType(itemId);
            if (quantity >= 0)
            {
                if (!InventoryManipulator.checkSpace(Client, itemId, quantity, ""))
                {
                    dropMessage(1, "Your inventory is full. Please remove an item from your " + invType.ToString() + " inventory.");
                    return null;
                }

                if (ItemConstants.isPet(itemId))
                {
                    if (from != null)
                    {
                        var evolved = new Pet(itemId, 0, Yitter.IdGenerator.YitIdHelper.NextId());

                        Point pos = getPosition();
                        pos.Y -= 12;
                        evolved.setPos(pos);
                        evolved.setFh(getMap().getFootholds().findBelow(evolved.getPos()).getId());
                        evolved.setStance(0);
                        evolved.Summoned = true;

                        var fromDefaultName = Client.CurrentCulture.GetItemName(from.getItemId());
                        evolved.Name = from.Name != fromDefaultName ? from.Name : fromDefaultName;
                        evolved.Tameness = from.Tameness;
                        evolved.Fullness = from.Fullness;
                        evolved.Level = from.Level;
                        evolved.setExpiration(Client.CurrentServerContainer.getCurrentTime() + expires);

                        item = evolved;
                    }
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
                                item = ItemInformationProvider.getInstance().scrollEquipWithId(item, ItemId.CHAOS_SCROll_60, true, ItemId.CHAOS_SCROll_60, isGM());
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
                    item!.setExpiration(Client.CurrentServerContainer.getCurrentTime() + expires);
                }

                addItemResult = InventoryManipulator.addFromDrop(Client, item!, false);
                if (!addItemResult)
                    return null;
            }
            else
            {
                InventoryManipulator.removeById(Client, invType, itemId, -quantity, true, false);
            }

            if (showMessage)
            {
                Client.sendPacket(PacketCreator.getShowItemGain(itemId, quantity, true));
            }

            return item;
        }

        public void RemoveById(InventoryType type, IEnumerable<int> itemIds, bool fromDrop)
        {
            Inventory inv = Bag[type];
            int slotLimit = type == InventoryType.EQUIPPED ? 128 : inv.getSlotLimit();

            for (short i = 0; i <= slotLimit; i++)
            {
                var item = inv.getItem((short)(type == InventoryType.EQUIPPED ? -i : i));
                if (item != null)
                {
                    if (itemIds.Contains(item.getItemId()))
                    {
                        RemoveItemBySlotCore(type, item.getPosition(), item.getQuantity(), fromDrop, false);
                    }
                }
            }
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

        readonly ConditionalWeakTable<Item, UseItemAction> _itemLocks = new();
        public UseItemCheck UseItem(Item item, short quantity, Func<bool> condition)
        {
            if (quantity <= 0)
                throw new BusinessFatalException("不合法的输入：消耗物品消耗的数量不能为负数");

            if (item.getQuantity() < quantity)
                return UseItemCheck.QuantityNotEnough;

            var itemLock = _itemLocks.GetValue(item, _ => new UseItemAction(quantity));

            lock (itemLock)
            {
                if (!condition.Invoke())
                    return UseItemCheck.NotPass;

                RemoveItemBySlot(item.getInventoryType(), item.getPosition(), (short)(-quantity), false);
                return UseItemCheck.Success;
            }
        }

        readonly Lock buyCashItemLock = new Lock();
        public void BuyCashItem(int cashType, CashItem cItem, Func<bool> condition)
        {
            lock (buyCashItemLock)
            {
                if (cItem.getPrice() > CashShopModel.getCash(cashType))
                    return;

                if (!condition.Invoke())
                    return ;

                CashShopModel.BuyCashItem(cashType, cItem);
            }
        }

        public UseItemCheck TryUseItem(Item item, short quantity)
        {
            if (quantity <= 0)
                throw new BusinessFatalException("不合法的输入：消耗物品消耗的数量不能为负数");

            if (item.getQuantity() < quantity)
                return UseItemCheck.QuantityNotEnough;

            if (!_itemLocks.TryAdd(item, new UseItemAction(quantity)))
                return UseItemCheck.InProgressing;

            return UseItemCheck.Success;
        }

        public void CancelUseItem(Item item)
        {
            _itemLocks.Remove(item);
        }

        public void CommitUseItem(Item item)
        {
            if (_itemLocks.TryGetValue(item, out var action))
            {
                _itemLocks.Remove(item);
                RemoveItemBySlot(item.getInventoryType(), item.getPosition(), (short)(-action.Quantity), false);
            }
        }
    }
}
