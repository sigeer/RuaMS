/*
	This file is part of the OdinMS Maple Story NewServer
    Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
		       Matthias Butz <matze@odinms.de>
		       Jan Christian Meyer <vimes@odinms.de>

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation version 3 as published by
    the Free Software Foundation. You may not use, modify or distribute
    this program under any other version of the GNU Affero General Public
    License.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/


using Application.Core.Game.Maps;
using client.autoban;
using client.inventory;
using client.inventory.manipulator;
using constants.game;
using constants.id;
using constants.inventory;
using net.packet;
using server;
using server.maps;
using tools;

namespace net.server.channel.handlers;


/**
 * @author Matze
 * @author Ronan - concurrency safety and reviewed minigames
 */
public class PlayerInteractionHandler : AbstractPacketHandler
{

    public enum Action
    {
        CREATE = 0,
        INVITE = 2,
        DECLINE = 3,
        VISIT = 4,
        ROOM = 5,
        CHAT = 6,
        CHAT_THING = 8,
        EXIT = 0xA,
        OPEN_STORE = 0xB,
        OPEN_CASH = 0xE,
        SET_ITEMS = 0xF,
        SET_MESO = 0x10,
        CONFIRM = 0x11,
        TRANSACTION = 0x14,
        ADD_ITEM = 0x16,
        BUY = 0x17,
        UPDATE_MERCHANT = 0x19,
        UPDATE_PLAYERSHOP = 0x1A,
        REMOVE_ITEM = 0x1B,
        BAN_PLAYER = 0x1C,
        MERCHANT_THING = 0x1D,
        OPEN_THING = 0x1E,
        PUT_ITEM = 0x21,
        MERCHANT_BUY = 0x22,
        TAKE_ITEM_BACK = 0x26,
        MAINTENANCE_OFF = 0x27,
        MERCHANT_ORGANIZE = 0x28,
        CLOSE_MERCHANT = 0x29,
        REAL_CLOSE_MERCHANT = 0x2A,
        MERCHANT_MESO = 0x2B,
        SOMETHING = 0x2D,
        VIEW_VISITORS = 0x2E,
        VIEW_BLACKLIST = 0x2F,
        ADD_TO_BLACKLIST = 0x30,
        REMOVE_FROM_BLACKLIST = 0x31,
        REQUEST_TIE = 0x32,
        ANSWER_TIE = 0x33,
        GIVE_UP = 0x34,
        EXIT_AFTER_GAME = 0x38,
        CANCEL_EXIT_AFTER_GAME = 0x39,
        READY = 0x3A,
        UN_READY = 0x3B,
        EXPEL = 0x3C,
        START = 0x3D,
        GET_RESULT = 0x3E,
        SKIP = 0x3F,
        MOVE_OMOK = 0x40,
        SELECT_CARD = 0x44
    }

    private static int establishMiniroomStatus(IPlayer chr, bool isMinigame)
    {
        if (isMinigame && FieldLimit.CANNOTMINIGAME.check(chr.getMap().getFieldLimit()))
        {
            return 11;
        }

        if (chr.getChalkboard() != null)
        {
            return 13;
        }

        if (chr.getEventInstance() != null)
        {
            return 5;
        }

        return 0;
    }

    public override void HandlePacket(InPacket p, IClient c)
    {
        if (!c.tryacquireClient())
        {    // thanks GabrielSin for pointing dupes within player interactions
            c.sendPacket(PacketCreator.enableActions());
            return;
        }

        try
        {
            byte mode = p.readByte();
            var chr = c.OnlinedCharacter;

            if (mode == Action.CREATE.getCode())
            {
                if (!chr.isAlive())
                {    // thanks GabrielSin for pointing this
                    chr.sendPacket(PacketCreator.getMiniRoomError(4));
                    return;
                }

                byte createType = p.readByte();
                if (createType == 3)
                {  // trade
                    Trade.startTrade(chr);
                }
                else if (createType == 1)
                { // omok mini game
                    int status = establishMiniroomStatus(chr, true);
                    if (status > 0)
                    {
                        chr.sendPacket(PacketCreator.getMiniRoomError(status));
                        return;
                    }

                    string desc = p.readString();
                    string pw;

                    if (p.readByte() != 0)
                    {
                        pw = p.readString();
                    }
                    else
                    {
                        pw = "";
                    }

                    int type = p.readByte();
                    if (type > 11)
                    {
                        type = 11;
                    }
                    else if (type < 0)
                    {
                        type = 0;
                    }
                    if (!chr.haveItem(ItemId.MINI_GAME_BASE + type))
                    {
                        chr.sendPacket(PacketCreator.getMiniRoomError(6));
                        return;
                    }

                    MiniGame game = new MiniGame(chr, desc, pw);
                    chr.setMiniGame(game);
                    game.setPieceType(type);
                    game.setGameType(MiniGameType.OMOK);
                    chr.getMap().addMapObject(game);
                    chr.getMap().broadcastMessage(PacketCreator.addOmokBox(chr, 1, 0));
                    game.sendOmok(c, type);
                }
                else if (createType == 2)
                { // matchcard
                    int status = establishMiniroomStatus(chr, true);
                    if (status > 0)
                    {
                        chr.sendPacket(PacketCreator.getMiniRoomError(status));
                        return;
                    }

                    string desc = p.readString();
                    string pw;

                    if (p.readByte() != 0)
                    {
                        pw = p.readString();
                    }
                    else
                    {
                        pw = "";
                    }

                    int type = p.readByte();
                    if (type > 2)
                    {
                        type = 2;
                    }
                    else if (type < 0)
                    {
                        type = 0;
                    }
                    if (!chr.haveItem(ItemId.MATCH_CARDS))
                    {
                        chr.sendPacket(PacketCreator.getMiniRoomError(6));
                        return;
                    }

                    MiniGame game = new MiniGame(chr, desc, pw);
                    game.setPieceType(type);
                    if (type == 0)
                    {
                        game.setMatchesToWin(6);
                    }
                    else if (type == 1)
                    {
                        game.setMatchesToWin(10);
                    }
                    else if (type == 2)
                    {
                        game.setMatchesToWin(15);
                    }
                    game.setGameType(MiniGameType.MATCH_CARD);
                    chr.setMiniGame(game);
                    chr.getMap().addMapObject(game);
                    chr.getMap().broadcastMessage(PacketCreator.addMatchCardBox(chr, 1, 0));
                    game.sendMatchCard(c, type);
                }
                else if (createType == 4 || createType == 5)
                { // shop
                    if (!GameConstants.isFreeMarketRoom(chr.getMapId()))
                    {
                        chr.sendPacket(PacketCreator.getMiniRoomError(15));
                        return;
                    }

                    int status = establishMiniroomStatus(chr, false);
                    if (status > 0)
                    {
                        chr.sendPacket(PacketCreator.getMiniRoomError(status));
                        return;
                    }

                    if (!canPlaceStore(chr))
                    {
                        return;
                    }

                    string desc = p.readString();
                    p.skip(3);
                    int itemId = p.readInt();
                    if (chr.getInventory(InventoryType.CASH).countById(itemId) < 1)
                    {
                        chr.sendPacket(PacketCreator.getMiniRoomError(6));
                        return;
                    }

                    if (ItemConstants.isPlayerShop(itemId))
                    {
                        PlayerShop shop = new PlayerShop(chr, desc, itemId);
                        chr.setPlayerShop(shop);
                        chr.getMap().addMapObject(shop);
                        shop.sendShop(c);
                        c.getWorldServer().registerPlayerShop(shop);
                        //c.sendPacket(PacketCreator.getPlayerShopRemoveVisitor(1));
                    }
                    else if (ItemConstants.isHiredMerchant(itemId))
                    {
                        HiredMerchant merchant = new HiredMerchant(chr, desc, itemId);
                        chr.setHiredMerchant(merchant);
                        c.getWorldServer().registerHiredMerchant(merchant);
                        chr.getClient().getChannelServer().addHiredMerchant(chr.getId(), merchant);
                        chr.sendPacket(PacketCreator.getHiredMerchant(chr, merchant, true));
                    }
                }
            }
            else if (mode == Action.INVITE.getCode())
            {
                int otherCid = p.readInt();
                var other = chr.getMap().getCharacterById(otherCid);
                if (other == null || chr.getId() == other.getId())
                {
                    return;
                }

                Trade.inviteTrade(chr, other);
            }
            else if (mode == Action.DECLINE.getCode())
            {
                Trade.declineTrade(chr);
            }
            else if (mode == Action.VISIT.getCode())
            {
                var tradeObj = chr.getTrade();
                if (tradeObj != null && tradeObj.getPartner() != null)
                {
                    if (!tradeObj.isFullTrade() && !tradeObj.getPartner().isFullTrade())
                    {
                        Trade.visitTrade(chr, tradeObj.getPartner().getChr());
                    }
                    else
                    {
                        chr.sendPacket(PacketCreator.getMiniRoomError(2));
                        return;
                    }
                }
                else
                {
                    if (isTradeOpen(chr))
                    {
                        return;
                    }

                    int oid = p.readInt();
                    var ob = chr.getMap().getMapObject(oid);
                    if (ob is PlayerShop shop)
                    {
                        shop.visitShop(chr);
                    }
                    else if (ob is MiniGame game)
                    {
                        p.skip(1);
                        string pw = p.available() > 1 ? p.readString() : "";

                        if (game.checkPassword(pw))
                        {
                            if (game.hasFreeSlot() && !game.isVisitor(chr))
                            {
                                game.addVisitor(chr);
                                chr.setMiniGame(game);
                                switch (game.getGameType())
                                {
                                    case MiniGameType.OMOK:
                                        game.sendOmok(c, game.getPieceType());
                                        break;
                                    case MiniGameType.MATCH_CARD:
                                        game.sendMatchCard(c, game.getPieceType());
                                        break;
                                }
                            }
                            else
                            {
                                chr.sendPacket(PacketCreator.getMiniRoomError(2));
                            }
                        }
                        else
                        {
                            chr.sendPacket(PacketCreator.getMiniRoomError(22));
                        }
                    }
                    else if (ob is HiredMerchant merchant && chr.getHiredMerchant() == null)
                    {
                        merchant.visitShop(chr);
                    }
                }
            }
            else if (mode == Action.CHAT.getCode())
            { // chat lol
                var merchant = chr.getHiredMerchant();
                if (chr.getTrade() != null)
                {
                    chr.getTrade()!.chat(p.readString());
                }
                else if (chr.getPlayerShop() != null)
                { //mini game
                    var shop = chr.getPlayerShop();
                    if (shop != null)
                    {
                        shop.chat(c, p.readString());
                    }
                }
                else if (chr.getMiniGame() != null)
                {
                    var game = chr.getMiniGame();
                    if (game != null)
                    {
                        game.chat(c, p.readString());
                    }
                }
                else if (merchant != null)
                {
                    merchant.sendMessage(chr, p.readString());
                }
            }
            else if (mode == Action.EXIT.getCode())
            {
                if (chr.getTrade() != null)
                {
                    Trade.cancelTrade(chr, TradeResult.PARTNER_CANCEL);
                }
                else
                {
                    chr.closePlayerShop();
                    chr.closeMiniGame(false);
                    chr.closeHiredMerchant(true);
                }
            }
            else if (mode == Action.OPEN_STORE.getCode() || mode == Action.OPEN_CASH.getCode())
            {
                if (isTradeOpen(chr))
                {
                    return;
                }

                if (mode == Action.OPEN_STORE.getCode())
                {
                    p.readByte();    //01
                }
                else
                {
                    p.readShort();
                    int birthday = p.readInt();
                    if (!CashOperationHandler.checkBirthday(c, birthday))
                    { // birthday check here found thanks to lucasziron
                        c.sendPacket(PacketCreator.serverNotice(1, "Please check again the birthday date."));
                        return;
                    }

                    c.sendPacket(PacketCreator.hiredMerchantOwnerMaintenanceLeave());
                }

                if (!canPlaceStore(chr))
                {    // thanks Ari for noticing player shops overlapping on opening time
                    return;
                }

                var shop = chr.getPlayerShop();
                var merchant = chr.getHiredMerchant();
                if (shop != null && shop.isOwner(chr))
                {
                    if (YamlConfig.config.server.USE_ERASE_PERMIT_ON_OPENSHOP)
                    {
                        try
                        {
                            InventoryManipulator.removeById(c, InventoryType.CASH, shop.getItemId(), 1, true, false);
                        }
                        catch (Exception)
                        {
                        } // fella does not have a player shop permit...
                    }

                    chr.getMap().broadcastMessage(PacketCreator.updatePlayerShopBox(shop));
                    shop.setOpen(true);
                }
                else if (merchant != null && merchant.isOwner(chr))
                {
                    chr.setHasMerchant(true);
                    merchant.setOpen(true);
                    chr.getMap().addMapObject(merchant);
                    chr.setHiredMerchant(null);
                    chr.getMap().broadcastMessage(PacketCreator.spawnHiredMerchantBox(merchant));
                }
            }
            else if (mode == Action.READY.getCode())
            {
                var game = chr.getMiniGame();
                game?.broadcast(PacketCreator.getMiniGameReady(game));
            }
            else if (mode == Action.UN_READY.getCode())
            {
                var game = chr.getMiniGame();
                game?.broadcast(PacketCreator.getMiniGameUnReady(game));
            }
            else if (mode == Action.START.getCode())
            {
                var game = chr.getMiniGame();
                if (game.getGameType().Equals(MiniGameType.OMOK))
                {
                    game.minigameMatchStarted();
                    game.broadcast(PacketCreator.getMiniGameStart(game, game.getLoser()));
                    chr.getMap().broadcastMessage(PacketCreator.addOmokBox(game.getOwner(), 2, 1));
                }
                else if (game.getGameType().Equals(MiniGameType.MATCH_CARD))
                {
                    game.minigameMatchStarted();
                    game.shuffleList();
                    game.broadcast(PacketCreator.getMatchCardStart(game, game.getLoser()));
                    chr.getMap().broadcastMessage(PacketCreator.addMatchCardBox(game.getOwner(), 2, 1));
                }
            }
            else if (mode == Action.GIVE_UP.getCode())
            {
                var game = chr.getMiniGame();
                if (game.getGameType().Equals(MiniGameType.OMOK))
                {
                    if (game.isOwner(chr))
                    {
                        game.minigameMatchVisitorWins(true);
                    }
                    else
                    {
                        game.minigameMatchOwnerWins(true);
                    }
                }
                else if (game.getGameType().Equals(MiniGameType.MATCH_CARD))
                {
                    if (game.isOwner(chr))
                    {
                        game.minigameMatchVisitorWins(true);
                    }
                    else
                    {
                        game.minigameMatchOwnerWins(true);
                    }
                }
            }
            else if (mode == Action.REQUEST_TIE.getCode())
            {
                var game = chr.getMiniGame()!;
                if (!game.isTieDenied(chr))
                {
                    if (game.isOwner(chr))
                    {
                        game.broadcastToVisitor(PacketCreator.getMiniGameRequestTie(game));
                    }
                    else
                    {
                        game.broadcastToOwner(PacketCreator.getMiniGameRequestTie(game));
                    }
                }
            }
            else if (mode == Action.ANSWER_TIE.getCode())
            {
                var game = chr.getMiniGame()!;
                if (p.readByte() != 0)
                {
                    game.minigameMatchDraw();
                }
                else
                {
                    game.denyTie(chr);

                    if (game.isOwner(chr))
                    {
                        game.broadcastToVisitor(PacketCreator.getMiniGameDenyTie(game));
                    }
                    else
                    {
                        game.broadcastToOwner(PacketCreator.getMiniGameDenyTie(game));
                    }
                }
            }
            else if (mode == Action.SKIP.getCode())
            {
                var game = chr.getMiniGame()!;
                if (game.isOwner(chr))
                {
                    game.broadcast(PacketCreator.getMiniGameSkipOwner(game));
                }
                else
                {
                    game.broadcast(PacketCreator.getMiniGameSkipVisitor(game));
                }
            }
            else if (mode == Action.MOVE_OMOK.getCode())
            {
                int x = p.readInt(); // x point
                int y = p.readInt(); // y point
                int type = p.readByte(); // piece ( 1 or 2; Owner has one piece, visitor has another, it switches every game.)
                chr.getMiniGame()!.setPiece(x, y, type, chr);
            }
            else if (mode == Action.SELECT_CARD.getCode())
            {
                int turn = p.readByte(); // 1st turn = 1; 2nd turn = 0
                int slot = p.readByte(); // slot
                var game = chr.getMiniGame()!;
                int firstslot = game.getFirstSlot();
                if (turn == 1)
                {
                    game.setFirstSlot(slot);
                    if (game.isOwner(chr))
                    {
                        game.broadcastToVisitor(PacketCreator.getMatchCardSelect(game, turn, slot, firstslot, turn));
                    }
                    else
                    {
                        game.getOwner().sendPacket(PacketCreator.getMatchCardSelect(game, turn, slot, firstslot, turn));
                    }
                }
                else if ((game.getCardId(firstslot)) == (game.getCardId(slot)))
                {
                    if (game.isOwner(chr))
                    {
                        game.broadcast(PacketCreator.getMatchCardSelect(game, turn, slot, firstslot, 2));
                        game.setOwnerPoints();
                    }
                    else
                    {
                        game.broadcast(PacketCreator.getMatchCardSelect(game, turn, slot, firstslot, 3));
                        game.setVisitorPoints();
                    }
                }
                else if (game.isOwner(chr))
                {
                    game.broadcast(PacketCreator.getMatchCardSelect(game, turn, slot, firstslot, 0));
                }
                else
                {
                    game.broadcast(PacketCreator.getMatchCardSelect(game, turn, slot, firstslot, 1));
                }
            }
            else if (mode == Action.SET_MESO.getCode())
            {
                chr.getTrade()!.setMeso(p.readInt());
            }
            else if (mode == Action.SET_ITEMS.getCode())
            {
                ItemInformationProvider ii = ItemInformationProvider.getInstance();
                InventoryType ivType = InventoryTypeUtils.getByType(p.ReadSByte());
                short pos = p.readShort();
                var item = chr.getInventory(ivType).getItem(pos);
                short quantity = p.readShort();
                byte targetSlot = p.readByte();

                if (targetSlot < 1 || targetSlot > 9)
                {
                    log.Warning("[Hack] Chr {CharacterName} Trying to dupe on trade slot.", chr.getName());
                    c.sendPacket(PacketCreator.enableActions());
                    return;
                }

                if (item == null)
                {
                    c.sendPacket(PacketCreator.serverNotice(1, "Invalid item description."));
                    c.sendPacket(PacketCreator.enableActions());
                    return;
                }

                if (ii.isUnmerchable(item.getItemId()))
                {
                    if (ItemConstants.isPet(item.getItemId()))
                    {
                        c.sendPacket(PacketCreator.serverNotice(1, "Pets are not allowed to be traded."));
                    }
                    else
                    {
                        c.sendPacket(PacketCreator.serverNotice(1, "Cash items are not allowed to be traded."));
                    }

                    c.sendPacket(PacketCreator.enableActions());
                    return;
                }

                if (quantity < 1 || quantity > item.getQuantity())
                {
                    c.sendPacket(PacketCreator.serverNotice(1, "You don't have enough quantity of the item."));
                    c.sendPacket(PacketCreator.enableActions());
                    return;
                }

                var trade = chr.getTrade();
                if (trade != null)
                {
                    if ((quantity <= item.getQuantity() && quantity >= 0) || ItemConstants.isRechargeable(item.getItemId()))
                    {
                        if (ii.isDropRestricted(item.getItemId()))
                        { // ensure that undroppable items do not make it to the trade window
                            if (!KarmaManipulator.hasKarmaFlag(item))
                            {
                                c.sendPacket(PacketCreator.serverNotice(1, "That item is untradeable."));
                                c.sendPacket(PacketCreator.enableActions());
                                return;
                            }
                        }

                        Inventory inv = chr.getInventory(ivType);
                        inv.lockInventory();
                        try
                        {
                            var checkItem = chr.getInventory(ivType).getItem(pos);
                            if (checkItem != item || checkItem.getPosition() != item.getPosition())
                            {
                                c.sendPacket(PacketCreator.serverNotice(1, "Invalid item description."));
                                c.sendPacket(PacketCreator.enableActions());
                                return;
                            }

                            Item tradeItem = item.copy();
                            if (ItemConstants.isRechargeable(item.getItemId()))
                            {
                                quantity = item.getQuantity();
                            }

                            tradeItem.setQuantity(quantity);
                            tradeItem.setPosition(targetSlot);

                            if (trade.addItem(tradeItem))
                            {
                                InventoryManipulator.removeFromSlot(c, ivType, item.getPosition(), quantity, true);

                                trade.getChr().sendPacket(PacketCreator.getTradeItemAdd(0, tradeItem));
                                if (trade.getPartner() != null)
                                {
                                    trade.getPartner().getChr().sendPacket(PacketCreator.getTradeItemAdd(1, tradeItem));
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            log.Warning(e, "Chr {CharacterName} tried to add {ItemName}x {ItemQuantity} in trade (slot {ItemSlot}), then exception occurred", chr, ii.getName(item.getItemId()), item.getQuantity(), targetSlot);
                        }
                        finally
                        {
                            inv.unlockInventory();
                        }
                    }
                }
            }
            else if (mode == Action.CONFIRM.getCode())
            {
                Trade.completeTrade(chr);
            }
            else if (mode == Action.ADD_ITEM.getCode() || mode == Action.PUT_ITEM.getCode())
            {
                if (isTradeOpen(chr))
                {
                    return;
                }

                InventoryType ivType = InventoryTypeUtils.getByType(p.ReadSByte());
                short slot = p.readShort();
                short bundles = p.readShort();
                var ivItem = chr.getInventory(ivType).getItem(slot);

                if (ivItem == null || ivItem.isUntradeable())
                {
                    c.sendPacket(PacketCreator.serverNotice(1, "Could not perform shop operation with that item."));
                    c.sendPacket(PacketCreator.enableActions());
                    return;
                }
                else if (ItemInformationProvider.getInstance().isUnmerchable(ivItem.getItemId()))
                {
                    if (ItemConstants.isPet(ivItem.getItemId()))
                    {
                        c.sendPacket(PacketCreator.serverNotice(1, "Pets are not allowed to be sold on the Player Store."));
                    }
                    else
                    {
                        c.sendPacket(PacketCreator.serverNotice(1, "Cash items are not allowed to be sold on the Player Store."));
                    }

                    c.sendPacket(PacketCreator.enableActions());
                    return;
                }

                short perBundle = p.readShort();

                if (ItemConstants.isRechargeable(ivItem.getItemId()))
                {
                    perBundle = 1;
                    bundles = 1;
                }
                else if (ivItem.getQuantity() < (bundles * perBundle))
                {     // thanks GabrielSin for finding a dupe here
                    c.sendPacket(PacketCreator.serverNotice(1, "Could not perform shop operation with that item."));
                    c.sendPacket(PacketCreator.enableActions());
                    return;
                }

                int price = p.readInt();
                if (perBundle <= 0 || perBundle * bundles > 2000 || bundles <= 0 || price <= 0 || price > int.MaxValue)
                {
                    AutobanFactory.PACKET_EDIT.alert(chr, chr.getName() + " tried to packet edit with hired merchants.");
                    log.Warning("Chr {CharacterName} might possibly have packet edited Hired Merchants. perBundle: {0}, perBundle * bundles (This multiplied cannot be greater than 2000): {1}, bundles: {2}, price: {Price}",
                            chr.getName(), perBundle, perBundle * bundles, bundles, price);
                    return;
                }

                Item sellItem = ivItem.copy();
                if (!ItemConstants.isRechargeable(ivItem.getItemId()))
                {
                    sellItem.setQuantity(perBundle);
                }

                PlayerShopItem shopItem = new PlayerShopItem(sellItem, bundles, price);
                var shop = chr.getPlayerShop();
                var merchant = chr.getHiredMerchant();
                if (shop != null && shop.isOwner(chr))
                {
                    if (shop.isOpen() || !shop.addItem(shopItem))
                    { // thanks Vcoc for pointing an exploit with unlimited shop slots
                        c.sendPacket(PacketCreator.serverNotice(1, "You can't sell it anymore."));
                        return;
                    }

                    if (ItemConstants.isRechargeable(ivItem.getItemId()))
                    {
                        InventoryManipulator.removeFromSlot(c, ivType, slot, ivItem.getQuantity(), true);
                    }
                    else
                    {
                        InventoryManipulator.removeFromSlot(c, ivType, slot, (short)(bundles * perBundle), true);
                    }

                    c.sendPacket(PacketCreator.getPlayerShopItemUpdate(shop));
                }
                else if (merchant != null && merchant.isOwner(chr))
                {
                    if (ivType.Equals(InventoryType.CASH) && merchant.isPublished())
                    {
                        c.sendPacket(PacketCreator.serverNotice(1, "Cash items are only allowed to be sold when first opening the store."));
                        return;
                    }

                    if (merchant.isOpen() || !merchant.addItem(shopItem))
                    { // thanks Vcoc for pointing an exploit with unlimited shop slots
                        c.sendPacket(PacketCreator.serverNotice(1, "You can't sell it anymore."));
                        return;
                    }

                    if (ItemConstants.isRechargeable(ivItem.getItemId()))
                    {
                        InventoryManipulator.removeFromSlot(c, ivType, slot, ivItem.getQuantity(), true);
                    }
                    else
                    {
                        InventoryManipulator.removeFromSlot(c, ivType, slot, (short)(bundles * perBundle), true);
                    }

                    c.sendPacket(PacketCreator.updateHiredMerchant(merchant, chr));

                    if (YamlConfig.config.server.USE_ENFORCE_MERCHANT_SAVE)
                    {
                        chr.saveCharToDB(false);
                    }

                    try
                    {
                        merchant.saveItems(false);   // thanks Masterrulax for realizing yet another dupe with merchants/Fredrick
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex.ToString());
                    }
                }
                else
                {
                    c.sendPacket(PacketCreator.serverNotice(1, "You can't sell without owning a shop."));
                }
            }
            else if (mode == Action.REMOVE_ITEM.getCode())
            {
                if (isTradeOpen(chr))
                {
                    return;
                }

                var shop = chr.getPlayerShop();
                if (shop != null && shop.isOwner(chr))
                {
                    if (shop.isOpen())
                    {
                        c.sendPacket(PacketCreator.serverNotice(1, "You can't take it with the store open."));
                        return;
                    }

                    int slot = p.readShort();
                    if (slot >= shop.getItems().Count || slot < 0)
                    {
                        AutobanFactory.PACKET_EDIT.alert(chr, chr.getName() + " tried to packet edit with a player shop.");
                        log.Warning("Chr {CharacterName} tried to remove item at slot {Slot}", chr.getName(), slot);
                        c.disconnect(true, false);
                        return;
                    }

                    shop.takeItemBack(slot, chr);
                }
            }
            else if (mode == Action.MERCHANT_MESO.getCode())
            {
                var merchant = chr.getHiredMerchant();
                if (merchant == null)
                {
                    return;
                }

                merchant.withdrawMesos(chr);

            }
            else if (mode == Action.VIEW_VISITORS.getCode())
            {
                var merchant = chr.getHiredMerchant();
                if (merchant == null || !merchant.isOwner(chr))
                {
                    return;
                }
                c.sendPacket(PacketCreator.viewMerchantVisitorHistory(merchant.getVisitorHistory()));
            }
            else if (mode == Action.VIEW_BLACKLIST.getCode())
            {
                var merchant = chr.getHiredMerchant();
                if (merchant == null || !merchant.isOwner(chr))
                {
                    return;
                }

                c.sendPacket(PacketCreator.viewMerchantBlacklist(merchant.getBlacklist()));
            }
            else if (mode == Action.ADD_TO_BLACKLIST.getCode())
            {
                var merchant = chr.getHiredMerchant();
                if (merchant == null || !merchant.isOwner(chr))
                {
                    return;
                }
                string chrName = p.readString();
                merchant.addToBlacklist(chrName);
            }
            else if (mode == Action.REMOVE_FROM_BLACKLIST.getCode())
            {
                var merchant = chr.getHiredMerchant();
                if (merchant == null || !merchant.isOwner(chr))
                {
                    return;
                }
                string chrName = p.readString();
                merchant.removeFromBlacklist(chrName);
            }
            else if (mode == Action.MERCHANT_ORGANIZE.getCode())
            {
                var merchant = chr.getHiredMerchant();
                if (merchant == null || !merchant.isOwner(chr))
                {
                    return;
                }

                merchant.withdrawMesos(chr);
                merchant.clearInexistentItems();

                if (merchant.getItems().Count == 0)
                {
                    merchant.closeOwnerMerchant(chr);
                    return;
                }
                c.sendPacket(PacketCreator.updateHiredMerchant(merchant, chr));

            }
            else if (mode == Action.BUY.getCode() || mode == Action.MERCHANT_BUY.getCode())
            {
                if (isTradeOpen(chr))
                {
                    return;
                }

                int itemid = p.readByte();
                short quantity = p.readShort();
                if (quantity < 1)
                {
                    AutobanFactory.PACKET_EDIT.alert(chr, chr.getName() + " tried to packet edit with a hired merchant and or player shop.");
                    log.Warning("Chr {CharacterName} tried to buy item {ItemId} with quantity {ItemQuantity}", chr.getName(), itemid, quantity);
                    c.disconnect(true, false);
                    return;
                }
                var shop = chr.getPlayerShop();
                var merchant = chr.getHiredMerchant();
                if (shop != null && shop.isVisitor(chr))
                {
                    if (shop.buy(c, itemid, quantity))
                    {
                        shop.broadcast(PacketCreator.getPlayerShopItemUpdate(shop));
                    }
                }
                else if (merchant != null && !merchant.isOwner(chr))
                {
                    merchant.buy(c, itemid, quantity);
                    merchant.broadcastToVisitorsThreadsafe(PacketCreator.updateHiredMerchant(merchant, chr));
                }
            }
            else if (mode == Action.TAKE_ITEM_BACK.getCode())
            {
                if (isTradeOpen(chr))
                {
                    return;
                }

                var merchant = chr.getHiredMerchant();
                if (merchant != null && merchant.isOwner(chr))
                {
                    if (merchant.isOpen())
                    {
                        c.sendPacket(PacketCreator.serverNotice(1, "You can't take it with the store open."));
                        return;
                    }

                    int slot = p.readShort();
                    if (slot >= merchant.getItems().Count || slot < 0)
                    {
                        AutobanFactory.PACKET_EDIT.alert(chr, chr.getName() + " tried to packet edit with a hired merchant.");
                        log.Warning("Chr {CharacterName} tried to remove item at slot {Slot}", chr.getName(), slot);
                        c.disconnect(true, false);
                        return;
                    }

                    merchant.takeItemBack(slot, chr);
                }
            }
            else if (mode == Action.CLOSE_MERCHANT.getCode())
            {
                if (isTradeOpen(chr))
                {
                    return;
                }

                var merchant = chr.getHiredMerchant();
                if (merchant != null)
                {
                    merchant.closeOwnerMerchant(chr);
                }
            }
            else if (mode == Action.MAINTENANCE_OFF.getCode())
            {
                if (isTradeOpen(chr))
                {
                    return;
                }

                var merchant = chr.getHiredMerchant();
                if (merchant != null)
                {
                    if (merchant.isOwner(chr))
                    {
                        if (merchant.getItems().Count == 0)
                        {
                            merchant.closeOwnerMerchant(chr);
                        }
                        else
                        {
                            merchant.clearMessages();
                            merchant.setOpen(true);
                            merchant.getMap().broadcastMessage(PacketCreator.updateHiredMerchantBox(merchant));
                        }
                    }
                }

                chr.setHiredMerchant(null);
                c.sendPacket(PacketCreator.enableActions());
            }
            else if (mode == Action.BAN_PLAYER.getCode())
            {
                p.skip(1);

                var shop = chr.getPlayerShop();
                if (shop != null && shop.isOwner(chr))
                {
                    shop.banPlayer(p.readString());
                }
            }
            else if (mode == Action.EXPEL.getCode())
            {
                var miniGame = chr.getMiniGame();
                if (miniGame != null && miniGame.isOwner(chr))
                {
                    var visitor = miniGame.getVisitor();

                    if (visitor != null)
                    {
                        visitor.closeMiniGame(false);
                        visitor.sendPacket(PacketCreator.getMiniGameClose(true, 5));
                    }
                }
            }
            else if (mode == Action.EXIT_AFTER_GAME.getCode())
            {
                var miniGame = chr.getMiniGame();
                if (miniGame != null)
                {
                    miniGame.setQuitAfterGame(chr, true);
                }
            }
            else if (mode == Action.CANCEL_EXIT_AFTER_GAME.getCode())
            {
                var miniGame = chr.getMiniGame();
                if (miniGame != null)
                {
                    miniGame.setQuitAfterGame(chr, false);
                }
            }
        }
        finally
        {
            c.releaseClient();
        }
    }

    private bool isTradeOpen(IPlayer chr)
    {
        if (chr.getTrade() != null)
        {   // thanks to Rien dev team
            //Apparently there is a dupe exploit that causes racing conditions when saving/retrieving from the db with stuff like trade open.
            chr.sendPacket(PacketCreator.enableActions());
            return true;
        }

        return false;
    }

    private bool canPlaceStore(IPlayer chr)
    {
        try
        {
            foreach (IMapObject mmo in chr.getMap().getMapObjectsInRange(chr.getPosition(), 23000, Arrays.asList(MapObjectType.HIRED_MERCHANT, MapObjectType.PLAYER)))
            {
                if (mmo is IPlayer mc)
                {
                    if (mc.getId() == chr.getId())
                    {
                        continue;
                    }

                    var shop = mc.getPlayerShop();
                    if (shop != null && shop.isOwner(mc))
                    {
                        chr.sendPacket(PacketCreator.getMiniRoomError(13));
                        return false;
                    }
                }
                else
                {
                    chr.sendPacket(PacketCreator.getMiniRoomError(13));
                    return false;
                }
            }

            Point cpos = chr.getPosition();
            var portal = chr.getMap().findClosestTeleportPortal(cpos);
            if (portal != null && portal.getPosition().distance(cpos) < 120.0)
            {
                chr.sendPacket(PacketCreator.getMiniRoomError(10));
                return false;
            }
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }

        return true;
    }
}


public static class PlayActionExtensions
{
    public static int getCode(this PlayerInteractionHandler.Action action)
    {
        return (int)action;
    }
}