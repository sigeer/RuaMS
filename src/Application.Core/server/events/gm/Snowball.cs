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
public class Snowball
{
    private ISnowBallMap map;
    private int position = 0;
    private int hits = 3;
    private int snowmanhp = 1000;
    private bool hittable = false;
    private int team;
    private bool winner = false;
    List<IPlayer> characters = new();

    public Snowball(int team, IMap map)
    {
        this.map = (map as ISnowBallMap)!;
        this.team = team;

        snowmanhp = this.map.SnowManHP;

        foreach (var chr in map.getCharacters())
        {
            if (chr.getTeam() == team)
            {
                characters.Add(chr);
            }
        }
    }

    public void startEvent()
    {
        if (hittable == true)
        {
            return;
        }

        foreach (var chr in characters)
        {
            if (chr != null)
            {
                chr.sendPacket(PacketCreator.rollSnowBall(false, 1, map.getSnowball(0)!, map.getSnowball(1)!));
                chr.sendPacket(PacketCreator.getClock(600));
            }
        }
        hittable = true;
        map.ChannelServer.Container.TimerManager.schedule(() =>
        {
            var ball0 = map.getSnowball(0)!;
            var ball1 = map.getSnowball(1)!;
            var teamBall = team == 0 ? ball0 : ball1;
            var anotherTeamBall = team == 0 ? ball0 : ball1;
            if (teamBall.getPosition() > anotherTeamBall.getPosition())
            {
                foreach (var chr in characters)
                {
                    if (chr != null)
                    {
                        chr.sendPacket(PacketCreator.rollSnowBall(false, 3, ball0, ball0));
                    }
                }
                winner = true;
            }
            else if (anotherTeamBall.getPosition() > teamBall.getPosition())
            {
                foreach (var chr in characters)
                {
                    if (chr != null)
                    {
                        chr.sendPacket(PacketCreator.rollSnowBall(false, 4, ball0, ball0));
                    }
                }
                winner = true;
            } //Else
            warpOut();
        }, 600000);

    }

    public bool isHittable()
    {
        return hittable;
    }

    public void setHittable(bool hit)
    {
        this.hittable = hit;
    }

    public int getPosition()
    {
        return position;
    }

    public int getSnowmanHP()
    {
        return snowmanhp;
    }

    public void setSnowmanHP(int hp)
    {
        this.snowmanhp = hp;
    }

    public void hit(int what, int damage)
    {
        var ball0 = map.getSnowball(0)!;
        var ball1 = map.getSnowball(1)!;
        if (what < 2)
        {
            if (damage > 0)
            {
                this.hits--;
            }
            else
            {
                if (this.snowmanhp - damage < 0)
                {
                    this.snowmanhp = 0;

                    map.ChannelServer.Container.TimerManager.schedule(() =>
                    {
                        setSnowmanHP(map.SnowManHP);
                        message(5);
                    }, 10000);
                }
                else
                {
                    this.snowmanhp -= damage;
                }
                map.broadcastMessage(PacketCreator.rollSnowBall(false, 1, ball0, ball1));
            }
        }

        if (this.hits == 0)
        {
            this.position += 1;
            switch (this.position)
            {
                case 45:
                    (team == 0 ? ball1 : ball0).message(1);
                    break;
                case 290:
                    (team == 0 ? ball1 : ball0).message(2);
                    break;
                case 560:
                    (team == 0 ? ball1 : ball0).message(3);
                    break;
            }

            this.hits = 3;
            map.broadcastMessage(PacketCreator.rollSnowBall(false, 0, ball0, ball1));
            map.broadcastMessage(PacketCreator.rollSnowBall(false, 1, ball0, ball1));
        }
        map.broadcastMessage(PacketCreator.hitSnowBall(what, damage));
    }

    public void message(int message)
    {
        foreach (var chr in characters)
        {
            if (chr != null)
            {
                chr.sendPacket(PacketCreator.snowballMessage(team, message));
            }
        }
    }

    public void warpOut()
    {
        map.ChannelServer.Container.TimerManager.schedule(() =>
        {
            if (winner)
            {
                map.warpOutByTeam(team, MapId.EVENT_WINNER);
            }
            else
            {
                map.warpOutByTeam(team, MapId.EVENT_EXIT);
            }

            map.setSnowball(team, null);
        }, 10000);
    }
}