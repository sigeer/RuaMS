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
   @Author: MedicOP - Add warparea command
*/

namespace client.command.commands.gm2;




public class WarpAreaCommand : Command
{
    public WarpAreaCommand()
    {
        setDescription("Warp all nearby players to a new map.");
    }

    public override void execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !warparea <mapid>");
            return;
        }

        try
        {
            var target = c.getChannelServer().getMapFactory().getMap(int.Parse(paramsValue[0]));
            if (target == null)
            {
                player.yellowMessage("Map ID " + paramsValue[0] + " is invalid.");
                return;
            }

            Point pos = player.getPosition();

            var characters = player.getMap().getAllPlayers();

            foreach (var victim in characters)
            {
                if (victim.getPosition().distanceSq(pos) <= 50000)
                {
                    victim.saveLocationOnWarp();
                    victim.changeMap(target, target.getRandomPlayerSpawnpoint());
                }
            }
        }
        catch (Exception ex)
        {
            player.yellowMessage("Map ID " + paramsValue[0] + " is invalid.");
        }
    }
}
