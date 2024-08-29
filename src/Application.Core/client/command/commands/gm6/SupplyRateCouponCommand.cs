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

namespace client.command.commands.gm6;

public class SupplyRateCouponCommand : Command
{
    public SupplyRateCouponCommand()
    {
        setDescription("Set availability of coupons in Cash Shop.");
    }

    public override void execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.dropMessage(5, "Syntax: !supplyratecoupon <yes|no>");
            return;
        }

        YamlConfig.config.server.USE_SUPPLY_RATE_COUPONS = !paramsValue[0].Equals("no", StringComparison.OrdinalIgnoreCase);
        player.dropMessage(5, "Rate coupons are now " + (YamlConfig.config.server.USE_SUPPLY_RATE_COUPONS ? "enabled" : "disabled") + " for purchase at the Cash Shop.");
    }
}
