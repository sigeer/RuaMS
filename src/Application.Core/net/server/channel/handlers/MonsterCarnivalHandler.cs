/*
 This file is part of the OdinMS Maple Story Server
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


using client;
using net.packet;
using net.server.world;
using server.life;
using server.partyquest;
using tools;

namespace net.server.channel.handlers;

/**
 * @author Drago (Dragohe4rt)
 */

public class MonsterCarnivalHandler : AbstractPacketHandler
{

    public override void handlePacket(InPacket p, Client c)
    {
        if (c.tryacquireClient())
        {
            try
            {
                try
                {
                    int tab = p.readByte();
                    int num = p.readByte();
                    int neededCP = 0;
                    if (tab == 0)
                    {
                        var mobs = c.getPlayer().getMap().getMobsToSpawn();
                        if (num >= mobs.Count || c.getPlayer().getCP() < mobs.get(num).Value)
                        {
                            c.sendPacket(PacketCreator.CPQMessage(1));
                            c.sendPacket(PacketCreator.enableActions());
                            return;
                        }

                        Monster mob = LifeFactory.getMonster(mobs.get(num).Key);
                        var mcpq = c.getPlayer().getMonsterCarnival();
                        if (mcpq != null)
                        {
                            if (!mcpq.canSummonR() && c.getPlayer().getTeam() == 0 || !mcpq.canSummonB() && c.getPlayer().getTeam() == 1)
                            {
                                c.sendPacket(PacketCreator.CPQMessage(2));
                                c.sendPacket(PacketCreator.enableActions());
                                return;
                            }

                            if (c.getPlayer().getTeam() == 0)
                            {
                                mcpq.summonR();
                            }
                            else
                            {
                                mcpq.summonB();
                            }

                            var spawnPos = c.getPlayer().getMap().getRandomSP(c.getPlayer().getTeam());
                            mob.setPosition(spawnPos.Value);

                            c.getPlayer().getMap().addMonsterSpawn(mob, 1, c.getPlayer().getTeam());
                            c.getPlayer().getMap().addAllMonsterSpawn(mob, 1, c.getPlayer().getTeam());
                            c.sendPacket(PacketCreator.enableActions());
                        }

                        neededCP = mobs.get(num).Value;
                    }
                    else if (tab == 1)
                    { //debuffs
                        List<int> skillid = c.getPlayer().getMap().getSkillIds();
                        if (num >= skillid.Count)
                        {
                            c.getPlayer().dropMessage(5, "An unexpected error has occurred.");
                            c.sendPacket(PacketCreator.enableActions());
                            return;
                        }
                        var skill = CarnivalFactory.getInstance().getSkill(skillid.get(num)); //ugh wtf
                        if (skill == null || c.getPlayer().getCP() < skill.cpLoss)
                        {
                            c.sendPacket(PacketCreator.CPQMessage(1));
                            c.sendPacket(PacketCreator.enableActions());
                            return;
                        }
                        var dis = skill.getDisease();
                        Party enemies = c.getPlayer().getParty().getEnemy();
                        if (skill.targetsAll)
                        {
                            int hitChance = rollHitChance(dis.getMobSkillType().Value);
                            if (hitChance <= 80)
                            {
                                foreach (PartyCharacter mpc in enemies.getPartyMembers())
                                {
                                    Character mc = mpc.getPlayer();
                                    if (mc != null)
                                    {
                                        if (dis == null)
                                        {
                                            mc.dispel();
                                        }
                                        else
                                        {
                                            mc.giveDebuff(dis, skill.getSkill());
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            int amount = enemies.getMembers().Count - 1;
                            int randd = (int)Math.Floor(Randomizer.nextDouble() * amount);
                            var chrApp = c.getPlayer().getMap().getCharacterById(enemies.getMemberByPos(randd).getId());
                            if (chrApp != null && chrApp.getMap().isCPQMap())
                            {
                                if (dis == null)
                                {
                                    chrApp.dispel();
                                }
                                else
                                {
                                    chrApp.giveDebuff(dis, skill.getSkill());
                                }
                            }
                        }
                        neededCP = skill.cpLoss;
                        c.sendPacket(PacketCreator.enableActions());
                    }
                    else if (tab == 2)
                    { //protectors
                        var skill = CarnivalFactory.getInstance().getGuardian(num);
                        if (skill == null || c.getPlayer().getCP() < skill.cpLoss)
                        {
                            c.sendPacket(PacketCreator.CPQMessage(1));
                            c.sendPacket(PacketCreator.enableActions());
                            return;
                        }

                        var mcpq = c.getPlayer().getMonsterCarnival();
                        if (mcpq != null)
                        {
                            if (!mcpq.canGuardianR() && c.getPlayer().getTeam() == 0 || !mcpq.canGuardianB() && c.getPlayer().getTeam() == 1)
                            {
                                c.sendPacket(PacketCreator.CPQMessage(2));
                                c.sendPacket(PacketCreator.enableActions());
                                return;
                            }

                            int success = c.getPlayer().getMap().spawnGuardian(c.getPlayer().getTeam(), num);
                            if (success != 1)
                            {
                                switch (success)
                                {
                                    case -1:
                                        c.sendPacket(PacketCreator.CPQMessage(3));
                                        break;

                                    case 0:
                                        c.sendPacket(PacketCreator.CPQMessage(4));
                                        break;

                                    default:
                                        c.sendPacket(PacketCreator.CPQMessage(3));
                                        break;
                                }
                                c.sendPacket(PacketCreator.enableActions());
                                return;
                            }
                            else
                            {
                                neededCP = skill.cpLoss;
                            }
                        }
                    }
                    c.getPlayer().gainCP(-neededCP);
                    c.getPlayer().getMap().broadcastMessage(PacketCreator.playerSummoned(c.getPlayer().getName(), tab, num));
                }
                catch (Exception e)
                {
                    log.Error(e.ToString());
                }
            }
            finally
            {
                c.releaseClient();
            }
        }
    }

    private int rollHitChance(MobSkillType type)
    {
        return (type) switch
        {
            MobSkillType.DARKNESS or MobSkillType.WEAKNESS or MobSkillType.POISON or MobSkillType.SLOW => (int)(Randomizer.nextDouble() * 100),
            _ => 0
        };
    }
}
