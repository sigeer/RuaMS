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



using Application.Core.Game.Relation;
using Application.Core.scripting.Event;
using constants.id;
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
    byte rank, skill = 0,  buffcount = 0;//buffcount includes buffs + skills
    sbyte stage = 0;
    PyramidMode mode;

    ScheduledFuture? _timer = null;
    ScheduledFuture? gaugeSchedule = null;

    public Pyramid(ITeam party, PyramidMode mode, int mapid) : base(party)
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

    public void startGaugeSchedule()
    {
        if (gaugeSchedule == null)
        {
            gauge = 100;
            count = 0;
            gaugeSchedule = TimerManager.getInstance().register(() =>
            {
                gauge -= decrease;
                if (gauge <= 0)
                {
                    warp(MapId.NETTS_PYRAMID);
                }

            }, 1000);
        }
    }

    public void kill()
    {
        _kill++;
        if (gauge < 100)
        {
            count++;
        }
        gauge++;
        broadcastInfo("hit", _kill);
        if (gauge >= 100)
        {
            gauge = 100;
        }
        checkBuffs();
    }

    public void cool()
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
        broadcastInfo("cool", _cool);
        checkBuffs();

    }

    public void miss()
    {
        _miss++;
        count -= missSub;
        gauge -= missSub;
        broadcastInfo("miss", _miss);
    }

    public int timer()
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

        _timer = TimerManager.getInstance().schedule(() =>
        {
            stage++;
            warp(map + (stage * 100));//Should work :D
        }, TimeSpan.FromSeconds(value));//, 4000
        broadcastInfo("party", getParticipants().Count > 1 ? 1 : 0);
        broadcastInfo("hit", _kill);
        broadcastInfo("miss", _miss);
        broadcastInfo("cool", _cool);
        broadcastInfo("skill", skill);
        broadcastInfo("laststage", stage);
        startGaugeSchedule();
        return value;
    }

    public void warp(int mapid)
    {
        foreach (var chr in getParticipants())
        {
            chr.changeMap(mapid, 0);
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

    public void broadcastInfo(string info, int amount)
    {
        foreach (var chr in getParticipants())
        {
            chr.sendPacket(PacketCreator.getEnergy("massacre_" + info, amount));
            chr.sendPacket(PacketCreator.pyramidGauge(count));
        }
    }

    public bool useSkill()
    {
        if (skill < 1)
        {
            return false;
        }

        skill--;
        broadcastInfo("skill", skill);
        return true;
    }

    public void checkBuffs()
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
                chr.sendPacket(PacketCreator.getEnergy("massacre_skill", skill));
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
                chr.sendPacket(PacketCreator.getEnergy("massacre_skill", skill));
                ii.getItemEffect(ItemId.PHARAOHS_BLESSING_3)?.applyTo(chr);
            }
        }
        else if (buffcount == 3 && total >= 1500)
        {
            skill++;
            broadcastInfo("skill", skill);
        }
        else if (buffcount == 4 && total >= 2000)
        {
            buffcount++;
            skill++;
            ItemInformationProvider ii = ItemInformationProvider.getInstance();
            foreach (var chr in getParticipants())
            {
                chr.sendPacket(PacketCreator.getEnergy("massacre_skill", skill));
                ii.getItemEffect(ItemId.PHARAOHS_BLESSING_4)?.applyTo(chr);
            }
        }
        else if (buffcount == 5 && total >= 2500)
        {
            skill++;
            broadcastInfo("skill", skill);
        }
        else if (buffcount == 6 && total >= 3000)
        {
            skill++;
            broadcastInfo("skill", skill);
        }
    }

    public void sendScore(IPlayer chr)
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
        chr.sendPacket(PacketCreator.pyramidScore(rank, _exp));
        chr.gainExp(_exp, true, true);
    }
}


