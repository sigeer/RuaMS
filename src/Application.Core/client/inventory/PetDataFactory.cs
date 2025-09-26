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

using Application.Templates.Item.Pet;
using Application.Templates.Providers;
using Application.Templates.XmlWzReader.Provider;
using System.Collections.Concurrent;

namespace client.inventory;


/**
 * @author Danny (Leifde)
 */
public class PetDataFactory
{
    static ConcurrentDictionary<string, PetCommand> petCommands = new();
    static ItemProvider _itemProvider = ProviderFactory.GetProvider<ItemProvider>();

    public static PetCommand? getPetCommand(int petId, int skillId)
    {
        var commandKey = $"{petId}{skillId}";
        if (petCommands.TryGetValue(commandKey, out var ret))
            return ret;

        var skillData = _itemProvider.GetRequiredItem<PetItemTemplate>(petId)?.InterActs.FirstOrDefault(x => x.Id == skillId);
        if (skillData != null)
        {
            ret = new PetCommand(petId, skillId, skillData.Prob, skillData.Inc);
            petCommands[commandKey] = ret;
        }
        return ret;
    }

    public static int getHunger(int petId)
    {
        return _itemProvider.GetRequiredItem<PetItemTemplate>(petId)?.Hungry ?? 1;
    }
}
