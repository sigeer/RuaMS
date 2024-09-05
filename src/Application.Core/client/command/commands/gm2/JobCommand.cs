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

namespace client.command.commands.gm2;

public class JobCommand : Command
{
    public JobCommand()
    {
        setDescription("Change job of a player.");
    }

    public override void execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length == 1)
        {
            int jobid = int.Parse(paramsValue[0]);
            if (jobid < 0 || jobid >= 2200)
            {
                player.message("Jobid " + jobid + " is not available.");
                return;
            }

            player.changeJob(JobUtils.getById(jobid));
            player.equipChanged();
        }
        else if (paramsValue.Length == 2)
        {
            var victim = c.getWorldServer().getPlayerStorage().getCharacterByName(paramsValue[0]);

            if (victim != null && victim.IsOnlined)
            {
                int jobid = int.Parse(paramsValue[1]);
                if (jobid < 0 || jobid >= 2200)
                {
                    player.message("Jobid " + jobid + " is not available.");
                    return;
                }

                victim.changeJob(JobUtils.getById(jobid));
                player.equipChanged();
            }
            else
            {
                player.message("Player '" + paramsValue[0] + "' could not be found.");
            }
        }
        else
        {
            player.message("Syntax: !job <job id> <opt: IGN of another person>");
        }
    }
}
