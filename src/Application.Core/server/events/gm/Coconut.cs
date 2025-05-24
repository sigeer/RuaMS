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


/**
 * @author kevintjuh93
 */
//Make them better :)
public class Coconut : Event
{
    private ICoconutMap map;
    private int MapleScore = 0;
    private int StoryScore = 0;
    private int countBombing = 80;
    private int countFalling = 401;
    private int countStopped = 20;
    private List<Coconuts> coconuts = new();

    public Coconut(IMap map) : base(map.getId(), 50)
    {
        this.map = (map as ICoconutMap)!;
        countBombing = this.map.CountBombing;
        countFalling = this.map.CountFalling;
        countStopped = this.map.CountStopped;
    }

    public void startEvent()
    {
        map.startEvent();
        for (int i = 0; i < 506; i++)
        {
            coconuts.Add(new Coconuts(i));
        }
        map.broadcastMessage(PacketCreator.hitCoconut(true, 0, 0));
        setCoconutsHittable(true);
        map.broadcastMessage(PacketCreator.getClock(map.TimeDefault));

        TimerManager.getInstance().schedule(Check, TimeSpan.FromSeconds(map.TimeDefault));
    }

    private void Check()
    {
        if (map.getId() == MapId.EVENT_COCONUT_HARVEST)
        {
            if (getMapleScore() == getStoryScore())
            {
                bonusTime();
            }
            else
            {
                var winnerTeam = getMapleScore() > getStoryScore() ? 0 : 1;
                foreach (var chr in map.getCharacters())
                {
                    if (chr.getTeam() == winnerTeam)
                    {
                        chr.sendPacket(PacketCreator.showEffect(map.EffectWin));
                        chr.sendPacket(PacketCreator.playSound(map.SoundWin));
                    }
                    else
                    {
                        chr.sendPacket(PacketCreator.showEffect(map.EffectLose));
                        chr.sendPacket(PacketCreator.playSound(map.SoundLose));
                    }
                }
                warpOut();
            }
        }
    }
    public void bonusTime()
    {
        map.broadcastMessage(PacketCreator.getClock(map.TimeExpand));
        TimerManager.getInstance().schedule(Check, TimeSpan.FromSeconds(map.TimeExpand));

    }

    public void warpOut()
    {
        setCoconutsHittable(false);
        TimerManager.getInstance().schedule(() =>
        {
            List<IPlayer> chars = new(map.getCharacters());

            foreach (var chr in chars)
            {
                if ((getMapleScore() > getStoryScore() && chr.getTeam() == 0) || (getStoryScore() > getMapleScore() && chr.getTeam() == 1))
                {
                    chr.changeMap(MapId.EVENT_WINNER);
                }
                else
                {
                    chr.changeMap(map.getForcedReturnId());
                }
            }
            map.Coconut = null;
        }, TimeSpan.FromSeconds(map.TimeFinish));
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