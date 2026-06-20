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
/// TODO: 待重构，让其与WorldChannel关联而不是Map
/// </summary>
public class Snowball
{
    private ISnowBallMap map;
    private int position = 0;
    private int hits = 3;
    private int snowmanhp = 1000;
    private bool hittable = false;
    private int team;
    private bool winner = false;
    List<Player> characters = new();

    public Snowball(int team, IMap map)
    {
        this.map = (map as ISnowBallMap)!;
        this.team = team;

        snowmanhp = this.map.SnowManHP;

        foreach (var chr in map.getAllPlayers())
        {
            if (chr.getTeam() == team)
            {
                characters.Add(chr);
            }
        }
    }

    public async Task startEvent()
    {
        if (hittable == true)
        {
            return;
        }

        foreach (var chr in characters)
        {
            if (chr != null)
            {
                await chr.SendPacket(PacketCreator.rollSnowBall(false, 1, map.getSnowball(0)!, map.getSnowball(1)!));
                await chr.SendPacket(PacketCreator.getClock(600));
            }
        }
        hittable = true;
        await map.ChannelServer.TimerManager.schedule(() =>
        {
            map.Send(m => ProcessTimeout());
        }, 600_000);

    }

    public async Task ProcessTimeout()
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
                    await chr.SendPacket(PacketCreator.rollSnowBall(false, 3, ball0, ball0));
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
                    await chr.SendPacket(PacketCreator.rollSnowBall(false, 4, ball0, ball0));
                }
            }
            winner = true;
        } //Else
        await warpOut();
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

    public async Task hit(int what, int damage)
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

                    await map.ChannelServer.TimerManager.schedule(() =>
                       {
                           map.Send(m => SnowmanRespawn());
                       }, 10_000);
                }
                else
                {
                    this.snowmanhp -= damage;
                }
                await map.broadcastMessage(PacketCreator.rollSnowBall(false, 1, ball0, ball1));
            }
        }

        if (this.hits == 0)
        {
            this.position += 1;
            switch (this.position)
            {
                case 45:
                    await (team == 0 ? ball1 : ball0).message(1);
                    break;
                case 290:
                    await (team == 0 ? ball1 : ball0).message(2);
                    break;
                case 560:
                    await (team == 0 ? ball1 : ball0).message(3);
                    break;
            }

            this.hits = 3;
            await map.broadcastMessage(PacketCreator.rollSnowBall(false, 0, ball0, ball1));
            await map.broadcastMessage(PacketCreator.rollSnowBall(false, 1, ball0, ball1));
        }
        await map.broadcastMessage(PacketCreator.hitSnowBall(what, damage));
    }

    public async Task SnowmanRespawn()
    {
        setSnowmanHP(map.SnowManHP);
        await message(5);
    }

    public async Task message(int message)
    {
        foreach (var chr in characters)
        {
            if (chr != null)
            {
                await chr.SendPacket(PacketCreator.snowballMessage(team, message));
            }
        }
    }

    public async Task warpOut()
    {
        await map.ChannelServer.TimerManager.schedule(() =>
        {
            map.Send(m => ProcessWarpOut());
        }, 10000);
    }

    public void ProcessWarpOut()
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
    }
}