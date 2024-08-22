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


using client.autoban;
using net.server;
using tools;

namespace client.command.commands.gm3;

public class IgnoreCommand : Command
{
    public IgnoreCommand()
    {
        setDescription("Toggle ignore a character from auto-ban alerts.");
    }

    public override void execute(Client c, string[] paramsValue)
    {
        Character player = c.getPlayer();
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !ignore <ign>");
            return;
        }
        var victim = c.getWorldServer().getPlayerStorage().getCharacterByName(paramsValue[0]);
        if (victim == null)
        {
            player.message("Player '" + paramsValue[0] + "' could not be found on this world.");
            return;
        }

        bool ignored = AutobanFactory.toggleIgnored(victim.getId());
        player.yellowMessage(victim.getName() + " is " + (ignored ? "now being ignored." : "no longer being ignored."));
        string message_ = player.getName() + (ignored ? " has started ignoring " : " has stopped ignoring ") + victim.getName() + ".";
        Server.getInstance().broadcastGMMessage(c.getWorld(), PacketCreator.serverNotice(5, message_));

    }
}
