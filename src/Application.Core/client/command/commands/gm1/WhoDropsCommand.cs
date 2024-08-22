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


using constants.id;
using server;
using server.life;

namespace client.command.commands.gm1;

public class WhoDropsCommand : Command
{
    public WhoDropsCommand()
    {
        setDescription("Show what drops an item.");
    }

    public override void execute(Client c, string[] paramsValue)
    {
        Character player = c.getPlayer();
        if (paramsValue.Length < 1)
        {
            player.dropMessage(5, "Please do @whodrops <item name>");
            return;
        }

        if (c.tryacquireClient())
        {
            try
            {
                string searchString = player.getLastCommandMessage();
                string output = "";
                var items = ItemInformationProvider.getInstance().getItemDataByName(searchString).Take(3);
                foreach (var data in items)
                {
                    output += "#b" + data.Name + "#k is dropped by:\r\n";

                    try
                    {
                        using var dbContext = new DBContext();
                        var ds = dbContext.DropData.Where(x => x.Itemid == data.Id).Take(50);

                        foreach (var item in ds)
                        {
                            string resultName = MonsterInformationProvider.getInstance().getMobNameFromId(item.Dropperid);
                            if (resultName != null)
                            {
                                output += resultName + ", ";
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        player.dropMessage(6, "There was a problem retrieving the required data. Please try again.");
                        log.Error(e.ToString());
                        return;
                    }
                    output += "\r\n\r\n";
                }
                c.getAbstractPlayerInteraction().npcTalk(NpcId.MAPLE_ADMINISTRATOR, output);
            }
            finally
            {
                c.releaseClient();
            }
        }
        else
        {
            player.dropMessage(5, "Please wait a while for your request to be processed.");
        }
    }
}
