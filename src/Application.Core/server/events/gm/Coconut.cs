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



using Application.Core.Game.Maps;
using Application.Core.Game.Maps.Specials;
using tools;

namespace server.events.gm;



/// <summary>
/// TODO: 待重构，让其与WorldChannel关联或EventInstance
/// </summary>
public class Coconut
{
    public ICoconutMap Map { get; }
    private int MapleScore = 0;
    private int StoryScore = 0;
    private int countBombing = 80;
    private int countFalling = 401;
    private int countStopped = 20;
    private List<Coconuts> coconuts = new();

    public Coconut(IMap map)
    {
        this.Map = (map as ICoconutMap)!;
        countBombing = this.Map.CountBombing;
        countFalling = this.Map.CountFalling;
        countStopped = this.Map.CountStopped;
    }

    public async Task startEvent()
    {
        Map.startEvent();
        for (int i = 0; i < 506; i++)
        {
            coconuts.Add(new Coconuts(this, i));
        }
        await Map.broadcastMessage(PacketCreator.hitCoconut(true, 0, 0));
        setCoconutsHittable(true);

        await StartTimer(Map.TimeDefault);
    }

    public async Task StartTimer(int time)
    {
        await Map.broadcastMessage(PacketCreator.getClock(time));

        await Map.ChannelServer.TimerManager.schedule(() =>
        {
            Map.Send(async m =>
            {
                await Check();
            });
        }, TimeSpan.FromSeconds(time));
    }

    public async Task Check()
    {
        if (Map.getId() == MapId.EVENT_COCONUT_HARVEST)
        {
            if (getMapleScore() == getStoryScore())
            {
                await bonusTime();
            }
            else
            {
                var winnerTeam = getMapleScore() > getStoryScore() ? 0 : 1;
                foreach (var chr in Map.getAllPlayers())
                {
                    if (chr.getTeam() == winnerTeam)
                    {
                        await chr.SendPacket(PacketCreator.showEffect(Map.EffectWin));
                        await chr.SendPacket(PacketCreator.playSound(Map.SoundWin));
                    }
                    else
                    {
                        await chr.SendPacket(PacketCreator.showEffect(Map.EffectLose));
                        await chr.SendPacket(PacketCreator.playSound(Map.SoundLose));
                    }
                }
                await warpOut();
            }
        }
    }
    public async Task bonusTime()
    {
        await StartTimer(Map.TimeExpand);
    }

    public async Task warpOut()
    {
        setCoconutsHittable(false);
        await Map.ChannelServer.TimerManager.schedule(() =>
        {
            Map.Send(async m =>
            {
                await ProcessWarpOut();
            });
        }, TimeSpan.FromSeconds(Map.TimeFinish));
    }

    public async Task ProcessWarpOut()
    {
        List<Player> chars = new(Map.getAllPlayers());

        foreach (var chr in chars)
        {
            if ((getMapleScore() > getStoryScore() && chr.getTeam() == 0) || (getStoryScore() > getMapleScore() && chr.getTeam() == 1))
            {
                await chr.changeMap(MapId.EVENT_WINNER);
            }
            else
            {
                await chr.changeMap(Map.getForcedReturnId());
            }
        }
        Map.Coconut = null;
    }

    public int getMapleScore()
    {
        return MapleScore;
    }

    public int getStoryScore()
    {
        return StoryScore;
    }

    public void addMapleScore()
    {
        this.MapleScore += 1;
    }

    public void addStoryScore()
    {
        this.StoryScore += 1;
    }

    public int getBombings()
    {
        return countBombing;
    }

    public void bombCoconut()
    {
        countBombing--;
    }

    public int getFalling()
    {
        return countFalling;
    }

    public void fallCoconut()
    {
        countFalling--;
    }

    public int getStopped()
    {
        return countStopped;
    }

    public void stopCoconut()
    {
        countStopped--;
    }

    public Coconuts getCoconut(int id)
    {
        return coconuts.get(id);
    }

    public List<Coconuts> getAllCoconuts()
    {
        return coconuts;
    }

    public void setCoconutsHittable(bool hittable)
    {
        foreach (Coconuts nut in coconuts)
        {
            nut.setHittable(hittable);
        }
    }
}