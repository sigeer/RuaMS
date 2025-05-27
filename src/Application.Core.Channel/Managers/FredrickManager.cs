using client.inventory;
using client.inventory.manipulator;
using Microsoft.EntityFrameworkCore;
using Serilog;
using server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tools;

namespace Application.Core.Channel.Managers
{
    internal class FredrickManager
    {
        private static byte canRetrieveFromFredrick(Player chr, List<ItemInventoryType> items)
        {
            if (!Inventory.checkSpotsAndOwnership(chr, items))
            {
                List<int> itemids = new();
                foreach (var it in items)
                {
                    itemids.Add(it.Item.getItemId());
                }

                if (chr.canHoldUniques(itemids))
                {
                    return 0x22;
                }
                else
                {
                    return 0x20;
                }
            }

            int netMeso = chr.getMerchantNetMeso();
            if (netMeso > 0)
            {
                if (!chr.canHoldMeso(netMeso))
                {
                    return 0x1F;
                }
            }
            else
            {
                if (chr.getMeso() < -1 * netMeso)
                {
                    return 0x21;
                }
            }

            return 0x0;
        }

        private static bool deleteFredrickItems(DBContext dbContext, int cid)
        {
            try
            {
                var typeValue = ItemFactory.MERCHANT.getValue();
                dbContext.Inventoryitems.Where(x => x.Type == typeValue && x.Characterid == cid).ExecuteDelete();
                return true;
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                return false;
            }
        }

        public void fredrickRetrieveItems(ChannelClient c)
        {     // thanks Gustav for pointing out the dupe on Fredrick handling
            if (c.tryacquireClient())
            {
                try
                {
                    var chr = c.OnlinedCharacter;

                    List<ItemInventoryType> items;
                    try
                    {
                        items = ItemFactory.MERCHANT.loadItems(chr.getId(), false);

                        byte response = canRetrieveFromFredrick(chr, items);
                        if (response != 0)
                        {
                            chr.sendPacket(PacketCreator.fredrickMessage(response));
                            return;
                        }

                        chr.withdrawMerchantMesos();

                        using var dbContext = new DBContext();
                        if (deleteFredrickItems(dbContext, chr.getId()))
                        {
                            var merchant = chr.getHiredMerchant();

                            if (merchant != null)
                            {
                                merchant.clearItems();
                            }

                            foreach (var it in items)
                            {
                                Item item = it.Item;
                                InventoryManipulator.addFromDrop(chr.Client, item, false);
                                var itemName = ItemInformationProvider.getInstance().getName(item.getItemId());
                                log.Debug("Chr {CharacterName} gained {ItemQuantity}x {ItemName} ({CharacterId})", chr.getName(), item.getQuantity(), itemName, item.getItemId());
                            }

                            chr.sendPacket(PacketCreator.fredrickMessage(0x1E));
                            removeFredrickLog(dbContext, chr.getId());
                        }
                        else
                        {
                            chr.message("An unknown error has occured.");
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex.ToString());
                    }
                }
                finally
                {
                    c.releaseClient();
                }
            }
        }

        public static void removeFredrickLog(DBContext dbContext, int cid)
        {
            try
            {
                dbContext.Fredstorages.Where(x => x.Cid == cid).ExecuteDelete();
            }
            catch (Exception sqle)
            {
                log.Error(sqle.ToString());
            }
        }
    }
}
