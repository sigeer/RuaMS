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
            if (c.NPCConversationManager != null)
                c.NPCConversationManager.dispose();

            c.NPCConversationManager = new NPCConversationManager(c, npc, chrs, true);
            var engine = getInvocableScriptEngine(GetNpcScriptPath(filename), c);

            if (engine == null)
            {
                c.OnlinedCharacter.dropMessage(1, "NPC " + npc + " is uncoded.");
                c.NPCConversationManager.dispose();
                return;
            }
            engine.AddHostedObject("cm", c.NPCConversationManager);
            _scripts[c] = engine;

            engine.CallFunction("start", chrs);
        }
        catch (Exception e)
        {
            log.Error(e, "Error starting NPC script: {ScriptName}, Npc: {Npc}", filename, npc);
            c.NPCConversationManager?.dispose();
        }
    }

    private bool start(IClient c, int npc, int oid, string? fileName, IPlayer? chr, bool itemScript, string engineName)
    {
        try
        {
            if (c.NPCConversationManager != null)
                c.NPCConversationManager.dispose();

            if (c.canClickNPC())
            {
                c.NPCConversationManager = new NPCConversationManager(c, npc, oid, fileName, itemScript);
                IEngine? engine = null;
                if (!itemScript)
                {
                    if (fileName != null)
                    {
                        engine = getInvocableScriptEngine(GetNpcScriptPath(fileName), c) ?? getInvocableScriptEngine(GetSpecialScriptPath(fileName), c);
                    }
                }
                else
                {
                    if (fileName != null)
                    {
                        // thanks MiLin for drafting NPC-based item scripts
                        engine = getInvocableScriptEngine(GetItemScriptPath(fileName), c);
                    }
                }
                if (engine == null)
                {
                    engine = getInvocableScriptEngine(GetNpcScriptPath(npc.ToString()), c);
                    c.NPCConversationManager.resetItemScript();
                }
                if (engine == null)
                {
                    c.NPCConversationManager.dispose();
                    return false;
                }
                engine.AddHostedObject(engineName, c.NPCConversationManager);

                _scripts[c] = engine;
                c.setClickedNPC();

                engine.CallFunction("start", chr);
            }
            else
            {
                c.sendPacket(PacketCreator.enableActions());
            }
            return true;
        }
        catch (Exception e)
        {
            log.Error(e, "Error starting NPC script: {ScriptName}, Npc: {Npc}", fileName, npc);
            c.NPCConversationManager?.dispose();

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

                var nextLevelContext = c.NPCConversationManager?.NextLevelContext;
                if (nextLevelContext?.LevelType != null)
                {
                    if (nextLevelContext.TryGetInvokeFunction(c, mode, type, selection, out var function) && function != null)
                    {
                        iv.CallFunction(function.Name, function.Params);
                    }
                    else
                    {
                        c.NPCConversationManager?.dispose();
                        return;
                    }
                }
                else
                    iv.CallFunction("action", mode, type, selection);
            }
            catch (Exception t)
            {
                if (c.NPCConversationManager != null)
                {
                    log.Error(t, "Error performing NPC script action for ScriptName: {ScriptName}, Npc: {Npc}", c.NPCConversationManager.getScriptName(), c.NPCConversationManager.getNpc());
                    c.NPCConversationManager.dispose();
                }
            }
        }
    }

    public void dispose(NPCConversationManager cm)
    {
        IClient c = cm.getClient();
        c.OnlinedCharacter.setCS(false);
        c.OnlinedCharacter.setNpcCooldown(DateTimeOffset.Now.ToUnixTimeMilliseconds());
        c.NPCConversationManager = null;
        _scripts.Remove(c);

        string scriptFolder = (cm.isItemScript() ? "item" : "npc");
        if (cm.getScriptName() != null)
        {
            if (scriptFolder == "npc")
                resetContext(GetSpecialScriptPath(cm.getScriptName()!), c);
            resetContext(GetScriptPath(scriptFolder, cm.getScriptName()!), c);
        }
        else
        {
            if (scriptFolder == "npc")
                resetContext(GetSpecialScriptPath(cm.getNpc().ToString()), c);
            resetContext(GetScriptPath(scriptFolder, cm.getNpc().ToString()), c);
        }

        c.OnlinedCharacter.flushDelayedUpdateQuests();
    }
}
