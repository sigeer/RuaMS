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


using Application.Core.client.creator.veteran;
using Application.Core.Game.Skills;
using Application.Core.Servers.Services;
using client.inventory;
using server;

namespace client.creator.veteran;

/**
 * @author RonanLana
 */
public class MagicianCreator : VeteranCreator
{
    private static int[] equips = { 0, ItemId.PURPLE_FAIRY_TOP, 0, ItemId.PURPLE_FAIRY_SKIRT, ItemId.RED_MAGICSHOES };
    private static int[] weapons = { ItemId.MITHRIL_WAND, ItemId.CIRCLE_WINDED_STAFF };
    private static int[] startingHpMp = { 405, 729 };
    private static int[] mpGain = { 0, 40, 80, 118, 156, 194, 230, 266, 302, 336, 370 };

    public MagicianCreator(ChannelService channelService) : base(channelService)
    {
    }

    private CharacterFactoryRecipe createRecipe(Job job, int level, int map, int top, int bottom, int shoes, int weapon, int gender, int improveSp)
    {
        CharacterFactoryRecipe recipe = new CharacterFactoryRecipe(job, level, map, top, bottom, shoes, weapon);
        ItemInformationProvider ii = ItemInformationProvider.getInstance();

        recipe.setInt(20);
        recipe.setRemainingAp(138);
        recipe.setRemainingSp(67);

        recipe.setMaxHp(startingHpMp[0]);
        recipe.setMaxMp(startingHpMp[1] + mpGain[improveSp]);

        recipe.setMeso(100000);

        if (gender == 0)
        {
            giveEquipment(recipe, ii, ItemId.BLUE_WIZARD_ROBE);
        }

        for (int i = 1; i < weapons.Length; i++)
        {
            giveEquipment(recipe, ii, weapons[i]);
        }

        giveItem(recipe, ItemId.ORANGE_POTION, 100, InventoryType.USE);
        giveItem(recipe, ItemId.MANA_ELIXIR, 100, InventoryType.USE);
        giveItem(recipe, ItemId.RELAXER, 1, InventoryType.SETUP);

        if (improveSp > 0)
        {
            improveSp += 5;
            recipe.setRemainingSp(recipe.getRemainingSp() - improveSp);

            int toUseSp = 5;
            Skill improveMpRec = SkillFactory.GetSkillTrust(Magician.IMPROVED_MP_RECOVERY);
            recipe.addStartingSkillLevel(improveMpRec, toUseSp);
            improveSp -= toUseSp;

            if (improveSp > 0)
            {
                Skill improveMaxMp = SkillFactory.GetSkillTrust(Magician.IMPROVED_MAX_MP_INCREASE);
                recipe.addStartingSkillLevel(improveMaxMp, improveSp);
            }
        }

        return recipe;
    }

    private void giveEquipment(CharacterFactoryRecipe recipe, ItemInformationProvider ii, int equipid)
    {
        Item nEquip = ii.getEquipById(equipid);
        recipe.addStartingEquipment(nEquip);
    }

    private void giveItem(CharacterFactoryRecipe recipe, int itemid, int quantity, InventoryType itemType)
    {
        recipe.addStartingItem(itemid, quantity, itemType);
    }

    public override int createCharacter(int accountId, string name, int face, int hair, int skin, int gender, int improveSp)
    {
        return createNewCharacter(accountId, name, face, hair, skin, gender, createRecipe(Job.MAGICIAN, 30, MapId.ELLINIA, equips[gender], equips[2 + gender], equips[4], weapons[0], gender, improveSp));
    }
}
