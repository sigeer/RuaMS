/*
    This file is part of the HeavenMS MapleStory NewServer, commands OdinMS-based
    Copyleft (L) 2016 - 2019 RonanLana

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

/*
   @Author: Ronan
*/


using Application.Core.constants.game;
using server.life;

namespace client.command.commands.gm4;

public class PmobCommand : Command
{
    public PmobCommand()
    {
        setDescription("Spawn a permanent mob on your location.");
    }

    public override void execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !pmob <mobid> [<mobtime>]");
            return;
        }

        // command suggestion thanks to HighKey21, none, bibiko94 (TAYAMO), asafgb
        int mapId = player.getMapId();
        int mobId = int.Parse(paramsValue[0]);
        int mobTime = (paramsValue.Length > 1) ? int.Parse(paramsValue[1]) : -1;

        Point checkpos = player.getMap().getGroundBelow(player.getPosition());
        int xpos = checkpos.X;
        int ypos = checkpos.Y;
        int fh = player.getMap().getFootholds().findBelow(checkpos).getId();

        var mob = LifeFactory.getMonster(mobId);
        if (mob != null && !mob.getName().Equals("MISSINGNO"))
        {
            mob.setPosition(checkpos);
            mob.setCy(ypos);
            mob.setRx0(xpos + 50);
            mob.setRx1(xpos - 50);
            mob.setFh(fh);
            try
            {

                using var dbContext = new DBContext();
                Plife newModel = new Plife(player.getWorld(), mapId, mobId, mobTime, xpos, ypos, fh, LifeType.Monster);
                dbContext.Plives.Add(newModel);
                dbContext.SaveChanges();

                foreach (var ch in player.getWorldServer().getChannels())
                {
                    var map = ch.getMapFactory().getMap(mapId);
                    map.addMonsterSpawn(mob, mobTime, -1);
                    map.addAllMonsterSpawn(mob, mobTime, -1);
                }

                player.yellowMessage("Pmob created.");
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                player.dropMessage(5, "Failed to store pmob in the database.");
            }
        }
        else
        {
            player.dropMessage(5, "You have entered an invalid mob id.");
        }
    }
}