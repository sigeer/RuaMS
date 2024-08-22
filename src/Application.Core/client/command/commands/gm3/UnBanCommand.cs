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

using Microsoft.EntityFrameworkCore;

namespace client.command.commands.gm3;




public class UnBanCommand : Command
{
    public UnBanCommand()
    {
        setDescription("Unban a player.");
    }

    public override void execute(Client c, string[] paramsValue)
    {
        Character player = c.getPlayer();
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !unban <playername>");
            return;
        }

        try
        {
            int aid = Character.getAccountIdByName(paramsValue[0]);
            using var dbContext = new DBContext();
            dbContext.Accounts.Where(x => x.Id == aid).ExecuteUpdate(x => x.SetProperty(y => y.Banned, -1));

            dbContext.Ipbans.Where(x => x.Aid == aid.ToString()).ExecuteDelete();

            dbContext.Macbans.Where(x => x.Aid == aid.ToString()).ExecuteDelete();
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
            player.message("Failed to unban " + paramsValue[0]);
            return;
        }
        player.message("Unbanned " + paramsValue[0]);
    }
}
