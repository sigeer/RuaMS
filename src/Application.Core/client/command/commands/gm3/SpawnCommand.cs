/*
    This file is part of the HeavenMS MapleStory Server, commands OdinMS-based
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
   @Author: Arthur L - Refactored command content into modules
*/


using server.life;

namespace client.command.commands.gm3;

public class SpawnCommand : Command
{
    public SpawnCommand()
    {
        setDescription("Spawn mob(s) on your location.");
    }

    public override void execute(Client c, string[] paramsValue)
    {
        Character player = c.getPlayer();
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !spawn <mobid> [<mobqty>]");
            return;
        }

        Monster monster = LifeFactory.getMonster(int.Parse(paramsValue[0]));
        if (monster == null)
        {
            return;
        }
        if (paramsValue.Length == 2)
        {
            for (int i = 0; i < int.Parse(paramsValue[1]); i++)
            {
                player.getMap().spawnMonsterOnGroundBelow(LifeFactory.getMonster(int.Parse(paramsValue[0])), player.getPosition());
            }
        }
        else
        {
            player.getMap().spawnMonsterOnGroundBelow(LifeFactory.getMonster(int.Parse(paramsValue[0])), player.getPosition());
        }
    }
}
