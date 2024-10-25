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
using tools;
using static server.ItemInformationProvider;

namespace scripting.npc;

/**
 * @author Matze
 */
public class NPCScriptManager : AbstractScriptManager
{
    private static NPCScriptManager instance = new NPCScriptManager();

    private Dictionary<IClient, NPCConversationManager> cms = new();
    readonly EngineStorate<IClient> _scripts = new EngineStorate<IClient>();

    public static NPCScriptManager getInstance()
    {
        return instance;
    }

    public bool isNpcScriptAvailable(IClient c, string fileName)
    {
        IEngine? engine = null;
        if (fileName != null)
        {
            engine = getInvocableScriptEngine(GetNpcScriptPath(fileName), c);
        }

        return engine != null;
    }

    public bool start(IClient c, int npc, IPlayer? chr)
    {
        return start(c, npc, -1, chr);
    }

    public bool start(IClient c, int npc, int oid, IPlayer? chr)
    {
        return start(c, npc, oid, null, chr);
    }

    public bool start(IClient c, int npc, string? fileName, IPlayer? chr)
    {
        return start(c, npc, -1, fileName, chr);
    }

    public bool start(IClient c, int npc, int oid, string? fileName, IPlayer? chr)
    {
        return start(c, npc, oid, fileName, chr, false, "cm");
    }

    public bool start(IClient c, ScriptedItem scriptItem, IPlayer? chr)
    {
        return start(c, scriptItem.getNpc(), -1, scriptItem.getScript(), chr, true, "im");
    }

    public void start(string filename, IClient c, int npc, List<IPlayer> chrs)
    {
        try
        {
            NPCConversationManager cm = new NPCConversationManager(c, npc, chrs, true);
            cm.dispose();

            if (cms.ContainsKey(c))
            {
                return;
            }
            cms.AddOrUpdate(c, cm);
            var engine = getInvocableScriptEngine(GetNpcScriptPath(filename), c);

            if (engine == null)
            {
                c.OnlinedCharacter.dropMessage(1, "NPC " + npc + " is uncoded.");
                cm.dispose();
                return;
            }
            engine.AddHostedObject("cm", cm);
            _scripts[c] =  engine;
            try
            {
                engine.CallFunction("start", chrs);
            }
            catch (Exception nsme)
            {
                log.Error(nsme.ToString());
            }

        }
        catch (Exception e)
        {
            log.Error(e, "Error starting NPC script: {ScriptName}", npc);
            dispose(c);
        }
    }

    private bool start(IClient c, int npc, int oid, string? fileName, IPlayer? chr, bool itemScript, string engineName)
    {
        try
        {
            if (cms.ContainsKey(c))
            {
                dispose(c);
            }

            if (c.canClickNPC())
            {
                NPCConversationManager cm = new NPCConversationManager(c, npc, oid, fileName, itemScript);
                cms.AddOrUpdate(c, cm);
                IEngine? engine = null;
                if (!itemScript)
                {
                    if (fileName != null)
                    {
                        engine = getInvocableScriptEngine(GetNpcScriptPath(fileName), c);
                    }
                }
                else
                {
                    if (fileName != null)
                    {     // thanks MiLin for drafting NPC-based item scripts
                        engine = getInvocableScriptEngine(GetItemScriptPath(fileName), c);
                    }
                }
                if (engine == null)
                {
                    engine = getInvocableScriptEngine(GetNpcScriptPath(npc.ToString()), c);
                    cm.resetItemScript();
                }
                if (engine == null)
                {
                    dispose(c);
                    return false;
                }
                engine.AddHostedObject(engineName, cm);

                _scripts[c] = engine;
                c.setClickedNPC();
                try
                {
                    engine.CallFunction("start");
                }
                catch (Exception)
                {
                    try
                    {
                        engine.CallFunction("start", chr);
                    }
                    catch (Exception nsma)
                    {
                        log.Error(nsma.ToString());
                    }
                }
            }
            else
            {
                c.sendPacket(PacketCreator.enableActions());
            }
            return true;
        }
        catch (Exception e)
        {
            log.Error(e, "Error starting NPC script: {ScriptName}", npc);
            dispose(c);

            return false;
        }
    }

    public void action(IClient c, sbyte mode, sbyte type, int selection)
    {
        var iv = _scripts[c];
        if (iv != null)
        {
            try
            {
                c.setClickedNPC();
                iv.CallFunction("action", mode, type, selection);
            }
            catch (Exception t)
            {
                if (getCM(c) != null)
                {
                    log.Error(t, "Error performing NPC script action for npc: {ScriptName}", getCM(c)!.getNpc());
                }
                dispose(c);
            }
        }
    }

    public void dispose(NPCConversationManager cm)
    {
        IClient c = cm.getClient();
        c.OnlinedCharacter.setCS(false);
        c.OnlinedCharacter.setNpcCooldown(DateTimeOffset.Now.ToUnixTimeMilliseconds());
        cms.Remove(c);
        _scripts.Remove(c);

        string scriptFolder = (cm.isItemScript() ? "item" : "npc");
        if (cm.getScriptName() != null)
        {
            resetContext(scriptFolder + "/" + cm.getScriptName() + ".js", c);
        }
        else
        {
            resetContext(scriptFolder + "/" + cm.getNpc() + ".js", c);
        }

        c.OnlinedCharacter.flushDelayedUpdateQuests();
    }

    public void dispose(IClient c)
    {
        var cm = cms.GetValueOrDefault(c);
        if (cm != null)
        {
            dispose(cm);
        }
    }

    public NPCConversationManager? getCM(IClient c)
    {
        return cms.GetValueOrDefault(c);
    }

}
