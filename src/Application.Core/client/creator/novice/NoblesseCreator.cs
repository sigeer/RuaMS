/*
    This file is part of the HeavenMS MapleStory Server
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

using Application.Core.client.creator.novice;
using Application.Core.Servers.Services;

namespace client.creator.novice;

/**
 * @author RonanLana
 */
public class NoblesseCreator : NoviceCreator
{
    public NoblesseCreator(ChannelService channelService) : base(channelService)
    {
    }

    private CharacterFactoryRecipe createRecipe(Job job, int level, int map, int top, int bottom, int shoes, int weapon)
    {
        CharacterFactoryRecipe recipe = new CharacterFactoryRecipe(job, level, map, top, bottom, shoes, weapon);
        giveItem(recipe, ItemId.NOBLESSE_GUIDE, 1, InventoryType.ETC);
        return recipe;
    }

    private void giveItem(CharacterFactoryRecipe recipe, int itemid, int quantity, InventoryType itemType)
    {
        recipe.addStartingItem(itemid, quantity, itemType);
    }

    public override int createCharacter(int accountId, string name, int face, int hair, int skin, int top, int bottom, int shoes, int weapon, int gender)
    {
        return createNewCharacter(accountId, name, face, hair, skin, gender, createRecipe(Job.NOBLESSE, 1, MapId.STARTING_MAP_NOBLESSE, top, bottom, shoes, weapon));
    }
}
