/*
	This file is part of the OdinMS Maple Story Server
    Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
		       Matthias Butz <matze@odinms.de>
		       Jan Christian Meyer <vimes@odinms.de>

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


using provider;
using provider.wz;

namespace client.inventory;


/**
 * @author Danny (Leifde)
 */
public class PetDataFactory
{
    private static DataProvider dataRoot = DataProviderFactory.getDataProvider(WZFiles.ITEM);
    private static Dictionary<string, PetCommand> petCommands = new();
    private static Dictionary<int, int> petHunger = new();

    public static PetCommand getPetCommand(int petId, int skillId)
    {
        var ret = petCommands.GetValueOrDefault(petId + "" + skillId);
        if (ret != null)
        {
            return ret;
        }
        lock (petCommands)
        {
            ret = petCommands.GetValueOrDefault(petId + "" + skillId);
            if (ret == null)
            {
                Data skillData = dataRoot.getData("Pet/" + petId + ".img");
                int prob = 0;
                int inc = 0;
                if (skillData != null)
                {
                    prob = DataTool.getInt("interact/" + skillId + "/prob", skillData, 0);
                    inc = DataTool.getInt("interact/" + skillId + "/inc", skillData, 0);
                }
                ret = new PetCommand(petId, skillId, prob, inc);
                petCommands.AddOrUpdate(petId + "" + skillId, ret);
            }
            return ret;
        }
    }

    public static int getHunger(int petId)
    {
        var ret = petHunger.get(petId);
        if (ret != null)
        {
            return ret.Value;
        }
        lock (petHunger)
        {
            ret = petHunger.get(petId);
            if (ret == null)
            {
                ret = DataTool.getInt(dataRoot.getData("Pet/" + petId + ".img").getChildByPath("info/hungry"), 1);
            }
            return ret.Value;
        }
    }
}
