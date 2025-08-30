/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */




using Application.Core.Channel.Net;
using Application.Core.Client;
using Application.Module.Marriage.Common.Models;
using Application.Shared.Constants.Inventory;
using Application.Shared.Constants.Item;
using Application.Shared.Net;
using Application.Utility.Compatible;
using Application.Utility.Configs;
using client.inventory;
using client.inventory.manipulator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using tools;

namespace Application.Module.Marriage.Channel.Net.Handlers;

/**
 * @author Drago (Dragohe4rt)
 */
public class WeddingHandler : ChannelHandlerBase
{
    readonly ILogger<WeddingHandler> _logger;
    readonly WeddingManager _weddingManager;
    readonly Configs _config;

    public WeddingHandler(ILogger<WeddingHandler> logger, WeddingManager weddingManager, IOptions<Configs> options)
    {
        _logger = logger;
        _weddingManager = weddingManager;
        _config = options.Value;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {

        if (c.tryacquireClient())
        {
            try
            {
                var chr = c.OnlinedCharacter;
                byte mode = p.readByte();

                if (mode == 6)
                {
                    throw new NotSupportedException();
                    //additem
                    short slot = p.readShort();
                    int itemid = p.readInt();
                    short quantity = p.readShort();

                    var marriage = _weddingManager.GetMarriageInstance(chr);
                    if (marriage != null)
                    {
                        try
                        {
                            bool groomWishlist = marriage.giftItemToSpouse(chr.getId());
                            string groomWishlistProp = "giftedItem" + (groomWishlist ? "G" : "B") + chr.getId();

                            int giftCount = marriage.getIntProperty(groomWishlistProp);
                            if (giftCount < _config.WEDDING_GIFT_LIMIT)
                            {
                                int cid = marriage.getIntProperty(groomWishlist ? "groomId" : "brideId");
                                if (chr.getId() != cid)
                                {   // cannot gift yourself
                                    var spouse = marriage.getPlayerById(cid);
                                    if (spouse != null)
                                    {
                                        InventoryType type = ItemConstants.getInventoryType(itemid);
                                        Inventory chrInv = chr.getInventory(type);

                                        Item? newItem = null;
                                        chrInv.lockInventory();
                                        try
                                        {
                                            var item = chrInv.getItem((byte)slot);
                                            if (item != null)
                                            {
                                                if (!item.isUntradeable())
                                                {
                                                    if (itemid == item.getItemId() && quantity <= item.getQuantity())
                                                    {
                                                        newItem = item.copy();
                                                        newItem.setQuantity(quantity);
                                                        marriage.addGiftItem(groomWishlist, newItem);
                                                        InventoryManipulator.removeFromSlot(c, type, slot, quantity, false, false);

                                                        KarmaManipulator.toggleKarmaFlagToUntradeable(newItem);
                                                        marriage.setIntProperty(groomWishlistProp, giftCount + 1);

                                                        c.sendPacket(WeddingPackets.onWeddingGiftResult(0xB, marriage.getWishlistItems(groomWishlist), Collections.singletonList(newItem)));
                                                    }
                                                }
                                                else
                                                {
                                                    c.sendPacket(WeddingPackets.onWeddingGiftResult(0xE, marriage.getWishlistItems(groomWishlist), null));
                                                }
                                            }
                                        }
                                        finally
                                        {
                                            chrInv.unlockInventory();
                                        }

                                        if (newItem != null)
                                        {
                                            if (YamlConfig.config.server.USE_ENFORCE_MERCHANT_SAVE)
                                            {
                                                chr.saveCharToDB();
                                            }
                                            _weddingManager.StoreGifts(marriage, groomWishlist);
                                        }
                                    }
                                    else
                                    {
                                        c.sendPacket(WeddingPackets.onWeddingGiftResult(0xE, marriage.getWishlistItems(groomWishlist), null));
                                    }
                                }
                                else
                                {
                                    c.sendPacket(WeddingPackets.onWeddingGiftResult(0xE, marriage.getWishlistItems(groomWishlist), null));
                                }
                            }
                            else
                            {
                                c.sendPacket(WeddingPackets.onWeddingGiftResult(0xC, marriage.getWishlistItems(groomWishlist), null));
                            }
                        }
                        catch (FormatException e)
                        {
                            _logger.LogError(e.ToString());
                        }
                    }
                    else
                    {
                        c.sendPacket(PacketCreator.enableActions());
                    }
                }
                else if (mode == 7)
                {
                    throw new NotSupportedException();
                    // take items
                    p.readByte();    // invType
                    int itemPos = p.ReadSByte();

                    var marriage = _weddingManager.GetMarriageInstance(chr);
                    if (marriage != null)
                    {
                        var groomWishlist = marriage.isMarriageGroom(chr);
                        if (groomWishlist != null)
                        {
                            var item = marriage.getGiftItem(c, groomWishlist.Value, itemPos);
                            if (item != null)
                            {
                                if (Inventory.checkSpot(chr, item))
                                {
                                    marriage.removeGiftItem(groomWishlist.Value, item);
                                    // marriage.saveGiftItemsToDb(c, groomWishlist.Value, chr.getId());

                                    InventoryManipulator.addFromDrop(c, item, true);

                                    c.sendPacket(WeddingPackets.onWeddingGiftResult(0xF, marriage.getWishlistItems(groomWishlist.Value), marriage.getGiftItems(c, groomWishlist.Value)));
                                }
                                else
                                {
                                    c.OnlinedCharacter.dropMessage(1, "Free a slot on your inventory before collecting this item.");
                                    c.sendPacket(WeddingPackets.onWeddingGiftResult(0xE, marriage.getWishlistItems(groomWishlist.Value), marriage.getGiftItems(c, groomWishlist.Value)));
                                }
                            }
                            else
                            {
                                c.OnlinedCharacter.dropMessage(1, "You have already collected this item.");
                                c.sendPacket(WeddingPackets.onWeddingGiftResult(0xE, marriage.getWishlistItems(groomWishlist.Value), marriage.getGiftItems(c, groomWishlist.Value)));
                            }
                        }
                    }
                    else
                    {
                        _weddingManager.TakeItemFromGifts(c.OnlinedCharacter, itemPos);
                    }
                }
                else if (mode == 8)
                {
                    // out of Wedding Registry
                    c.sendPacket(PacketCreator.enableActions());
                }
                else
                {
                    _logger.LogWarning("Unhandled wedding mode: {WeddingMode}", mode);
                }
            }
            finally
            {
                c.releaseClient();
            }
        }
    }
}
