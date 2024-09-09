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


using tools;

namespace client.command.commands.gm4;

public class FishingRateCommand : Command
{
    public FishingRateCommand()
    {
        setDescription("Set fishing rate.");
    }

    public override void execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !fishrate <newrate>");
            return;
        }

        int fishrate = Math.Max(int.Parse(paramsValue[0]), 1);
        c.getWorldServer().FishingRate = fishrate;
        c.getWorldServer().broadcastPacket(PacketCreator.serverNotice(6, "[Rate] Fishing Rate has been changed to " + fishrate + "x."));
    }
}
