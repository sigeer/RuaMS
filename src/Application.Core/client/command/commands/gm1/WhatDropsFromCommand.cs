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


using constants.id;
using server;
using server.life;

namespace client.command.commands.gm1;



public class WhatDropsFromCommand : Command
{
    public WhatDropsFromCommand()
    {
        setDescription("Show what items drop from a mob.");
    }

    public override void execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.dropMessage(5, "Please do @whatdropsfrom <monster name>");
            return;
        }
        string monsterName = player.getLastCommandMessage();
        string output = "";
        int limit = 3;
        var dataList = MonsterInformationProvider.getMobsIDsFromName(monsterName).Take(limit);
        foreach (var data in dataList)
        {

            int mobId = data.Key;
            string mobName = data.Value;
            output += mobName + " drops the following items:\r\n\r\n";
            foreach (MonsterDropEntry drop in MonsterInformationProvider.getInstance().retrieveDrop(mobId))
            {
                try
                {
                    var name = ItemInformationProvider.getInstance().getName(drop.itemId);
                    if (name == null || name.Equals("null") || drop.chance == 0)
                    {
                        continue;
                    }
                    float chance = Math.Max(1000000 / drop.chance / (!MonsterInformationProvider.getInstance().isBoss(mobId) ? player.getDropRate() : player.getBossDropRate()), 1);
                    output += "- " + name + " (1/" + (int)chance + ")\r\n";
                }
                catch (Exception ex)
                {
                    log.Error(ex.ToString());
                    continue;
                }
            }
            output += "\r\n";

        }

        c.getAbstractPlayerInteraction().npcTalk(NpcId.MAPLE_ADMINISTRATOR, output);
    }
}
