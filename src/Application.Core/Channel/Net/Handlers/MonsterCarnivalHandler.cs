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


using Application.Core.Game.Maps.Specials;
using Microsoft.Extensions.Logging;
using server.life;
using server.partyquest;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author Drago (Dragohe4rt)
 */

public class MonsterCarnivalHandler : ChannelHandlerBase
{
    readonly ILogger<MonsterCarnivalHandler> _logger;

    public MonsterCarnivalHandler(ILogger<MonsterCarnivalHandler> logger)
    {
        _logger = logger;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        if (c.tryacquireClient())
        {
            try
            {
                try
                {
                    int tab = p.ReadSByte();
                    int num = p.ReadSByte();
                    int neededCP = 0;
                    if (tab == 0)
                    {
                        var map = (c.OnlinedCharacter.getMap() as ICPQMap)!;

                        var mobs = map.getMobsToSpawn();
                        if (num >= mobs.Count || c.OnlinedCharacter.AvailableCP < mobs[num].Value)
                        {
                            c.sendPacket(PacketCreator.CPQMessage(1));
                            c.sendPacket(PacketCreator.enableActions());
                            return;
                        }

                        var mob = LifeFactory.Instance.GetMonsterTrust(mobs[num].Key);
                        if (c.OnlinedCharacter.MCTeam != null)
                        {
                            if (!c.OnlinedCharacter.MCTeam.CanSummon())
                            {
                                c.sendPacket(PacketCreator.CPQMessage(2));
                                c.sendPacket(PacketCreator.enableActions());
                                return;
                            }

                            c.OnlinedCharacter.MCTeam.Summon();

                            var spawnPos = map.getRandomSP(c.OnlinedCharacter.MCTeam.TeamFlag);
                            mob.setPosition(spawnPos);

                            c.OnlinedCharacter.getMap().addMonsterSpawn(mob, 1, c.OnlinedCharacter.MCTeam.TeamFlag);
                            c.OnlinedCharacter.getMap().addAllMonsterSpawn(mob, 1, c.OnlinedCharacter.MCTeam.TeamFlag);
                            c.sendPacket(PacketCreator.enableActions());
                        }

                        neededCP = mobs[num].Value;
                    }
                    else if (tab == 1)
                    {
                        var map = (c.OnlinedCharacter.getMap() as ICPQMap)!;
                        //debuffs
                        List<int> skillid = map.GetSkillIds();
                        if (num >= skillid.Count)
                        {
                            c.OnlinedCharacter.dropMessage(5, "An unexpected error has occurred.");
                            c.sendPacket(PacketCreator.enableActions());
                            return;
                        }
                        var skill = CarnivalFactory.getInstance().getSkill(skillid[num]); //ugh wtf
                        if (skill == null || c.OnlinedCharacter.AvailableCP < skill.cpLoss)
                        {
                            c.sendPacket(PacketCreator.CPQMessage(1));
                            c.sendPacket(PacketCreator.enableActions());
                            return;
                        }
                        var dis = skill.getDisease();
                        var enemies = c.OnlinedCharacter.MCTeam!.Enemy!;
                        if (skill.targetsAll)
                        {
                            int hitChance = rollHitChance((MobSkillType)skill.MobSkillId);
                            if (hitChance <= 80)
                            {
                                foreach (var mc in enemies.Team.GetChannelMembers(c.CurrentServer))
                                {
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
                            int amount = enemies.Team.GetChannelMembers(c.CurrentServer).Count - 1;
                            int randd = (int)Math.Floor(Randomizer.nextDouble() * amount);
                            var chrApp = c.OnlinedCharacter.getMap().getCharacterById(enemies.Team.GetRandomMemberId());
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
                    {
                        //protectors
                        var skill = CarnivalFactory.getInstance().getGuardian(num);
                        if (skill == null || c.OnlinedCharacter.AvailableCP < skill.cpLoss)
                        {
                            c.sendPacket(PacketCreator.CPQMessage(1));
                            c.sendPacket(PacketCreator.enableActions());
                            return;
                        }

                        if (c.OnlinedCharacter.MCTeam != null)
                        {
                            if (!c.OnlinedCharacter.MCTeam.CanGuardian())
                            {
                                c.sendPacket(PacketCreator.CPQMessage(2));
                                c.sendPacket(PacketCreator.enableActions());
                                return;
                            }

                            var map = c.OnlinedCharacter.getMap() as ICPQMap;
                            int success = map.spawnGuardian(c.OnlinedCharacter.MCTeam.TeamFlag, num);
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
                    c.OnlinedCharacter.gainCP(-neededCP);
                    c.OnlinedCharacter.getMap().broadcastMessage(PacketCreator.playerSummoned(c.OnlinedCharacter.getName(), tab, num));
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                }
            }
            finally
            {
                c.releaseClient();
            }
        }
    }

    private int rollHitChance(MobSkillType? type)
    {
        return (type) switch
        {
            MobSkillType.DARKNESS or MobSkillType.WEAKNESS or MobSkillType.POISON or MobSkillType.SLOW => (int)(Randomizer.nextDouble() * 100),
            _ => 0
        };
    }
}
