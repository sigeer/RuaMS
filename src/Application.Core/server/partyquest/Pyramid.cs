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



using Application.Core.Channel;
using Application.Core.Channel.DataProviders;
using Application.Core.Game.Relation;
using tools;

namespace server.partyquest;

/**
 * @author kevintjuh93
 */
public class Pyramid : PartyQuest
{
    public enum PyramidMode
    {
        EASY = 0, NORMAL = 1, HARD = 2, HELL = 3
    }


    int _kill = 0, _miss = 0, _cool = 0, _exp = 0, map, count;
    byte coolAdd = 5, missSub = 4, decrease = 1;//hmmm
    short gauge;
    byte rank, skill = 0, buffcount = 0;//buffcount includes buffs + skills
    sbyte stage = 0;
    PyramidMode mode;

    ScheduledFuture? _timer = null;
    ScheduledFuture? gaugeSchedule = null;

    public Pyramid(WorldChannel worldChannel, Team party, PyramidMode mode, int mapid) : base(worldChannel, party)
    {
        this.mode = mode;
        this.map = mapid;

        byte plus = (byte)mode;
        coolAdd += plus;
        missSub += plus;
        switch (plus)
        {
            case 0:
                decrease = 1;
                break;
            case 1:
            case 2:
                decrease = 2;
                break;
            case 3:
                decrease = 3;
                break;
        }
    }

    public async Task startGaugeSchedule()
    {
        if (gaugeSchedule == null)
        {
            gauge = 100;
            count = 0;
            gaugeSchedule = await worldChannel.TimerManager.register(() =>
            {
                worldChannel.Send(async w =>
                {
                    await ProcessGauge();
                });
            }, 1000);
        }
    }

    public async Task ProcessGauge()
    {
        gauge -= decrease;
        if (gauge <= 0)
        {
            await warp(MapId.NETTS_PYRAMID);
        }
    }

    public async Task kill()
    {
        _kill++;
        if (gauge < 100)
        {
            count++;
        }
        gauge++;
        await broadcastInfo("hit", _kill);
        if (gauge >= 100)
        {
            gauge = 100;
        }
        await checkBuffs();
    }

    public async Task cool()
    {
        _cool++;
        short plus = coolAdd;
        if ((gauge + coolAdd) > 100)
        {
            plus -= (short)((gauge + coolAdd) - 100);
        }
        gauge += plus;
        count += plus;
        if (gauge >= 100)
        {
            gauge = 100;
        }
        await broadcastInfo("cool", _cool);
        await checkBuffs();

    }

    public async Task miss()
    {
        _miss++;
        count -= missSub;
        gauge -= missSub;
        await broadcastInfo("miss", _miss);
    }

    public async Task<int> timer()
    {
        int value;
        if (stage > 0)
        {
            value = 180;
        }
        else
        {
            value = 120;
        }

        _timer = await worldChannel.TimerManager.schedule(() =>
        {
            worldChannel.Send(async w =>
            {
                await ProcessTimeout();
            });
        }, TimeSpan.FromSeconds(value));//, 4000
        await broadcastInfo("party", getParticipants().Count > 1 ? 1 : 0);
        await broadcastInfo("hit", _kill);
        await broadcastInfo("miss", _miss);
        await broadcastInfo("cool", _cool);
        await broadcastInfo("skill", skill);
        await broadcastInfo("laststage", stage);
        await startGaugeSchedule();
        return value;
    }

    public async Task ProcessTimeout()
    {
        stage++;
        await warp(map + (stage * 100));//Should work :D
    }

    public async Task warp(int mapid)
    {
        foreach (var chr in getParticipants())
        {
            await chr.MapModel.Send(async m =>
            {
                await chr.changeMap(mapid, 0);
            });
        }
        if (stage > -1)
        {
            gaugeSchedule?.cancel(false);
            gaugeSchedule = null;
            _timer?.cancel(false);
            _timer = null;
        }
        else
        {
            stage = 0;
        }
    }

    public async Task broadcastInfo(string info, int amount)
    {
        foreach (var chr in getParticipants())
        {
            await chr.SendPacket(PacketCreator.getEnergy("massacre_" + info, amount));
            await chr.SendPacket(PacketCreator.pyramidGauge(count));
        }
    }

    public async Task<bool> useSkill()
    {
        if (skill < 1)
        {
            return false;
        }

        skill--;
        await broadcastInfo("skill", skill);
        return true;
    }

    public async Task checkBuffs()
    {
        int total = (_kill + _cool);
        if (buffcount == 0 && total >= 250)
        {
            buffcount++;
            ItemInformationProvider ii = ItemInformationProvider.getInstance();
            foreach (var chr in getParticipants())
            {
                ii.getItemEffect(ItemId.PHARAOHS_BLESSING_1)?.applyTo(chr);
            }

        }
        else if (buffcount == 1 && total >= 500)
        {
            buffcount++;
            skill++;
            ItemInformationProvider ii = ItemInformationProvider.getInstance();
            foreach (var chr in getParticipants())
            {
                await chr.SendPacket(PacketCreator.getEnergy("massacre_skill", skill));
                ii.getItemEffect(ItemId.PHARAOHS_BLESSING_2)?.applyTo(chr);
            }
        }
        else if (buffcount == 2 && total >= 1000)
        {
            buffcount++;
            skill++;
            ItemInformationProvider ii = ItemInformationProvider.getInstance();
            foreach (var chr in getParticipants())
            {
                await chr.SendPacket(PacketCreator.getEnergy("massacre_skill", skill));
                ii.getItemEffect(ItemId.PHARAOHS_BLESSING_3)?.applyTo(chr);
            }
        }
        else if (buffcount == 3 && total >= 1500)
        {
            skill++;
            await broadcastInfo("skill", skill);
        }
        else if (buffcount == 4 && total >= 2000)
        {
            buffcount++;
            skill++;
            ItemInformationProvider ii = ItemInformationProvider.getInstance();
            foreach (var chr in getParticipants())
            {
                await chr.SendPacket(PacketCreator.getEnergy("massacre_skill", skill));
                ii.getItemEffect(ItemId.PHARAOHS_BLESSING_4)?.applyTo(chr);
            }
        }
        else if (buffcount == 5 && total >= 2500)
        {
            skill++;
            await broadcastInfo("skill", skill);
        }
        else if (buffcount == 6 && total >= 3000)
        {
            skill++;
            await broadcastInfo("skill", skill);
        }
    }

    public async Task sendScore(Player chr)
    {
        if (_exp == 0)
        {
            int totalkills = (_kill + _cool);
            if (stage == 5)
            {
                if (totalkills >= 3000)
                {
                    rank = 0;
                }
                else if (totalkills >= 2000)
                {
                    rank = 1;
                }
                else if (totalkills >= 1500)
                {
                    rank = 2;
                }
                else if (totalkills >= 500)
                {
                    rank = 3;
                }
                else
                {
                    rank = 4;
                }
            }
            else
            {
                if (totalkills >= 2000)
                {
                    rank = 3;
                }
                else
                {
                    rank = 4;
                }
            }

            if (rank == 0)
            {
                _exp = (60500 + (5500 * (byte)mode));
            }
            else if (rank == 1)
            {
                _exp = (55000 + (5000 * (byte)mode));
            }
            else if (rank == 2)
            {
                _exp = (46750 + (4250 * (byte)mode));
            }
            else if (rank == 3)
            {
                _exp = (22000 + (2000 * (byte)mode));
            }

            _exp += ((_kill * 2) + (_cool * 10));
        }
        await chr.SendPacket(PacketCreator.pyramidScore(rank, _exp));
        await chr.gainExp(_exp, true, true);
    }
}


