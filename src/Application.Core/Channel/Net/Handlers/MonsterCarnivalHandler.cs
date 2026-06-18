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
using Application.Core.scripting.Events.Instances;
using Microsoft.Extensions.Logging;
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

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        {
            await c.tryacquireClient();
            try
            {
                try
                {
                    var eim = c.OnlinedCharacter.getEventInstance() as MonsterCarnivalEventInstanceManager;
                    if (eim == null)
                    {
                        await c.SendPacket(PacketCreator.enableActions());
                        throw new BusinessOutOfInstance();
                    }

                    var playerData = eim.GetPlayerData(c.OnlinedCharacter.Id);
                    if (playerData == null)
                    {
                        await eim.exitPlayer(c.OnlinedCharacter);
                        await c.SendPacket(PacketCreator.enableActions());
                        throw new BusinessOutOfInstance();
                    }

                    int tab = p.ReadSByte();
                    int num = p.ReadSByte();
                    int neededCP = 0;
                    if (tab == 0)
                    {
                        var map = (c.OnlinedCharacter.getMap() as ICPQMap)!;

                        var mobs = map.getMobsToSpawn();
                        if (num >= mobs.Count || playerData.AvailableCP < mobs[num].Value)
                        {
                            await c.SendPacket(PacketCreator.CPQMessage(1));
                            await c.SendPacket(PacketCreator.enableActions());
                            return;
                        }

                        if (!eim.CanSummon(c.OnlinedCharacter))
                        {
                            await c.SendPacket(PacketCreator.CPQMessage(2));
                            await c.SendPacket(PacketCreator.enableActions());
                            return;
                        }

                        eim.Summon(c.OnlinedCharacter);

                        var spawnPos = map.getRandomSP(playerData.TeamFlag);
                        await c.OnlinedCharacter.getMap().addMonsterSpawn(mobs[num].Key, spawnPos, 1, playerData.TeamFlag);
                        await c.SendPacket(PacketCreator.enableActions());

                        neededCP = mobs[num].Value;
                    }
                    else if (tab == 1)
                    {
                        var map = (c.OnlinedCharacter.getMap() as ICPQMap)!;
                        //debuffs
                        List<int> skillid = map.GetSkillIds();
                        if (num >= skillid.Count)
                        {
                            await c.OnlinedCharacter.dropMessage(5, "An unexpected error has occurred.");
                            await c.SendPacket(PacketCreator.enableActions());
                            return;
                        }
                        var skill = CarnivalFactory.getInstance().getSkill(skillid[num]); //ugh wtf
                        if (skill == null || playerData.AvailableCP < skill.cpLoss)
                        {
                            await c.SendPacket(PacketCreator.CPQMessage(1));
                            await c.SendPacket(PacketCreator.enableActions());
                            return;
                        }
                        var dis = skill.getDisease();
                        var enemies = eim.GetEnemyMembers(c.OnlinedCharacter);
                        if (skill.targetsAll)
                        {
                            int hitChance = rollHitChance((MobSkillType)skill.MobSkillId);
                            if (hitChance <= 80)
                            {
                                foreach (var mc in enemies)
                                {
                                    if (mc.IsOnlined)
                                    {
                                        if (dis == null)
                                        {
                                            await mc.dispel();
                                        }
                                        else
                                        {
                                            await mc.giveDebuff(dis, skill.getSkill());
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            var chrApp = Randomizer.Select(enemies);
                            if (chrApp != null && chrApp.getMap().isCPQMap())
                            {
                                if (dis == null)
                                {
                                    await chrApp.dispel();
                                }
                                else
                                {
                                    await chrApp.giveDebuff(dis, skill.getSkill());
                                }
                            }
                        }
                        neededCP = skill.cpLoss;
                        await c.SendPacket(PacketCreator.enableActions());
                    }
                    else if (tab == 2)
                    {
                        //protectors
                        var skill = CarnivalFactory.getInstance().getGuardian(num);
                        if (skill == null || playerData.AvailableCP < skill.cpLoss)
                        {
                            await c.SendPacket(PacketCreator.CPQMessage(1));
                            await c.SendPacket(PacketCreator.enableActions());
                            return;
                        }

                        var map = await eim.GetEventMap();
                        if (!eim.CanGuardian(c.OnlinedCharacter, map))
                        {
                            await c.SendPacket(PacketCreator.CPQMessage(2));
                            await c.SendPacket(PacketCreator.enableActions());
                            return;
                        }

                        int success = await map.spawnGuardian(playerData.TeamFlag, num);
                        if (success != 1)
                        {
                            switch (success)
                            {
                                case -1:
                                    await c.SendPacket(PacketCreator.CPQMessage(3));
                                    break;

                                case 0:
                                    await c.SendPacket(PacketCreator.CPQMessage(4));
                                    break;

                                default:
                                    await c.SendPacket(PacketCreator.CPQMessage(3));
                                    break;
                            }
                            await c.SendPacket(PacketCreator.enableActions());
                            return;
                        }
                        else
                        {
                            neededCP = skill.cpLoss;
                        }
                    }
                    await eim.GainCP(c.OnlinedCharacter, -neededCP);
                    await c.OnlinedCharacter.getMap().broadcastMessage(PacketCreator.CPQ_PlayerSummoned(c.OnlinedCharacter.getName(), tab, num));
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
