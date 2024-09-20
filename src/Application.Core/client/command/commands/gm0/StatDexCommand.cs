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

namespace client.command.commands.gm0;

public class StatDexCommand : Command
{
    public StatDexCommand()
    {
        setDescription("Assign AP into DEX.");
    }

    public override void execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        int remainingAp = player.getRemainingAp();

        int amount;
        if (paramsValue.Length > 0)
        {
            try
            {
                amount = Math.Min(int.Parse(paramsValue[0]), remainingAp);
            }
            catch (Exception e)
            {
                log.Warning(e.ToString());
                player.dropMessage("That is not a valid number!");
                return;
            }
        }
        else
        {
            amount = Math.Min(remainingAp, YamlConfig.config.server.MAX_AP - player.getDex());
        }
        if (!player.assignDex(Math.Max(amount, 0)))
        {
            player.dropMessage("Please make sure your AP is not over " + YamlConfig.config.server.MAX_AP + " and you have enough to distribute.");
        }
    }
}
