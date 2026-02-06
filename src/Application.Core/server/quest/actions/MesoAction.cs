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

namespace server.quest.actions;

/**
 * @author Tyler (Twdtwd)
 */
public class MesoAction : AbstractQuestAction
{
    int mesos;

    public MesoAction(Quest quest, int data) : base(QuestActionType.MESO, quest)
    {

        questID = quest.getId();
        mesos = data;
    }


    public override void run(Player chr, int? extSelection)
    {
        runAction(chr, mesos);
    }

    public static void runAction(Player chr, int gain)
    {
        if (gain < 0)
        {
            chr.GainMeso(gain, GainItemShow.ShowInChat);
        }
        else
        {
            var mesoGain = gain * chr.getMesoRate();
            if (YamlConfig.config.server.USE_QUEST_RATE)
            {
                mesoGain = gain * chr.getQuestMesoRate();
            }

            chr.GainMeso((int)mesoGain, GainItemShow.ShowInChat);
        }
    }
}
