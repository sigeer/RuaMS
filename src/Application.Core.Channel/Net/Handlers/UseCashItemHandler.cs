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
using Application.Core.Game.Players;
using Application.Utility.Configs;
using Application.Utility.Exceptions;
using Application.Utility.Extensions;
using client.creator.veteran;
using client.inventory;
using client.inventory.manipulator;
using client.processor.npc;
using client.processor.stat;
using constants.game;
using constants.id;
using constants.inventory;
using Microsoft.Extensions.Logging;
using net.packet;
using net.packet.outs;
using server;
using server.maps;
using service;
using System.Text;
using tools;
using static client.inventory.Equip;

namespace Application.Core.Channel.Net.Handlers;

public class UseCashItemHandler : ChannelHandlerBase
{

    private NoteService noteService;
    readonly ILogger<UseCashItemHandler> _logger;

    public UseCashItemHandler(NoteService noteService, ILogger<UseCashItemHandler> logger)
    {
        this.noteService = noteService;
        _logger = logger;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var player = c.OnlinedCharacter;

        long timeNow = c.CurrentServer.getCurrentTime();
        if (timeNow - player.getLastUsedCashItem() < 3000)
        {
            player.dropMessage(1, "You have used a cash item recently. Wait a moment, then try again.");
            c.sendPacket(PacketCreator.enableActions());
            return;
        }
        player.setLastUsedCashItem(timeNow);

        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        short position = p.readShort();
        int itemId = p.readInt();
        int itemType = itemId / 10000;

        Inventory cashInv = player.getInventory(InventoryType.CASH);
        var toUse = cashInv.getItem(position);
        if (toUse == null || toUse.getItemId() != itemId)
        {
            toUse = cashInv.findById(itemId);

            if (toUse == null)
            {
                c.sendPacket(PacketCreator.enableActions());
                return;
            }

            position = toUse.getPosition();
        }

        if (toUse.getQuantity() < 1)
        {
            c.sendPacket(PacketCreator.enableActions());
            return;
        }

        string medal = "";
        var medalItem = player.getInventory(InventoryType.EQUIPPED).getItem(-49);
        if (medalItem != null)
        {
            medal = "<" + ii.getName(medalItem.getItemId()) + "> ";
        }

        if (itemType == 504)
        { // vip teleport rock
            string error1 = "Either the player could not be found or you were trying to teleport to an illegal location.";
            bool vip = p.readByte() == 1 && itemId / 1000 >= 5041;
            remove(c, position, itemId);
            bool success = false;
            if (!vip)
            {
                int mapId = p.readInt();
                if (itemId / 1000 >= 5041 || mapId / 100000000 == player.getMapId() / 100000000)
                {
                    //check vip or same continent
                    var targetMap = c.CurrentServer.getMapFactory().getMap(mapId);
                    if (!FieldLimit.CANNOTVIPROCK.check(targetMap.getFieldLimit()) && (targetMap.getForcedReturnId() == MapId.NONE || MapId.isMapleIsland(mapId)))
                    {
                        player.forceChangeMap(targetMap, targetMap.getRandomPlayerSpawnpoint());
                        success = true;
                    }
                    else
                    {
                        player.dropMessage(1, error1);
                    }
                }
                else
                {
                    player.dropMessage(1, "You cannot teleport between continents with this teleport rock.");
                }
            }
            else
            {
                string name = p.readString();
                var victim = c.CurrentServer.getPlayerStorage().getCharacterByName(name);

                if (victim != null)
                {
                    var targetMap = victim.getMap();
                    if (!FieldLimit.CANNOTVIPROCK.check(targetMap.getFieldLimit()) && (targetMap.getForcedReturnId() == MapId.NONE || MapId.isMapleIsland(targetMap.getId())))
                    {
                        if (!victim.isGM() || victim.gmLevel() <= player.gmLevel())
                        {   // thanks Yoboes for noticing non-GM's being unreachable through rocks
                            player.forceChangeMap(targetMap, targetMap.findClosestPlayerSpawnpoint(victim.getPosition()));
                            success = true;
                        }
                        else
                        {
                            player.dropMessage(1, error1);
                        }
                    }
                    else
                    {
                        player.dropMessage(1, "You cannot teleport to this map.");
                    }
                }
                else
                {
                    player.dropMessage(1, "Player could not be found in this channel.");
                }
            }

            if (!success)
            {
                InventoryManipulator.addById(c, itemId, 1);
                c.sendPacket(PacketCreator.enableActions());
            }
        }
        else if (itemType == 505)
        {
            // AP/SP reset
            if (!player.isAlive())
            {
                c.sendPacket(PacketCreator.enableActions());
                return;
            }

            if (itemId > ItemId.AP_RESET)
            {
                int SPTo = p.readInt();
                int SPFrom = p.readInt();

                AssignSPProcessor.ResetSkill(player, SPTo, SPFrom);
            }
            else
            {
                int APTo = p.readInt();
                int APFrom = p.readInt();

                if (!AssignAPProcessor.APResetAction(c, APFrom, APTo))
                {
                    return;
                }
            }
            remove(c, position, itemId);
        }
        else if (itemType == 506)
        {
            Item? eq = null;
            if (itemId == 5060000)
            { // Item tag.
                int equipSlot = p.readShort();
                if (equipSlot == 0)
                {
                    return;
                }
                eq = player.getInventory(InventoryType.EQUIPPED).getItem((short)equipSlot);
                eq?.setOwner(player.getName());
            }
            else if (itemId == 5060001 || itemId == 5061000 || itemId == 5061001 || itemId == 5061002 || itemId == 5061003)
            { // Sealing lock
                InventoryType type = InventoryTypeUtils.getByType((sbyte)p.readInt());
                eq = player.getInventory(type).getItem((short)p.readInt());
                if (eq == null)
                { //Check if the type is EQUIPMENT?
                    return;
                }
                short flag = eq.getFlag();
                if (eq.getExpiration() > -1 && (eq.getFlag() & ItemConstants.LOCK) != ItemConstants.LOCK)
                {
                    return; //No perma items pls
                }
                flag |= ItemConstants.LOCK;
                eq.setFlag(flag);

                long period = 0;
                if (itemId == 5061000)
                {
                    period = 7;
                }
                else if (itemId == 5061001)
                {
                    period = 30;
                }
                else if (itemId == 5061002)
                {
                    period = 90;
                }
                else if (itemId == 5061003)
                {
                    period = 365;
                }

                if (period > 0)
                {
                    long expiration = eq.getExpiration() > -1 ? eq.getExpiration() : c.CurrentServer.getCurrentTime();
                    eq.setExpiration(expiration + 24 * 3600 * 1000 * (period));
                }

                // double-remove found thanks to BHB
            }
            else if (itemId == 5060002)
            { // Incubator
                sbyte inventory2 = (sbyte)p.readInt();
                short slot2 = (short)p.readInt();
                var item2 = player.getInventory(InventoryTypeUtils.getByType(inventory2)).getItem(slot2);
                if (item2 == null) // hacking
                {
                    return;
                }
                if (getIncubatedItem(c, itemId))
                {
                    InventoryManipulator.removeFromSlot(c, InventoryTypeUtils.getByType(inventory2), slot2, 1, false);
                    remove(c, position, itemId);
                }
                return;
            }
            p.readInt(); // time stamp
            if (eq != null)
            {
                player.forceUpdateItem(eq);
                remove(c, position, itemId);
            }
        }
        else if (itemType == 507)
        {
            bool whisper;
            switch ((itemId / 1000) % 10)
            {
                case 1: // Megaphone
                    if (player.getLevel() > 9)
                    {
                        player.getClient().getChannelServer().broadcastPacket(PacketCreator.serverNotice(2, medal + player.getName() + " : " + p.readString()));
                    }
                    else
                    {
                        player.dropMessage(1, "You may not use this until you're level 10.");
                        return;
                    }
                    break;
                case 2: // Super megaphone
                    c.CurrentServer.BroadcastWorldPacket(PacketCreator.serverNotice(3, c.ActualChannel, medal + player.getName() + " : " + p.readString(), (p.readByte() != 0)));
                    break;
                case 5: // Maple TV
                    int tvType = itemId % 10;
                    bool megassenger = false;
                    bool ear = false;
                    IPlayer? victim = null;
                    if (tvType != 1)
                    {
                        if (tvType >= 3)
                        {
                            megassenger = true;
                            if (tvType == 3)
                            {
                                p.readByte();
                            }
                            ear = 1 == p.readByte();
                        }
                        else if (tvType != 2)
                        {
                            p.readByte();
                        }
                        if (tvType != 4)
                        {
                            victim = c.CurrentServer.getPlayerStorage().getCharacterByName(p.readString());
                        }
                    }
                    List<string> messages = new();
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < 5; i++)
                    {
                        string message = p.readString();
                        if (megassenger)
                        {
                            builder.Append(" ").Append(message);
                        }
                        messages.Add(message);
                    }
                    p.readInt();

                    if (!MapleTVEffect.broadcastMapleTVIfNotActive(player, victim, messages, tvType))
                    {
                        player.dropMessage(1, "MapleTV is already in use.");
                        return;
                    }

                    if (megassenger)
                    {
                        c.CurrentServer.BroadcastWorldPacket(PacketCreator.serverNotice(3, c.ActualChannel, medal + player.getName() + " : " + builder, ear));
                    }

                    break;
                case 6: //item megaphone
                    string msg = medal + player.getName() + " : " + p.readString();
                    whisper = p.readByte() == 1;
                    Item? item = null;
                    if (p.readByte() == 1)
                    { //item
                        item = player.getInventory(InventoryTypeUtils.getByType((sbyte)p.readInt())).getItem((short)p.readInt());
                        if (item == null) //hack
                        {
                            return;
                        }

                        // thanks Conrad for noticing that untradeable items should be allowed in megas
                    }
                    c.CurrentServer.BroadcastWorldPacket(PacketCreator.itemMegaphone(msg, whisper, c.ActualChannel, item));
                    break;
                case 7: //triple megaphone
                    int lines = p.ReadSByte();
                    if (lines < 1 || lines > 3) //hack
                    {
                        return;
                    }
                    string[] msg2 = new string[lines];
                    for (int i = 0; i < lines; i++)
                    {
                        msg2[i] = medal + player.getName() + " : " + p.readString();
                    }
                    whisper = p.readByte() == 1;
                    c.CurrentServer.BroadcastWorldPacket(PacketCreator.getMultiMegaphone(msg2, c.ActualChannel, whisper));
                    break;
            }
            remove(c, position, itemId);
        }
        else if (itemType == 508)
        {   // thanks tmskdl12 for graduation banner; thanks ratency for first pointing lack of Kite handling
            Kite kite = new Kite(player, p.readString(), itemId);

            if (!GameConstants.isFreeMarketRoom(player.getMapId()))
            {
                player.getMap().spawnKite(kite);
                remove(c, position, itemId);
            }
            else
            {
                c.sendPacket(PacketCreator.sendCannotSpawnKite());
            }
        }
        else if (itemType == 509)
        {
            string sendTo = p.readString();
            string msg = p.readString();
            bool sendSuccess = noteService.sendNormal(msg, player.getName(), sendTo);
            if (sendSuccess)
            {
                remove(c, position, itemId);
                c.sendPacket(new SendNoteSuccessPacket());
            }
        }
        else if (itemType == 510)
        {
            player.getMap().broadcastMessage(PacketCreator.musicChange("Jukebox/Congratulation"));
            remove(c, position, itemId);
        }
        else if (itemType == 512)
        {
            if (ii.getStateChangeItem(itemId) != 0)
            {
                foreach (var mChar in player.getMap().getCharacters())
                {
                    ii.getItemEffect(ii.getStateChangeItem(itemId))?.applyTo(mChar);
                }
            }
            player.getMap().startMapEffect(ii.getMsg(itemId).replaceFirst("%s", player.getName()).replaceFirst("%s", p.readString()), itemId);
            remove(c, position, itemId);
        }
        else if (itemType == 517)
        {
            var pet = player.getPet(0);
            if (pet == null)
            {
                c.sendPacket(PacketCreator.enableActions());
                return;
            }
            string newName = p.readString();
            pet.setName(newName);
            pet.saveToDb();

            var item = player.getInventory(InventoryType.CASH).getItem(pet.getPosition());
            if (item != null)
            {
                player.forceUpdateItem(item);
            }

            player.getMap().broadcastMessage(player, PacketCreator.changePetName(player, newName, 1), true);
            c.sendPacket(PacketCreator.enableActions());
            remove(c, position, itemId);
        }
        else if (itemType == 520)
        {
            player.gainMeso(ii.getMeso(itemId), true, false, true);
            remove(c, position, itemId);
            c.sendPacket(PacketCreator.enableActions());
        }
        else if (itemType == 523)
        {
            int itemid = p.readInt();

            if (!YamlConfig.config.server.USE_ENFORCE_ITEM_SUGGESTION)
            {
                c.getWorldServer().addOwlItemSearch(itemid);
            }
            player.setOwlSearch(itemid);
            var hmsAvailable = c.getWorldServer().getAvailableItemBundles(itemid);
            if (hmsAvailable.Count > 0)
            {
                remove(c, position, itemId);
            }

            c.sendPacket(PacketCreator.owlOfMinerva(c, itemid, hmsAvailable));
            c.sendPacket(PacketCreator.enableActions());

        }
        else if (itemType == 524)
        {
            for (byte i = 0; i < 3; i++)
            {
                var pet = player.getPet(i);
                if (pet != null)
                {
                    var pair = pet.canConsume(itemId);

                    if (pair.CanConsume)
                    {
                        pet.gainTamenessFullness(player, pair.PetId, 100, 1, true);
                        remove(c, position, itemId);
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            c.sendPacket(PacketCreator.enableActions());
        }
        else if (itemType == 530)
        {
            ii.getItemEffect(itemId)?.applyTo(player);
            remove(c, position, itemId);
        }
        else if (itemType == 533)
        {
            DueyProcessor.dueySendTalk(c, true);
        }
        else if (itemType == 537)
        {
            if (GameConstants.isFreeMarketRoom(player.getMapId()))
            {
                player.dropMessage(5, "You cannot use the chalkboard here.");
                player.sendPacket(PacketCreator.enableActions());
                return;
            }

            player.setChalkboard(p.readString());
            player.getMap().broadcastMessage(PacketCreator.useChalkboard(player, false));
            player.sendPacket(PacketCreator.enableActions());
            //remove(c, position, itemId);  thanks Conrad for noticing chalkboards shouldn't be depleted upon use
        }
        else if (itemType == 539)
        {
            List<string> strLines = new();
            for (int i = 0; i < 4; i++)
            {
                strLines.Add(p.readString());
            }

            c.CurrentServer.BroadcastWorldPacket(PacketCreator.getAvatarMega(player, medal, c.ActualChannel, itemId, strLines, (p.readByte() != 0)));
            TimerManager.getInstance().schedule(() => c.CurrentServer.BroadcastWorldPacket(PacketCreator.byeAvatarMega()), TimeSpan.FromSeconds(10));
            remove(c, position, itemId);
        }
        else if (itemType == 540)
        {
            p.readByte();
            p.readInt();
            if (itemId == ItemId.NAME_CHANGE)
            {
                c.sendPacket(PacketCreator.showNameChangeCancel(player.cancelPendingNameChange()));
            }
            else if (itemId == ItemId.WORLD_TRANSFER)
            {
                throw new BusinessNotsupportException("World Transfer");
            }
            remove(c, position, itemId);
            c.sendPacket(PacketCreator.enableActions());
        }
        else if (itemType == 543)
        {
            if (itemId == ItemId.MAPLE_LIFE_B && !c.GainCharacterSlot())
            {
                player.dropMessage(1, "You have already used up all 12 extra character slots.");
                c.sendPacket(PacketCreator.enableActions());
                return;
            }

            string name = p.readString();
            int face = p.readInt();
            int hair = p.readInt();
            int haircolor = p.readInt();
            int skin = p.readInt();
            int gender = p.readInt();
            int jobid = p.readInt();
            int improveSp = p.readInt();

            int createStatus = (jobid) switch
            {
                0 => WarriorCreator.createCharacter(c, name, face, hair + haircolor, skin, gender, improveSp),
                1 => MagicianCreator.createCharacter(c, name, face, hair + haircolor, skin, gender, improveSp),
                2 => BowmanCreator.createCharacter(c, name, face, hair + haircolor, skin, gender, improveSp),
                3 => ThiefCreator.createCharacter(c, name, face, hair + haircolor, skin, gender, improveSp),
                _ => PirateCreator.createCharacter(c, name, face, hair + haircolor, skin, gender, improveSp),
            };

            if (createStatus == 0)
            {
                c.sendPacket(PacketCreator.sendMapleLifeError(0));   // success!

                player.showHint("#bSuccess#k on creation of the new character through the Maple Life card.");
                remove(c, position, itemId);
            }
            else
            {
                if (createStatus == -1)
                {    // check name
                    c.sendPacket(PacketCreator.sendMapleLifeNameError());
                }
                else
                {
                    c.sendPacket(PacketCreator.sendMapleLifeError(-1 * createStatus));
                }
            }
        }
        else if (itemType == 545)
        {
            // MiuMiu's travel store
            if (player.getShop() == null)
            {
                var shop = ShopFactory.getInstance().getShop(1338);
                if (shop != null)
                {
                    shop.sendShop(c);
                    remove(c, position, itemId);
                }
            }
            else
            {
                c.sendPacket(PacketCreator.enableActions());
            }
        }
        else if (itemType == 550)
        { //Extend item expiration
            c.sendPacket(PacketCreator.enableActions());
        }
        else if (itemType == 552)
        {
            InventoryType type = InventoryTypeUtils.getByType((sbyte)p.readInt());
            short slot = (short)p.readInt();
            var item = player.getInventory(type).getItem(slot);
            if (item == null || item.getQuantity() <= 0 || KarmaManipulator.hasKarmaFlag(item) || !ii.isKarmaAble(item.getItemId()))
            {
                c.sendPacket(PacketCreator.enableActions());
                return;
            }

            KarmaManipulator.setKarmaFlag(item);
            player.forceUpdateItem(item);
            remove(c, position, itemId);
            c.sendPacket(PacketCreator.enableActions());
        }
        else if (itemType == 552)
        { //DS EGG THING
            c.sendPacket(PacketCreator.enableActions());
        }
        else if (itemType == 557)
        {
            p.readInt();
            int itemSlot = p.readInt();
            p.readInt();
            var equip = (Equip)player.getInventory(InventoryType.EQUIP).getItem((short)itemSlot);
            if (equip.getVicious() >= 2 || player.getInventory(InventoryType.CASH).findById(ItemId.VICIOUS_HAMMER) == null)
            {
                return;
            }
            equip.setVicious(equip.getVicious() + 1);
            equip.setUpgradeSlots(equip.getUpgradeSlots() + 1);
            remove(c, position, itemId);
            c.sendPacket(PacketCreator.enableActions());
            c.sendPacket(PacketCreator.sendHammerData(equip.getVicious()));
            player.forceUpdateItem(equip);
        }
        else if (itemType == 561)
        { //VEGA'S SPELL
            if (p.readInt() != 1)
            {
                return;
            }

            byte eSlot = (byte)p.readInt();
            var eitem = player.getInventory(InventoryType.EQUIP).getItem(eSlot);

            if (p.readInt() != 2)
            {
                return;
            }

            byte uSlot = (byte)p.readInt();
            var uitem = player.getInventory(InventoryType.USE).getItem(uSlot);
            if (eitem == null || uitem == null)
            {
                return;
            }

            Equip toScroll = (Equip)eitem;
            if (toScroll.getUpgradeSlots() < 1)
            {
                c.sendPacket(PacketCreator.getInventoryFull());
                return;
            }

            //should have a check here against PE hacks
            if (itemId / 1000000 != 5)
            {
                itemId = 0;
            }

            player.toggleBlockCashShop();

            int curlevel = toScroll.getLevel();
            c.sendPacket(PacketCreator.sendVegaScroll(0x40));

            Equip scrolled = (Equip)ii.scrollEquipWithId(toScroll, uitem.getItemId(), false, itemId, player.isGM())!;
            c.sendPacket(PacketCreator.sendVegaScroll(scrolled.getLevel() > curlevel ? 0x41 : 0x43));
            //opcodes 0x42, 0x44: "this item cannot be used"; 0x39, 0x45: crashes

            InventoryManipulator.removeFromSlot(c, InventoryType.USE, uSlot, 1, false);
            remove(c, position, itemId);

            IChannelClient client = c;
            TimerManager.getInstance().schedule(() =>
            {
                if (!player.isLoggedin())
                {
                    return;
                }

                player.toggleBlockCashShop();

                List<ModifyInventory> mods = new();
                mods.Add(new ModifyInventory(3, scrolled));
                mods.Add(new ModifyInventory(0, scrolled));
                client.sendPacket(PacketCreator.modifyInventory(true, mods));

                var scrollResult = scrolled.getLevel() > curlevel ? ScrollResult.SUCCESS : ScrollResult.FAIL;
                player.getMap().broadcastMessage(PacketCreator.getScrollEffect(player.getId(), scrollResult, false, false));
                if (eSlot < 0 && (scrollResult == ScrollResult.SUCCESS))
                {
                    player.equipChanged();
                }

                client.sendPacket(PacketCreator.enableActions());
            }, TimeSpan.FromSeconds(3));
        }
        else
        {
            _logger.LogWarning("NEW CASH ITEM TYPE: {ItemType}, packet: {Packet}", itemType, p);
            c.sendPacket(PacketCreator.enableActions());
        }
    }

    private static void remove(IChannelClient c, short position, int itemid)
    {
        Inventory cashInv = c.OnlinedCharacter.getInventory(InventoryType.CASH);
        cashInv.lockInventory();
        try
        {
            var it = cashInv.getItem(position);
            if (it == null || it.getItemId() != itemid)
            {
                it = cashInv.findById(itemid);
                if (it != null)
                {
                    position = it.getPosition();
                }
            }

            InventoryManipulator.removeFromSlot(c, InventoryType.CASH, position, 1, true, false);
        }
        finally
        {
            cashInv.unlockInventory();
        }
    }

    private static bool getIncubatedItem(IChannelClient c, int id)
    {
        int[] ids = { 1012070, 1302049, 1302063, 1322027, 2000004, 2000005, 2020013, 2020015, 2040307, 2040509, 2040519, 2040521, 2040533, 2040715, 2040717, 2040810, 2040811, 2070005, 2070006, 4020009, };
        int[] quantitys = { 1, 1, 1, 1, 240, 200, 200, 200, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 3 };
        int amount = 0;
        for (int i = 0; i < ids.Length; i++)
        {
            if (i == id)
            {
                amount = quantitys[i];
            }
        }
        if (c.OnlinedCharacter.getInventory(InventoryTypeUtils.getByType((sbyte)(id / 1000000))).isFull())
        {
            return false;
        }
        InventoryManipulator.addById(c, id, (short)amount);
        return true;
    }
}
