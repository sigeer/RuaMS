/*
    This file is part of the HeavenMS MapleStory Server
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


using tools;

namespace client.command.commands.gm4;

/**
 * @author Ronan
 */
public class BossDropRateCommand : Command
{
    public BossDropRateCommand()
    {
        setDescription("Set world boss drop rate.");
    }

    public override void execute(Client c, string[] paramsValue)
    {
        Character player = c.getPlayer();
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !bossdroprate <newrate>");
            return;
        }

        int bossdroprate = Math.Max(int.Parse(paramsValue[0]), 1);
        c.getWorldServer().setBossDropRate(bossdroprate);
        c.getWorldServer().broadcastPacket(PacketCreator.serverNotice(6, "[Rate] Boss Drop Rate has been changed to " + bossdroprate + "x."));
    }
}
