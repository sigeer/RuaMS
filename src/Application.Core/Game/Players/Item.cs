using Application.Core.scripting.Event;
using client.inventory;
using client.inventory.manipulator;
using constants.inventory;
using server;
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
                itemExpireTask = TimerManager.getInstance().register(() =>
                {
                    bool deletedCoupon = false;

                    long expiration, currenttime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
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
                                else
                                {
                                    var pet = item.getPet();   // thanks Lame for noticing pets not getting despawned after expiration time
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
                                var replace = ii.getReplaceOnExpire(item.getItemId());
                                if (replace.Id > 0)
                                {
                                    toadd.Add(replace.Id);
                                    if (replace.Message.Length > 0)
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

                }, 60000);
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

        public bool haveItemWithId(int itemid, bool checkEquipped)
        {
            return (Bag[ItemConstants.getInventoryType(itemid)].findById(itemid) != null)
                    || (checkEquipped && Bag[InventoryType.EQUIPPED].findById(itemid) != null);
        }

        public bool haveItemEquipped(int itemid)
        {
            return (Bag[InventoryType.EQUIPPED].findById(itemid) != null);
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

    }
}
