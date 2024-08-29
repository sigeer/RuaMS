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


using client;
using constants.game;
using Microsoft.ClearScript.V8;
using server.quest;

namespace scripting.quest;



/**
 * @author RMZero213
 */
public class QuestScriptManager : AbstractScriptManager
{
    private static QuestScriptManager instance = new QuestScriptManager();

    private Dictionary<IClient, QuestActionManager> qms = new();
    private Dictionary<IClient, V8ScriptEngine> scripts = new();

    public static QuestScriptManager getInstance()
    {
        return instance;
    }

    private V8ScriptEngine getQuestScriptEngine(IClient c, short questid)
    {
        var engine = getInvocableScriptEngine("quest/" + questid + ".js", c);
        if (engine == null && GameConstants.isMedalQuest(questid))
        {
            engine = getInvocableScriptEngine("quest/medalQuest.js", c);   // start generic medal quest
        }

        return engine;
    }

    public void start(IClient c, short questid, int npc)
    {
        Quest quest = Quest.getInstance(questid);
        try
        {
            QuestActionManager qm = new QuestActionManager(c, questid, npc, true);
            if (qms.ContainsKey(c))
            {
                return;
            }
            if (c.canClickNPC())
            {
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

                engine.AddHostObject("qm", qm);

                scripts.AddOrUpdate(c, engine);
                c.setClickedNPC();
                engine.InvokeSync("start", (byte)1, (byte)0, 0);
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
        var iv = scripts.GetValueOrDefault(c);
        if (iv != null)
        {
            try
            {
                c.setClickedNPC();
                iv.InvokeSync("start", mode, type, selection);
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
            var qm = new QuestActionManager(c, questid, npc, false);
            if (qms.ContainsKey(c))
            {
                return;
            }
            if (c.canClickNPC())
            {
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

                engine.AddHostObject("qm", qm);

                scripts.AddOrUpdate(c, engine);
                c.setClickedNPC();
                engine.InvokeSync("end", (byte)1, (byte)0, 0);
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
        var iv = scripts.GetValueOrDefault(c);
        if (iv != null)
        {
            try
            {
                c.setClickedNPC();
                iv.InvokeSync("end", mode, type, selection);
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

                engine.AddHostObject("qm", qm);

                scripts.AddOrUpdate(c, engine);
                c.setClickedNPC();
                engine.InvokeSync("raiseOpen");
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
        scripts.Remove(c);
        c.OnlinedCharacter.setNpcCooldown(DateTimeOffset.Now.ToUnixTimeMilliseconds());
        resetContext("quest/" + qm.getQuest() + ".js", c);
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
        scripts.Clear();
        qms.Clear();
    }
}
