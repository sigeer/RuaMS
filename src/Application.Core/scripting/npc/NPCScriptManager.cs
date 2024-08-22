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
using Microsoft.ClearScript.V8;
using net.server.world;
using tools;
using static server.ItemInformationProvider;

namespace scripting.npc;

/**
 * @author Matze
 */
public class NPCScriptManager : AbstractScriptManager
{
    private static NPCScriptManager instance = new NPCScriptManager();

    private Dictionary<Client, NPCConversationManager> cms = new();
    private Dictionary<Client, V8ScriptEngine> scripts = new();

    public static NPCScriptManager getInstance()
    {
        return instance;
    }

    public bool isNpcScriptAvailable(Client c, string fileName)
    {
        V8ScriptEngine? engine = null;
        if (fileName != null)
        {
            engine = getInvocableScriptEngine("npc/" + fileName + ".js", c);
        }

        return engine != null;
    }

    public bool start(Client c, int npc, Character chr)
    {
        return start(c, npc, -1, chr);
    }

    public bool start(Client c, int npc, int oid, Character chr)
    {
        return start(c, npc, oid, null, chr);
    }

    public bool start(Client c, int npc, string fileName, Character chr)
    {
        return start(c, npc, -1, fileName, chr);
    }

    public bool start(Client c, int npc, int oid, string fileName, Character chr)
    {
        return start(c, npc, oid, fileName, chr, false, "cm");
    }

    public bool start(Client c, ScriptedItem scriptItem, Character chr)
    {
        return start(c, scriptItem.getNpc(), -1, scriptItem.getScript(), chr, true, "im");
    }

    public void start(string filename, Client c, int npc, List<PartyCharacter> chrs)
    {
        try
        {
            NPCConversationManager cm = new NPCConversationManager(c, npc, chrs, true);
            cm.dispose();
            if (cms.ContainsKey(c))
            {
                return;
            }
            cms.Add(c, cm);
            V8ScriptEngine engine = getInvocableScriptEngine("npc/" + filename + ".js", c);

            if (engine == null)
            {
                c.getPlayer().dropMessage(1, "NPC " + npc + " is uncoded.");
                cm.dispose();
                return;
            }
            engine.AddHostObject("cm", cm);
            scripts.AddOrUpdate(c, engine);
            try
            {
                engine.InvokeSync("start", chrs);
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

    private bool start(Client c, int npc, int oid, string fileName, Character chr, bool itemScript, string engineName)
    {
        try
        {
            NPCConversationManager cm = new NPCConversationManager(c, npc, oid, fileName, itemScript);
            if (cms.ContainsKey(c))
            {
                dispose(c);
            }
            if (c.canClickNPC())
            {
                cms.Add(c, cm);
                V8ScriptEngine? engine = null;
                if (!itemScript)
                {
                    if (fileName != null)
                    {
                        engine = getInvocableScriptEngine("npc/" + fileName + ".js", c);
                    }
                }
                else
                {
                    if (fileName != null)
                    {     // thanks MiLin for drafting NPC-based item scripts
                        engine = getInvocableScriptEngine("item/" + fileName + ".js", c);
                    }
                }
                if (engine == null)
                {
                    engine = getInvocableScriptEngine("npc/" + npc + ".js", c);
                    cm.resetItemScript();
                }
                if (engine == null)
                {
                    dispose(c);
                    return false;
                }
                engine.AddHostObject(engineName, cm);

                scripts.AddOrUpdate(c, engine);
                c.setClickedNPC();
                try
                {
                    engine.InvokeSync("start");
                }
                catch (Exception nsme)
                {
                    try
                    {
                        engine.InvokeSync("start", chr);
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

    public void action(Client c, byte mode, byte type, int selection)
    {
        var iv = scripts.GetValueOrDefault(c);
        if (iv != null)
        {
            try
            {
                c.setClickedNPC();
                iv.InvokeSync("action", mode, type, selection);
            }
            catch (Exception t)
            {
                if (getCM(c) != null)
                {
                    log.Error(t, "Error performing NPC script action for npc: {ScriptName}", getCM(c).getNpc());
                }
                dispose(c);
            }
        }
    }

    public void dispose(NPCConversationManager cm)
    {
        Client c = cm.getClient();
        c.getPlayer().setCS(false);
        c.getPlayer().setNpcCooldown(DateTimeOffset.Now.ToUnixTimeMilliseconds());
        cms.Remove(c);
        scripts.Remove(c);

        string scriptFolder = (cm.isItemScript() ? "item" : "npc");
        if (cm.getScriptName() != null)
        {
            resetContext(scriptFolder + "/" + cm.getScriptName() + ".js", c);
        }
        else
        {
            resetContext(scriptFolder + "/" + cm.getNpc() + ".js", c);
        }

        c.getPlayer().flushDelayedUpdateQuests();
    }

    public void dispose(Client c)
    {
        var cm = cms.GetValueOrDefault(c);
        if (cm != null)
        {
            dispose(cm);
        }
    }

    public NPCConversationManager? getCM(Client c)
    {
        return cms.GetValueOrDefault(c);
    }

}
