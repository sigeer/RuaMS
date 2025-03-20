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


using Application.Core.Scripting.Infrastructure;
using client;
using constants.game;
using Org.BouncyCastle.Bcpg.Sig;
using server.quest;

namespace scripting.quest;



/**
 * @author RMZero213
 */
public class QuestScriptManager : AbstractScriptManager
{
    private static QuestScriptManager instance = new QuestScriptManager();

    readonly EngineStorate<IClient> _scripts = new EngineStorate<IClient>();

    public static QuestScriptManager getInstance()
    {
        return instance;
    }

    private IEngine? getQuestScriptEngine(IClient c, short questid)
    {
        var engine = getInvocableScriptEngine(GetQuestScriptPath(questid.ToString()), c);
        if (engine == null && GameConstants.isMedalQuest(questid))
        {
            engine = getInvocableScriptEngine(GetQuestScriptPath("medalQuest"), c);   // start generic medal quest
        }

        return engine;
    }

    public void start(IClient c, short questid, int npc)
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
                    log.Warning("START Quest {QuestId} is uncoded.", questid);
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
            log.Error(t, "Error starting quest script: {QuestId}", questid);
            c.NPCConversationManager?.dispose();
        }
    }

    public void start(IClient c, sbyte mode, sbyte type, int selection)
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
                log.Error(e, "Error starting quest script: {QuestId}", getQM(c)?.getQuest());
                c.NPCConversationManager?.dispose();
            }
        }
    }

    public void end(IClient c, short questid, int npc)
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
                    log.Warning("END Quest {QuestId} is uncoded.", questid);
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
            log.Error(t, "Error starting quest script: {QuestId}", questid);
            c.NPCConversationManager?.dispose();
        }
    }

    public void end(IClient c, sbyte mode, sbyte type, int selection)
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
                log.Error(e, "Error ending quest script: {QuestId}", getQM(c)?.getQuest());
                c.NPCConversationManager?.dispose();
            }
        }
    }

    public void raiseOpen(IClient c, short questid, int npc)
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
                    log.Error("RAISE Quest " + questid + " is uncoded.");
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
            log.Error(t, "Error during quest script raiseOpen for quest: {QuestId}", questid);
            c.NPCConversationManager?.dispose();
        }
    }

    public void dispose(QuestActionManager qm, IClient c)
    {
        c.NPCConversationManager = null;
        _scripts.Remove(c);
        c.OnlinedCharacter.setNpcCooldown(DateTimeOffset.Now.ToUnixTimeMilliseconds());
        resetContext(GetQuestScriptPath(qm.getQuest().ToString()), c);
        c.OnlinedCharacter.flushDelayedUpdateQuests();
    }

    public QuestActionManager? getQM(IClient c)
    {
        return c.NPCConversationManager as QuestActionManager;
    }
}
