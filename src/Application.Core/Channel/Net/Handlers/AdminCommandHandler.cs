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
using Application.Core.Channel.Services;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using client.inventory;
using client.inventory.manipulator;
using Microsoft.Extensions.Logging;
using server.life;
using server.quest;
using System.Text;
using tools;

namespace Application.Core.Channel.Net.Handlers;

public class AdminCommandHandler : ChannelHandlerBase
{
    readonly ILogger<AdminCommandHandler> _logger;
    readonly AdminService _adminService;

    public AdminCommandHandler(ILogger<AdminCommandHandler> logger, AdminService adminService)
    {
        _logger = logger;
        _adminService = adminService;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        if (!c.OnlinedCharacter.isGM())
        {
            return;
        }
        byte mode = p.readByte();
        string victim;
        IPlayer? target;
        switch (mode)
        {
            case 0x00: // Level1~Level8 & Package1~Package2
                int[][] toSpawn = ItemInformationProvider.getInstance().getSummonMobs(p.readInt());
                foreach (int[] toSpawnChild in toSpawn)
                {
                    if (Randomizer.nextInt(100) < toSpawnChild[1])
                    {
                        c.OnlinedCharacter.getMap().spawnMonsterOnGroundBelow(LifeFactory.getMonster(toSpawnChild[0]), c.OnlinedCharacter.getPosition());
                    }
                }
                c.sendPacket(PacketCreator.enableActions());
                break;
            case 0x01:
                { // /d (inv)
                    sbyte inventoryType = p.ReadSByte();
                    Inventory inValue = c.OnlinedCharacter.getInventory(InventoryTypeUtils.getByType(inventoryType));
                    for (short i = 1; i <= inValue.getSlotLimit(); i++)
                    {
                        //TODO What is the point of this loop?
                        var item = inValue.getItem(i);
                        if (item != null)
                        {
                            InventoryManipulator.removeFromSlot(c, InventoryTypeUtils.getByType(inventoryType), i, item.getQuantity(), false);
                        }
                        return;
                    }
                    break;
                }
            case 0x02: // Exp
                c.OnlinedCharacter.setExp(p.readInt());
                break;
            case 0x03: // /ban <name>
                c.OnlinedCharacter.yellowMessage("Please use !ban <IGN> <Reason>");
                break;
            case 0x04: // /block <name> <duration (in days)> <HACK/BOT/AD/HARASS/CURSE/SCAM/MISCONDUCT/SELL/ICASH/TEMP/GM/IPROGRAM/MEGAPHONE>
                victim = p.readString();
                int type = p.readByte(); //reason
                int duration = p.readInt();
                string description = p.readString();
                _adminService.Ban(c.OnlinedCharacter, victim, type, description, duration);
                break;
            case 0x10: // /h, information added by vana -- <and tele mode f1> ... hide ofcourse
                c.OnlinedCharacter.Hide(p.readByte() == 1);
                break;
            case 0x11: // Entering a map
                switch (p.readByte())
                {
                    case 0:// /u
                        StringBuilder sb = new StringBuilder("USERS ON THIS MAP: ");
                        foreach (var mc in c.OnlinedCharacter.getMap().getCharacters())
                        {
                            sb.Append(mc.getName());
                            sb.Append(" ");
                        }
                        c.OnlinedCharacter.message(sb.ToString());
                        break;
                    case 12:// /uclip and entering a map
                        break;
                }
                break;
            case 0x12: // Send
                victim = p.readString();
                int mapId = p.readInt();
                c.getChannelServer().getPlayerStorage().getCharacterByName(victim)?.changeMap(c.getChannelServer().getMapFactory().getMap(mapId));
                break;
            case 0x15: // Kill
                int mobToKill = p.readInt();
                int amount = p.readInt();
                List<IMapObject> monsterx = c.OnlinedCharacter.getMap().getMapObjectsInRange(c.OnlinedCharacter.getPosition(), double.PositiveInfinity, Arrays.asList(MapObjectType.MONSTER));
                for (int x = 0; x < amount; x++)
                {
                    var monster = (Monster)monsterx[x];
                    if (monster.getId() == mobToKill)
                    {
                        c.OnlinedCharacter.getMap().killMonster(monster, c.OnlinedCharacter, true);
                    }
                }
                break;
            case 0x16: // Questreset
                Quest.getInstance(p.readShort()).reset(c.OnlinedCharacter);
                break;
            case 0x17: // Summon
                int mobId = p.readInt();
                int quantity = p.readInt();
                for (int i = 0; i < quantity; i++)
                {
                    c.OnlinedCharacter.getMap().spawnMonsterOnGroundBelow(LifeFactory.getMonster(mobId), c.OnlinedCharacter.getPosition());
                }
                break;
            case 0x18: // Maple & Mobhp
                int mobHp = p.readInt();
                c.OnlinedCharacter.dropMessage("Monsters HP");
                List<IMapObject> monsters = c.OnlinedCharacter.getMap().getMapObjectsInRange(c.OnlinedCharacter.getPosition(), double.PositiveInfinity, Arrays.asList(MapObjectType.MONSTER));
                foreach (var mobs in monsters)
                {
                    Monster monster = (Monster)mobs;
                    if (monster.getId() == mobHp)
                    {
                        c.OnlinedCharacter.dropMessage(monster.getName() + ": " + monster.getHp());
                    }
                }
                break;
            case 0x1E: // Warn
                victim = p.readString();
                string message = p.readString();
                target = c.getChannelServer().getPlayerStorage().getCharacterByName(victim);
                if (target != null)
                {
                    target.getClient().sendPacket(PacketCreator.serverNotice(1, message));
                    c.sendPacket(PacketCreator.getGMEffect(0x1E, 1));
                }
                else
                {
                    c.sendPacket(PacketCreator.getGMEffect(0x1E, 0));
                }
                break;
            case 0x24:// /Artifact Ranking
                break;
            case 0x77: //Testing purpose
                if (p.available() == 4)
                {
                    _logger.LogDebug("int: {0}", p.readInt());
                }
                else if (p.available() == 2)
                {
                    _logger.LogDebug("short: {0}", p.readShort());
                }
                break;
            default:
                _logger.LogInformation("New GM packet encountered (MODE: {Mode}): {0}", mode, p);
                break;
        }
    }
}
