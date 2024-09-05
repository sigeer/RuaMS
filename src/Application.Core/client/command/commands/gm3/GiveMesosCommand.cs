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

namespace client.command.commands.gm3;

public class GiveMesosCommand : Command
{
    public GiveMesosCommand()
    {
        setDescription("Give mesos to a player.");
    }

    public override void execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !givems [<playername>] <gainmeso>");
            return;
        }

        string recv_, value_;
        long mesos_ = 0;

        if (paramsValue.Length == 2)
        {
            recv_ = paramsValue[0];
            value_ = paramsValue[1];
        }
        else
        {
            recv_ = c.OnlinedCharacter.getName();
            value_ = paramsValue[0];
        }

        try
        {
            mesos_ = long.Parse(value_);
            if (mesos_ > int.MaxValue)
            {
                mesos_ = int.MaxValue;
            }
            else if (mesos_ < int.MinValue)
            {
                mesos_ = int.MinValue;
            }
        }
        catch (FormatException nfe)
        {
            if (value_ == ("max"))
            {  // "max" descriptor suggestion thanks to Vcoc
                mesos_ = int.MaxValue;
            }
            else if (value_ == ("min"))
            {
                mesos_ = int.MinValue;
            }
        }

        var victim = c.getWorldServer().getPlayerStorage().getCharacterByName(recv_);
        if (victim != null && victim.IsOnlined)
        {
            victim.gainMeso((int)mesos_, true);
            player.message("MESO given.");
        }
        else
        {
            player.message("Player '" + recv_ + "' could not be found.");
        }
    }
}
