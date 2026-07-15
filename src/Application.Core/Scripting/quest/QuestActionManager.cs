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


using Application.Core.Channel.DataProviders;
using scripting.npc;
using server.quest;
using server.quest.actions;

namespace scripting.quest;

/**
 * @author RMZero213
 */
public class QuestActionManager : NPCConversationManager
{
    private bool start; // this is if the script in question is start or end
    private int quest;

    public QuestActionManager(IChannelClient c, int quest, int npc, bool start) : base(c, npc)
    {
        this.quest = quest;
        this.start = start;
    }

    public int getQuest()
    {
        return quest;
    }

    public bool isStart()
    {
        return start;
    }

    public async Task<bool> forceStartQuest()
    {
        return await forceStartQuest(quest);
    }

    public async Task<bool> forceCompleteQuest()
    {
        return await forceCompleteQuest(quest);
    }

    // For compatibility with some older scripts...
    public async Task startQuest()
    {
        await forceStartQuest();
    }

    // For compatibility with some older scripts...
    public async Task completeQuest()
    {
        await forceCompleteQuest();
    }

    public async Task CompleteQuestN(int? selection = null)
    {
        await Quest.getInstance(quest).complete(c.OnlinedCharacter, npc, selection);
    }

    public override async Task gainExp(int gain)
    {
        await ExpAction.runAction(getPlayer(), gain);
    }

    public override async Task gainMeso(int gain)
    {
        await MesoAction.runAction(getPlayer(), gain);
    }

    public string? getMedalName()
    {
        // usable only for medal quests (id 299XX)
        return c.CurrentCulture.GetItemName(QuestFactory.Instance.GetMedalRequirement((short)quest));
    }

    public async Task<bool> CanCompleteQuest()
    {
        return await QuestFactory.Instance.GetInstance(quest).canComplete(getPlayer(), npc);
    }
}
