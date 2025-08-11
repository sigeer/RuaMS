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


using Application.Core.Channel.DataProviders;
using Application.Core.Channel.ServerData;
using Application.Core.Channel.Services;
using Application.Core.Game.Maps;
using Application.Core.Game.Trades;
using client.autoban;
using client.inventory;
using client.inventory.manipulator;
using constants.game;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Core.Channel.Net.Handlers;


/**
 * @author Matze
 * @author Ronan - concurrency safety and reviewed minigames
 */
public class PlayerInteractionHandler : ChannelHandlerBase
{
    readonly ILogger<PlayerInteractionHandler> _logger;
    readonly AutoBanDataManager _autoBanManager;
    readonly PlayerShopService _service;

    public PlayerInteractionHandler(ILogger<PlayerInteractionHandler> logger, AutoBanDataManager autoBanManager, PlayerShopService service)
    {
        _logger = logger;
        _autoBanManager = autoBanManager;
        _service = service;
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

    public override void HandlePacket(InPacket p, IChannelClient c)
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

            if (mode == PlayerInterAction.CREATE.getCode())
            {
                if (!chr.isAlive())
                {
                    // thanks GabrielSin for pointing this
                    chr.sendPacket(PacketCreator.getMiniRoomError(4));
                    return;
                }

                byte createType = p.readByte();
                if (createType == 3)
                {
                    // trade
                    TradeManager.StartTrade(chr);
                }
                else if (createType == 1 || createType == 2)
                {
                    // 1. omok mini game
                    // 2. matchcard
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

                    int type = p.ReadSByte();
                    if (createType == 1 && !chr.haveItem(ItemId.MINI_GAME_BASE + type))
                    {
                        chr.sendPacket(PacketCreator.getMiniRoomError(6));
                        return;
                    }

                    if (createType == 2 && !chr.haveItem(ItemId.MATCH_CARDS))
                    {
                        chr.sendPacket(PacketCreator.getMiniRoomError(6));
                        return;
                    }


                    MiniGame game = new MiniGame(chr, desc, pw, createType, type);
                    chr.setMiniGame(game);
                    chr.getMap().addMapObject(game);
                    chr.getMap().broadcastMessage(PacketCreator.AddMiniGameBox(chr, 1, 0));
                    game.SendGameInfo(c);
                }
                else if (createType == 4 || createType == 5)
                {
                    // shop
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

                    short unknown1 = p.readShort();
                    sbyte unknown2 = p.ReadSByte();

                    int itemId = p.readInt();
                    var item = chr.getInventory(InventoryType.CASH).findById(itemId);
                    if (item == null)
                    {
                        chr.sendPacket(PacketCreator.getMiniRoomError(6));
                        return;
                    }

                    IPlayerShop? iShop = null;
                    if (ItemConstants.isPlayerShop(itemId))
                    {
                        var shop = new PlayerShop(chr, desc, item);

                        chr.sendPacket(PacketCreator.getPlayerShop(shop, true));

                        iShop = shop;
                        //c.sendPacket(PacketCreator.getPlayerShopRemoveVisitor(1));
                    }
                    else if (ItemConstants.isHiredMerchant(itemId))
                    {
                        if (!_service.CanHiredMerchant(chr))
                        {
                            _autoBanManager.Autoban(AutobanFactory.ITEM_VAC, chr, "尝试绕过重复开店的检测");
                            return;
                        }

                        var merchant = new HiredMerchant(chr, desc, item);

                        chr.sendPacket(PacketCreator.getHiredMerchant(chr, merchant, true));

                        iShop = merchant;
                    }

                    chr.VisitingShop = iShop;
                }
            }
            else if (mode == PlayerInterAction.INVITE.getCode())
            {
                int otherCid = p.readInt();
                var other = chr.getMap().getCharacterById(otherCid);
                if (other == null || chr.getId() == other.getId())
                {
                    return;
                }

                TradeManager.InviteTrade(chr, other);
            }
            else if (mode == PlayerInterAction.DECLINE.getCode())
            {
                TradeManager.DeclineTrade(chr);
            }
            else if (mode == PlayerInterAction.VISIT.getCode())
            {
                var tradeObj = chr.getTrade();
                if (tradeObj != null && tradeObj.PartnerTrade != null)
                {
                    if (!tradeObj.isFullTrade() && !tradeObj.PartnerTrade.isFullTrade())
                    {
                        TradeManager.VisitTrade(chr, tradeObj.PartnerTrade.getChr());
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
                    if (ob is IPlayerShop shop)
                    {
                        shop.VisitShop(chr);
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
                }
            }
            else if (mode == PlayerInterAction.CHAT.getCode())
            {
                // chat lol          
                if (chr.getTrade() != null)
                {
                    chr.getTrade()!.chat(p.readString());
                }

                else if (chr.getMiniGame() != null)
                {
                    var game = chr.getMiniGame();
                    if (game != null)
                    {
                        game.chat(c, p.readString());
                    }
                }
                else if (chr.VisitingShop != null)
                {
                    chr.VisitingShop.sendMessage(chr, p.readString());
                }
            }
            else if (mode == PlayerInterAction.EXIT.getCode())
            {
                if (chr.getTrade() != null)
                {
                    TradeManager.CancelTrade(chr, TradeResult.PARTNER_CANCEL);
                }
                else
                {
                    chr.LeaveVisitingShop();
                    chr.closeMiniGame(false);
                }
            }
            else if (mode == PlayerInterAction.OPEN_STORE.getCode() || mode == PlayerInterAction.OPEN_CASH.getCode())
            {
                // 开店？
                if (isTradeOpen(chr))
                {
                    return;
                }

                if (mode == PlayerInterAction.OPEN_STORE.getCode())
                {
                    p.readByte();    //01
                }
                else
                {
                    p.readShort();
                    int birthday = p.readInt();
                    if (!c.CheckBirthday(birthday))
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

                if (chr.VisitingShop == null)
                    return;

                IPlayerShop manageShop = chr.VisitingShop;
                if (chr.VisitingShop.Type == PlayerShopType.PlayerShop && chr.VisitingShop.OwnerId == chr.Id)
                {
                    if (YamlConfig.config.server.USE_ERASE_PERMIT_ON_OPENSHOP)
                    {
                        InventoryManipulator.removeById(c, InventoryType.CASH, chr.VisitingShop.SourceItemId, 1, true, false);
                    }

                    var shop = (PlayerShop)chr.VisitingShop;

                    chr.getMap().addMapObject(shop);
                    chr.getMap().broadcastMessage(PacketCreator.updatePlayerShopBox(shop));

                    shop.SetOpen();
                }
                if (chr.VisitingShop.Type == PlayerShopType.HiredMerchant && chr.VisitingShop.OwnerId == chr.Id)
                {
                    var merchant = (HiredMerchant)chr.VisitingShop;

                    merchant.SetOpen();

                    chr.getMap().addMapObject(merchant);
                    chr.getMap().broadcastMessage(PacketCreator.spawnHiredMerchantBox(merchant));
                }
                c.getChannelServer().PlayerShopManager.NewPlayerShop(manageShop);
            }
            else if (mode == PlayerInterAction.READY.getCode())
            {
                var game = chr.getMiniGame();
                game?.broadcast(PacketCreator.getMiniGameReady(game));
            }
            else if (mode == PlayerInterAction.UN_READY.getCode())
            {
                var game = chr.getMiniGame();
                game?.broadcast(PacketCreator.getMiniGameUnReady(game));
            }
            else if (mode == PlayerInterAction.START.getCode())
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
            else if (mode == PlayerInterAction.GIVE_UP.getCode())
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
            else if (mode == PlayerInterAction.REQUEST_TIE.getCode())
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
            else if (mode == PlayerInterAction.ANSWER_TIE.getCode())
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
            else if (mode == PlayerInterAction.SKIP.getCode())
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
            else if (mode == PlayerInterAction.MOVE_OMOK.getCode())
            {
                int x = p.readInt(); // x point
                int y = p.readInt(); // y point
                int type = p.ReadSByte(); // piece ( 1 or 2; Owner has one piece, visitor has another, it switches every game.)
                chr.getMiniGame()!.setPiece(x, y, type, chr);
            }
            else if (mode == PlayerInterAction.SELECT_CARD.getCode())
            {
                int turn = p.ReadSByte(); // 1st turn = 1; 2nd turn = 0
                int slot = p.ReadSByte(); // slot
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
            else if (mode == PlayerInterAction.SET_MESO.getCode())
            {
                chr.getTrade()!.setMeso(p.readInt());
            }
            else if (mode == PlayerInterAction.SET_ITEMS.getCode())
            {
                ItemInformationProvider ii = ItemInformationProvider.getInstance();
                InventoryType ivType = InventoryTypeUtils.getByType(p.ReadSByte());
                short pos = p.readShort();
                var item = chr.getInventory(ivType).getItem(pos);
                short quantity = p.readShort();
                sbyte targetSlot = p.ReadSByte();

                if (targetSlot < 1 || targetSlot > 9)
                {
                    _logger.LogWarning("[Hack] Chr {CharacterName} Trying to dupe on trade slot.", chr.getName());
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
                        {
                            // ensure that undroppable items do not make it to the trade window
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
                                if (trade.PartnerTrade != null)
                                {
                                    trade.PartnerTrade.getChr().sendPacket(PacketCreator.getTradeItemAdd(1, tradeItem));
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogWarning(e, "Chr {CharacterName} tried to add {ItemName}x {ItemQuantity} in trade (slot {ItemSlot}), then exception occurred", chr, ii.getName(item.getItemId()), item.getQuantity(), targetSlot);
                        }
                        finally
                        {
                            inv.unlockInventory();
                        }
                    }
                }
            }
            else if (mode == PlayerInterAction.CONFIRM.getCode())
            {
                TradeManager.CompleteTrade(chr);
            }
            else if (mode == PlayerInterAction.ADD_ITEM.getCode() || mode == PlayerInterAction.PUT_ITEM.getCode())
            {
                if (isTradeOpen(chr))
                {
                    return;
                }

                InventoryType ivType = InventoryTypeUtils.getByType(p.ReadSByte());
                short slot = p.readShort();
                // 多少捆（客户端计算：总数/每捆多少个）perBundles * bundles = quantity
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

                // 每捆多少个
                short perBundle = p.readShort();

                if (ItemConstants.isRechargeable(ivItem.getItemId()))
                {
                    perBundle = 1;
                    bundles = 1;
                }
                else if (ivItem.getQuantity() < (bundles * perBundle))
                {
                    // thanks GabrielSin for finding a dupe here
                    c.sendPacket(PacketCreator.serverNotice(1, "Could not perform shop operation with that item."));
                    c.sendPacket(PacketCreator.enableActions());
                    return;
                }

                int price = p.readInt();
                if (perBundle <= 0 || perBundle * bundles > 2000 || bundles <= 0 || price <= 0 || price > int.MaxValue)
                {
                    _autoBanManager.Alert(AutobanFactory.PACKET_EDIT, c.OnlinedCharacter, chr.getName() + " tried to packet edit with hired merchants.");
                    _logger.LogWarning("Chr {CharacterName} might possibly have packet edited Hired Merchants. perBundle: {0}, perBundle * bundles (This multiplied cannot be greater than 2000): {1}, bundles: {2}, price: {Price}",
                            chr.getName(), perBundle, perBundle * bundles, bundles, price);
                    return;
                }

                c.getChannelServer().PlayerShopManager.AddCommodity(chr, ivType, ivItem, perBundle, bundles, price);
            }
            else if (mode == PlayerInterAction.REMOVE_ITEM.getCode() || mode == PlayerInterAction.TAKE_ITEM_BACK.getCode())
            {
                if (isTradeOpen(chr))
                {
                    return;
                }

                int itemIndex = p.readShort();
                c.getChannelServer().PlayerShopManager.RemoveCommodity(chr, itemIndex);
            }
            else if (mode == PlayerInterAction.MERCHANT_MESO.getCode())
            {
                var merchant = chr.VisitingShop as HiredMerchant;
                if (merchant == null)
                {
                    return;
                }

                merchant.withdrawMesos(chr);

            }
            else if (mode == PlayerInterAction.VIEW_VISITORS.getCode())
            {
                var merchant = chr.VisitingShop as HiredMerchant;
                if (merchant == null || !merchant.IsOwner(chr))
                {
                    return;
                }
                c.sendPacket(PacketCreator.viewMerchantVisitorHistory(merchant.getVisitorHistory()));
            }
            else if (mode == PlayerInterAction.VIEW_BLACKLIST.getCode())
            {
                var merchant = chr.VisitingShop as HiredMerchant;
                if (merchant == null || !merchant.IsOwner(chr))
                {
                    return;
                }

                c.sendPacket(PacketCreator.viewMerchantBlacklist(merchant.BlackList));
            }
            else if (mode == PlayerInterAction.ADD_TO_BLACKLIST.getCode())
            {
                var merchant = chr.VisitingShop as HiredMerchant;
                if (merchant == null || !merchant.IsOwner(chr))
                {
                    return;
                }
                string chrName = p.readString();
                merchant.addToBlacklist(chrName);
            }
            else if (mode == PlayerInterAction.REMOVE_FROM_BLACKLIST.getCode())
            {
                var merchant = chr.VisitingShop as HiredMerchant;
                if (merchant == null || !merchant.IsOwner(chr))
                {
                    return;
                }
                string chrName = p.readString();
                merchant.removeFromBlacklist(chrName);
            }
            else if (mode == PlayerInterAction.MERCHANT_ORGANIZE.getCode())
            {
                // 整理道具
                var merchant = chr.VisitingShop as HiredMerchant;
                if (merchant == null || !merchant.IsOwner(chr))
                {
                    return;
                }

                merchant.withdrawMesos(chr);
                merchant.clearInexistentItems();

                if (merchant.getItems().Count == 0)
                {
                    merchant.Close();
                    return;
                }
                c.sendPacket(PacketCreator.updateHiredMerchant(merchant, chr));

            }
            else if (mode == PlayerInterAction.BUY.getCode() || mode == PlayerInterAction.MERCHANT_BUY.getCode())
            {
                if (isTradeOpen(chr))
                {
                    return;
                }

                // 从代码来看这里并不是itemId, 而是index
                int itemIndex = p.ReadSByte();
                short quantity = p.readShort();
                if (quantity < 1)
                {
                    _autoBanManager.Alert(AutobanFactory.PACKET_EDIT, c.OnlinedCharacter, chr.getName() + " tried to packet edit with a hired merchant and or player shop.");
                    _logger.LogWarning("Chr {CharacterName} tried to buy item {ItemId} with quantity {ItemQuantity}", chr.getName(), itemIndex, quantity);
                    c.Disconnect(true, false);
                    return;
                }

                c.getChannelServer().PlayerShopManager.BuyItem(chr, itemIndex, quantity);
            }
            else if (mode == PlayerInterAction.CLOSE_MERCHANT.getCode())
            {
                if (isTradeOpen(chr))
                {
                    return;
                }

                c.getChannelServer().PlayerShopManager.CloseByPlayer(chr);

            }
            else if (mode == PlayerInterAction.MAINTENANCE_OFF.getCode())
            {
                if (isTradeOpen(chr))
                {
                    return;
                }

                var merchant = chr.VisitingShop as HiredMerchant;
                if (merchant != null)
                {
                    if (merchant.IsOwner(chr))
                    {
                        merchant.SetOpen();
                        merchant.getMap().broadcastMessage(PacketCreator.updateHiredMerchantBox(merchant));

                        c.getChannelServer().PlayerShopManager.SyncPlayerShop(merchant);
                    }
                }

                c.sendPacket(PacketCreator.enableActions());
            }
            else if (mode == PlayerInterAction.BAN_PLAYER.getCode())
            {
                var unknown1 = p.ReadSByte();

                var shop = chr.VisitingShop as PlayerShop;
                if (shop != null && shop.IsOwner(chr))
                {
                    shop.banPlayer(p.readString());
                }
            }
            else if (mode == PlayerInterAction.EXPEL.getCode())
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
            else if (mode == PlayerInterAction.EXIT_AFTER_GAME.getCode())
            {
                var miniGame = chr.getMiniGame();
                if (miniGame != null)
                {
                    miniGame.setQuitAfterGame(chr, true);
                }
            }
            else if (mode == PlayerInterAction.CANCEL_EXIT_AFTER_GAME.getCode())
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
        {
            // thanks to Rien dev team
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

                    var shop = mc.VisitingShop;
                    if (shop != null && shop.IsOwner(mc))
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
            _logger.LogError(e.ToString());
        }

        return true;
    }
}


public static class PlayActionExtensions
{
    public static int getCode(this PlayerInterAction action)
    {
        return (int)action;
    }
}