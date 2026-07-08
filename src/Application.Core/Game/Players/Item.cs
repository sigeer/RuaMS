using Application.Core.Channel.DataProviders;
using Application.Core.Client.inventory;
using Application.Core.Game.Items;
using Application.Core.Game.Relation;
using Application.Templates.Etc;
using Application.Utility.Performance;
using client.inventory;
using client.inventory.manipulator;
using System.Diagnostics;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        private int itemEffect;

        public async Task ClearExpiredSkills(long now)
        {
            foreach (var skill in getSkills())
            {
                if (skill.Value.expiration != -1 && skill.Value.expiration < now)
                {
                    await changeSkillLevel(skill.Key, -1, 0, -1);
                }
            }
        }

        public AbstractInventory getInventory(InventoryType type)
        {
            return Bag[type];
        }

        public InventoryEquipped GetEquipped()
        {
            return (Bag[InventoryType.EQUIPPED] as InventoryEquipped) ?? throw new BusinessFatalException("装备栏未初始化");
        }

        public Inventory GetInventory(InventoryType type)
        {
            return (Bag[type] as Inventory) ?? throw new BusinessFatalException($"{type}栏未初始化");
        }

        public byte getSlots(int type)
        {
            return (byte)(type == InventoryType.CASH.getType() ? 96 : Bag[type].getSlotLimit());
        }

        public bool canGainSlots(int type, int slots)
        {
            return Bag[type].CanGainSlot((short)slots);
        }
        public async Task<bool> gainSlots(int type, int slots, bool update)
        {
            int newLimit = gainSlotsInternal(type, slots);
            if (newLimit != -1)
            {
                await SyncCharAsync();
                if (update)
                {
                    await SendPacket(PacketCreator.updateInventorySlotLimit(type, newLimit));
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

        [ScriptCall]
        public bool haveItemEquipped(int itemid)
        {
            return (Bag[InventoryType.EQUIPPED].findById(itemid) != null);
        }

        public bool hasEmptySlot(sbyte invType)
        {
            return getInventory(InventoryTypeUtils.getByType(invType)).getNextFreeSlot() > -1;
        }

        public int getItemQuantity(int itemid)
        {
            return Bag[ItemConstants.getInventoryType(itemid)].countById(itemid);
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="delta">修改值</param>
        /// <param name="check">是否验证范围，true且未通过验证时不会修改金币</param>
        /// <returns>实际获取的金币</returns>
        int ChangeMeso(int delta, bool check = false)
        {
            int notGain = 0;
            int actualGain = delta;
            var nextMeso = (long)MesoValue.get() + delta;

            if (nextMeso > int.MaxValue)
            {
                notGain = (int)(nextMeso - int.MaxValue);
                actualGain -= notGain;
            }
            else if (nextMeso < 0)
            {
                notGain = (int)nextMeso;
                actualGain = -MesoValue.get();
            }

            Activity.Current?.AddEvent(
                    new ActivityEvent(
                        "ChangeMeso",
                        tags: new ActivityTagsCollection
                        {
                            ["Gain"] = delta,
                            ["ActualGain"] = actualGain,
                        }));

            if (!check || actualGain == delta)
                MesoValue.addAndGet(actualGain);
            return actualGain;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gain"></param>
        /// <param name="d"></param>
        /// <param name="enableActions"></param>
        /// <returns>是否成功</returns>
        public async Task<bool> TryGainMeso(int gain, GainItemShow d = GainItemShow.NotShown, bool enableActions = false)
        {
            using var activity = GameMetrics.ActivitySource.StartActivity("PlayerGainMeso");
            activity?.SetTag("PlayerId", Id);
            activity?.SetTag("Player", Name);
            activity?.SetTag("Meso", gain);

            var actualGain = ChangeMeso(gain, true);
            if (actualGain != gain)
            {
                return false;
            }

            await updateSingleStat(Stat.MESO, MesoValue, enableActions);
            await GainMesoShowMessage(actualGain, d);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gain"></param>
        /// <param name="d"></param>
        /// <param name="enableActions"></param>
        /// <returns>超出上限后无法拾取的数量</returns>
        public async Task<int> GainMeso(int gain, GainItemShow d = GainItemShow.NotShown, bool enableActions = false)
        {
            using var activity = GameMetrics.ActivitySource.StartActivity("PlayerGainMeso");
            activity?.SetTag("PlayerId", Id);
            activity?.SetTag("Player", Name);
            activity?.SetTag("Meso", gain);

            var actualGain = ChangeMeso(gain);

            if (actualGain != 0)
            {
                await updateSingleStat(Stat.MESO, MesoValue, enableActions);
                await GainMesoShowMessage(actualGain, d);
            }
            else
            {
                await SendPacket(PacketCreator.enableActions());
            }
            return gain - actualGain;
        }
        #endregion

        public async Task GainItemShowMessage(int itemId, short quantity, GainItemShow d = GainItemShow.NotShown)
        {
            switch (d)
            {
                case Shared.Constants.Item.GainItemShow.NotShown:
                    break;
                case Shared.Constants.Item.GainItemShow.ShowInChat:
                    await SendPacket(PacketCreator.getShowItemGain(itemId, quantity, true));
                    break;
                case Shared.Constants.Item.GainItemShow.ShowInMessage:
                    await SendPacket(PacketCreator.getShowItemGain(itemId, quantity, false));
                    break;
                default:
                    break;
            }
        }

        async Task GainMesoShowMessage(int meso, GainItemShow d = GainItemShow.NotShown)
        {
            switch (d)
            {
                case Shared.Constants.Item.GainItemShow.NotShown:
                    break;
                case Shared.Constants.Item.GainItemShow.ShowInChat:
                    await SendPacket(PacketCreator.getShowMesoGain(meso, true));
                    break;
                case Shared.Constants.Item.GainItemShow.ShowInMessage:
                    await SendPacket(PacketCreator.getShowMesoGain(meso, false));
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemId">道具Id</param>
        /// <param name="quantity">数量，负数表示移除</param>
        /// <param name="randomStats">随机属性，仅在item为装备时有效</param>
        /// <param name="show">显示方式</param>
        /// <param name="expires">道具有效时长。单位ms， -1不会过期</param>
        /// <param name="nextSetter">设置其他道具属性，不能对返回值修改属性（要在传客户端前修改）</param>
        /// <returns>获得的道具，尽量不使用：如果是可叠放物品，不会与背包物品对应，</returns>
        public async Task<Item?> GainItem(int itemId, short quantity, bool randomStats = false, GainItemShow show = GainItemShow.NotShown, long expires = -1, Action<Item>? nextSetter = null)
        {
            if (quantity == 0)
            {
                return null;
            }

            using var activity = GameMetrics.ActivitySource.StartActivity("PlayerGainItem");
            activity?.SetTag("PlayerId", Id);
            activity?.SetTag("Player", Name);
            activity?.SetTag("ItemId", itemId);
            activity?.SetTag("Quantity", quantity);

            Item? item = null;

            var invType = ItemConstants.getInventoryType(itemId);
            if (quantity >= 0)
            {
                if (!InventoryManipulator.checkSpace(Client, itemId, quantity, ""))
                {
                    await dropMessage(1, "Your inventory is full. Please remove an item from your " + invType.ToString() + " inventory.");
                    return null;
                }

                ItemInformationProvider ii = ItemInformationProvider.getInstance();

                item = ii.GenerateVirtualItemById(itemId, quantity);
                if (item == null)
                    return null;

                if (item is Equip it)
                {
                    if (ItemConstants.isAccessory(item.getItemId()) && it.getUpgradeSlots() <= 0)
                    {
                        it.setUpgradeSlots(3);
                    }

                    // 手工制作时，使用提升属性（通过不消耗升级次数的混沌卷）
                    if (YamlConfig.config.server.USE_ENHANCED_CRAFTING == true && getCS() == true)
                    {
                        if (!(isGM() && YamlConfig.config.server.USE_PERFECT_GM_SCROLL))
                        {
                            it.setUpgradeSlots(it.getUpgradeSlots() + 1);
                        }
                        item = ItemInformationProvider.getInstance().scrollEquipWithId(it, ItemId.CHAOS_SCROll_60, true, ItemId.CHAOS_SCROll_60, isGM())!;
                    }

                    else if (randomStats)
                    {
                        ii.randomizeStats(it);
                    }
                }
                else if (item is Pet pet)
                {
                    pet.Name = Client.CurrentCulture.GetItemName(itemId) ?? "";
                }

                if (expires > 0)
                {
                    item.setExpiration(Client.CurrentServer.Node.getCurrentTime() + expires);
                }

                if (nextSetter != null)
                {
                    nextSetter(item);
                }

                var addItemResult = await InventoryManipulator.addFromDrop(Client, item!, false);
                if (!addItemResult)
                    return null;
            }
            else
            {
                await Bag.RemoveFromInventory(invType, -quantity, i => i.getItemId() == itemId, showMessage: show != GainItemShow.NotShown);
            }

            await GainItemShowMessage(itemId, quantity, show);

            return item;
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

        public async Task BuyCashItem(int cashType, CashCommodityTemplate cItem, Func<Task<bool>> condition)
        {
            if (cItem.Price > CashShopModel.getCash(cashType))
                return;

            if (!await condition.Invoke())
                return;

            CashShopModel.BuyCashItem(cashType, cItem);
            await SendPacket(PacketCreator.showCash(this));
        }
    }
}
