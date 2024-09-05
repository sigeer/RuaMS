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
   @Author: Arthur L - Refactored command content into modules
*/


using net.server;
using server.expeditions;

namespace client.command.commands.gm3;




public class ExpedsCommand : Command
{
    public ExpedsCommand()
    {
        setDescription("Show all ongoing boss expeditions.");
    }

    public override void execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        foreach (var ch in Server.getInstance().getChannelsFromWorld(c.getWorld()))
        {
            List<Expedition> expeds = ch.getExpeditions();
            if (expeds.Count == 0)
            {
                player.yellowMessage("No Expeditions in Channel " + ch.getId());
                continue;
            }
            player.yellowMessage("Expeditions in Channel " + ch.getId());
            int id = 0;
            foreach (Expedition exped in expeds)
            {
                id++;
                player.yellowMessage("> Expedition " + id);
                player.yellowMessage(">> Type: " + exped.getType().ToString());
                player.yellowMessage(">> Status: " + (exped.isRegistering() ? "REGISTERING" : "UNDERWAY"));
                player.yellowMessage(">> Size: " + exped.getMembers().Count);
                player.yellowMessage(">> Leader: " + exped.getLeader().getName());
                int memId = 2;
                foreach (var e in exped.getMembers())
                {
                    if (exped.isLeader(e.Key))
                    {
                        continue;
                    }
                    player.yellowMessage(">>> Member " + memId + ": " + e.Value);
                    memId++;
                }
            }
        }
    }
}
