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

namespace Application.Core.Server.quest.actions;

/**
 * @author Tyler (Twdtwd)
 */
public class PetSkillAction : AbstractQuestAction
{
    short flag;

    public PetSkillAction(Quest quest, int data) : base(QuestActionType.PETSKILL, quest)
    {

        questID = quest.getId();
        flag = (short)data;
    }


    public override Task<bool> check(Player chr, int? extSelection)
    {
        var bossPet = chr.GetPetByIndex(0);
        if (bossPet == null)
        {
            return Task.FromResult(false);
        }

        return Task.FromResult((bossPet.PetItem.PetSkill & flag) != flag);
    }

    public override async Task run(Player chr, int? extSelection)
    {
        var bossPet = chr.GetPetByIndex(0);
        if (bossPet != null)
        {
            bossPet.PetItem.PetSkill |= flag;
            await chr.forceUpdateItem(bossPet.PetItem);
        }
    }
}
