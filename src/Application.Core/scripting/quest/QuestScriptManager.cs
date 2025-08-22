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


using Application.Core.Channel;
using Application.Core.Game.Commands;
using Application.Core.Scripting.Infrastructure;
using client;
using Microsoft.Extensions.Logging;
using server.quest;

namespace scripting.quest;



/**
 * @author RMZero213
 */
public class QuestScriptManager : AbstractScriptManager
{

    readonly EngineStorate<IChannelClient> _scripts = new EngineStorate<IChannelClient>();

    public QuestScriptManager(ILogger<AbstractScriptManager> logger, CommandExecutor commandExecutor, WorldChannel worldChannel, IEnumerable<IAddtionalRegistry> addtionalRegistries)
        : base(logger, commandExecutor, worldChannel, addtionalRegistries)
    {
    }

    private IEngine? getQuestScriptEngine(IChannelClient c, short questid)
    {
        var engine = getInvocableScriptEngine(GetQuestScriptPath(questid.ToString()), c);
        if (engine == null && GameUtils.isMedalQuest(questid))
        {
            engine = getInvocableScriptEngine(GetQuestScriptPath("medalQuest"), c);   // start generic medal quest
        }

        return engine;
    }

    public void start(IChannelClient c, short questid, int npc)
    {
        Quest quest = Quest.getInstance(questid);
        try
        {
            if (c.NPCConversationManager != null)
            {
                if (c.NPCConversationManager is not QuestActionManager)
                    c.NPCConversationManager.dispose();
                else
                    return;
            }

            if (c.canClickNPC())
            {
                c.NPCConversationManager = new QuestActionManager(c, questid, npc, true);

                if (!quest.hasScriptRequirement(false))
                {
                    // lack of scripted quest checks found thanks to Mali, Resinate
                    c.NPCConversationManager.dispose();
                    return;
                }

                var engine = getQuestScriptEngine(c, questid);
                if (engine == null)
                {
                    _logger.LogWarning("START Quest {QuestId} is uncoded.", questid);
                    c.NPCConversationManager.dispose();
                    return;
                }

                engine.AddHostedObject("qm", c.NPCConversationManager);

                _scripts[c] = engine;
                c.setClickedNPC();
                engine.CallFunction("start", (byte)1, (byte)0, 0);
            }
        }
        catch (Exception t)
        {
            _logger.LogError(t, "Error starting quest script: {QuestId}", questid);
            c.NPCConversationManager?.dispose();
        }
    }

    public void start(IChannelClient c, sbyte mode, sbyte type, int selection)
    {
        var iv = _scripts[c];
        if (iv != null)
        {
            try
            {
                c.setClickedNPC();
                iv.CallFunction("start", mode, type, selection);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error starting quest script: {QuestId}", getQM(c)?.getQuest());
                c.NPCConversationManager?.dispose();
            }
        }
    }

    public void end(IChannelClient c, short questid, int npc)
    {
        Quest quest = Quest.getInstance(questid);
        if (!c.OnlinedCharacter.getQuest(quest).getStatus().Equals(QuestStatus.Status.STARTED) || (!c.OnlinedCharacter.getMap().containsNPC(npc) && !quest.isAutoComplete()))
        {
            c.NPCConversationManager?.dispose();
            return;
        }
        try
        {
            if (c.NPCConversationManager != null)
            {
                if (c.NPCConversationManager is not QuestActionManager)
                    c.NPCConversationManager.dispose();
                else
                    return;
            }

            if (c.canClickNPC())
            {
                c.NPCConversationManager = new QuestActionManager(c, questid, npc, false);

                if (!quest.hasScriptRequirement(true))
                {
                    c.NPCConversationManager.dispose();
                    return;
                }

                var engine = getQuestScriptEngine(c, questid);
                if (engine == null)
                {
                    _logger.LogWarning("END Quest {QuestId} is uncoded.", questid);
                    c.NPCConversationManager.dispose();
                    return;
                }

                engine.AddHostedObject("qm", c.NPCConversationManager);

                _scripts[c] = engine;
                c.setClickedNPC();
                engine.CallFunction("end", (byte)1, (byte)0, 0);
            }
        }
        catch (Exception t)
        {
            _logger.LogError(t, "Error starting quest script: {QuestId}", questid);
            c.NPCConversationManager?.dispose();
        }
    }

    public void end(IChannelClient c, sbyte mode, sbyte type, int selection)
    {
        var iv = _scripts[c];
        if (iv != null)
        {
            try
            {
                c.setClickedNPC();
                iv.CallFunction("end", mode, type, selection);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error ending quest script: {QuestId}", getQM(c)?.getQuest());
                c.NPCConversationManager?.dispose();
            }
        }
    }

    public void raiseOpen(IChannelClient c, short questid, int npc)
    {
        try
        {
            if (c.NPCConversationManager != null)
            {
                if (c.NPCConversationManager is not QuestActionManager)
                    c.NPCConversationManager.dispose();
                else
                    return;
            }
            if (c.canClickNPC())
            {
                c.NPCConversationManager = new QuestActionManager(c, questid, npc, true);

                var engine = getQuestScriptEngine(c, questid);
                if (engine == null)
                {
                    _logger.LogError("RAISE Quest " + questid + " is uncoded.");
                    c.NPCConversationManager.dispose();
                    return;
                }

                engine.AddHostedObject("qm", c.NPCConversationManager);

                _scripts[c] = engine;
                c.setClickedNPC();
                engine.CallFunction("raiseOpen");
            }
        }
        catch (Exception t)
        {
            _logger.LogError(t, "Error during quest script raiseOpen for quest: {QuestId}", questid);
            c.NPCConversationManager?.dispose();
        }
    }

    public void dispose(QuestActionManager qm, IChannelClient c)
    {
        c.NPCConversationManager = null;
        _scripts.Remove(c);
        c.OnlinedCharacter.setNpcCooldown(c.CurrentServerContainer.getCurrentTime());
        resetContext(GetQuestScriptPath(qm.getQuest().ToString()), c);
        c.OnlinedCharacter.flushDelayedUpdateQuests();
    }

    public QuestActionManager? getQM(IChannelClient c)
    {
        return c.NPCConversationManager as QuestActionManager;
    }
}
