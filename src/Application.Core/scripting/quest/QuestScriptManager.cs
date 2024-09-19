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
using server.quest;

namespace scripting.quest;



/**
 * @author RMZero213
 */
public class QuestScriptManager : AbstractScriptManager
{
    private static QuestScriptManager instance = new QuestScriptManager();

    private Dictionary<IClient, QuestActionManager> qms = new();
    readonly EngineStorate<IClient> _scripts = new EngineStorate<IClient>();

    public static QuestScriptManager getInstance()
    {
        return instance;
    }

    private IEngine getQuestScriptEngine(IClient c, short questid)
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
            if (qms.ContainsKey(c))
            {
                return;
            }

            if (c.canClickNPC())
            {
                QuestActionManager qm = new QuestActionManager(c, questid, npc, true);
                qms.AddOrUpdate(c, qm);

                if (!quest.hasScriptRequirement(false))
                {   // lack of scripted quest checks found thanks to Mali, Resinate
                    qm.dispose();
                    return;
                }

                var engine = getQuestScriptEngine(c, questid);
                if (engine == null)
                {
                    log.Warning("START Quest {QuestId} is uncoded.", questid);
                    qm.dispose();
                    return;
                }

                engine.AddHostedObject("qm", qm);

                _scripts[c] = engine;
                c.setClickedNPC();
                engine.CallFunction("start", (byte)1, (byte)0, 0);
            }
        }
        catch (Exception t)
        {
            log.Error(t, "Error starting quest script: {QuestId}", questid);
            dispose(c);
        }
    }

    public void start(IClient c, byte mode, byte type, int selection)
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
                dispose(c);
            }
        }
    }

    public void end(IClient c, short questid, int npc)
    {
        Quest quest = Quest.getInstance(questid);
        if (!c.OnlinedCharacter.getQuest(quest).getStatus().Equals(QuestStatus.Status.STARTED) || (!c.OnlinedCharacter.getMap().containsNPC(npc) && !quest.isAutoComplete()))
        {
            dispose(c);
            return;
        }
        try
        {
            if (qms.ContainsKey(c))
            {
                return;
            }

            if (c.canClickNPC())
            {
                var qm = new QuestActionManager(c, questid, npc, false);
                qms.AddOrUpdate(c, qm);

                if (!quest.hasScriptRequirement(true))
                {
                    qm.dispose();
                    return;
                }

                var engine = getQuestScriptEngine(c, questid);
                if (engine == null)
                {
                    log.Warning("END Quest {QuestId} is uncoded.", questid);
                    qm.dispose();
                    return;
                }

                engine.AddHostedObject("qm", qm);

                _scripts[c] = engine;
                c.setClickedNPC();
                engine.CallFunction("end", (byte)1, (byte)0, 0);
            }
        }
        catch (Exception t)
        {
            log.Error(t, "Error starting quest script: {QuestId}", questid);
            dispose(c);
        }
    }

    public void end(IClient c, byte mode, byte type, int selection)
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
                dispose(c);
            }
        }
    }

    public void raiseOpen(IClient c, short questid, int npc)
    {
        try
        {
            var qm = new QuestActionManager(c, questid, npc, true);
            if (qms.ContainsKey(c))
            {
                return;
            }
            if (c.canClickNPC())
            {
                qms.AddOrUpdate(c, qm);

                var engine = getQuestScriptEngine(c, questid);
                if (engine == null)
                {
                    //FilePrinter.printError(FilePrinter.QUEST_UNCODED, "RAISE Quest " + questid + " is uncoded.");
                    qm.dispose();
                    return;
                }

                engine.AddHostedObject("qm", qm);

                _scripts[c] = engine;
                c.setClickedNPC();
                engine.CallFunction("raiseOpen");
            }
        }
        catch (Exception t)
        {
            log.Error(t, "Error during quest script raiseOpen for quest: {QuestId}", questid);
            dispose(c);
        }
    }

    public void dispose(QuestActionManager qm, IClient c)
    {
        qms.Remove(c);
        _scripts.Remove(c);
        c.OnlinedCharacter.setNpcCooldown(DateTimeOffset.Now.ToUnixTimeMilliseconds());
        resetContext(GetQuestScriptPath(qm.getQuest().ToString()), c);
        c.OnlinedCharacter.flushDelayedUpdateQuests();
    }

    public void dispose(IClient c)
    {
        var qm = qms.GetValueOrDefault(c);
        if (qm != null)
        {
            dispose(qm, c);
        }
    }

    public QuestActionManager? getQM(IClient c)
    {
        return qms.GetValueOrDefault(c);
    }

    public void reloadQuestScripts()
    {
        _scripts.Clear();
        qms.Clear();
    }
}
