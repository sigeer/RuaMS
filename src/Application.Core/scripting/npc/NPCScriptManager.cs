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


using Application.Core.Game.Commands;
using Application.Core.Channel;
using Application.Core.Scripting.Infrastructure;
using Microsoft.Extensions.Logging;
using tools;
using static Application.Core.Channel.DataProviders.ItemInformationProvider;

namespace scripting.npc;

/**
 * @author Matze
 */
public class NPCScriptManager : AbstractScriptManager
{

    readonly EngineStorate<IChannelClient> _scripts = new EngineStorate<IChannelClient>();

    public NPCScriptManager(ILogger<AbstractScriptManager> logger, CommandExecutor commandExecutor, WorldChannel worldChannel) : base(logger, commandExecutor, worldChannel)
    {
    }

    public bool isNpcScriptAvailable(IChannelClient c, string fileName)
    {
        IEngine? engine = null;
        if (fileName != null)
        {
            engine = getInvocableScriptEngine(GetNpcScriptPath(fileName), c);
        }

        return engine != null;
    }

    public bool start(IChannelClient c, int npc, IPlayer? chr)
    {
        return start(c, npc, -1, chr);
    }

    public bool start(IChannelClient c, int npc, int oid, IPlayer? chr)
    {
        return start(c, npc, oid, null, chr);
    }

    public bool start(IChannelClient c, int npc, string? fileName, IPlayer? chr)
    {
        return start(c, npc, -1, fileName, chr);
    }

    public bool start(IChannelClient c, int npc, int oid, string? fileName, IPlayer? chr)
    {
        return start(c, npc, oid, fileName, chr, false, "cm");
    }

    public bool start(IChannelClient c, ScriptedItem scriptItem, IPlayer? chr)
    {
        return start(c, scriptItem.getNpc(), -1, scriptItem.getScript(), chr, true, "im");
    }

    public void start(string filename, IChannelClient c, int npc, List<IPlayer> chrs)
    {
        try
        {
            if (c.NPCConversationManager != null)
                c.NPCConversationManager.dispose();


            var scriptMeta = GetScriptMeta(GetNpcScriptPath(filename));
            var engine = getInvocableScriptEngine(scriptMeta, c);
            if (engine == null)
            {
                c.OnlinedCharacter.dropMessage(1, "NPC " + npc + " is uncoded.");
                return;
            }

            c.NPCConversationManager = new NPCConversationManager(c, npc, scriptMeta!, chrs, true);
            engine.AddHostedObject("cm", c.NPCConversationManager);
            _scripts[c] = engine;

            engine.CallFunction("start", chrs);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error starting NPC script: {ScriptName}, Npc: {Npc}", filename, npc);
            c.NPCConversationManager?.dispose();
        }
    }

    private bool start(IChannelClient c, int npc, int oid, string? fileName, IPlayer? chr, bool itemScript, string engineName)
    {
        try
        {
            if (c.NPCConversationManager != null)
                c.NPCConversationManager.dispose();

            if (c.canClickNPC())
            {
                ScriptMeta? scriptMeta = null;
                if (!itemScript)
                {
                    if (fileName != null)
                    {
                        scriptMeta = GetScriptMeta(GetNpcScriptPath(fileName)) ?? GetScriptMeta(GetSpecialScriptPath(fileName));
                    }
                }
                else
                {
                    if (fileName != null)
                    {
                        // thanks MiLin for drafting NPC-based item scripts
                        scriptMeta = GetScriptMeta(GetItemScriptPath(fileName));
                    }
                }
                if (scriptMeta == null)
                {
                    itemScript = false;
                    scriptMeta = GetScriptMeta(GetNpcScriptPath(npc.ToString()));
                }
                if (scriptMeta == null)
                {
                    return false;
                }

                var engine = getInvocableScriptEngine(scriptMeta, c);
                c.NPCConversationManager = new NPCConversationManager(c, npc, oid, scriptMeta, itemScript);
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
            _logger.LogError(e, "Error starting NPC script: {ScriptName}, Npc: {Npc}", fileName, npc);
            c.NPCConversationManager?.dispose();

            return false;
        }
    }

    public void action(IChannelClient c, sbyte mode, sbyte type, int selection)
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
                    _logger.LogError(t, "Error performing NPC script action for ScriptName: {ScriptName}, Npc: {Npc}", c.NPCConversationManager.ScriptMeta, c.NPCConversationManager.getNpc());
                    c.NPCConversationManager.dispose();
                }
            }
        }
    }

    public void dispose(NPCConversationManager cm)
    {
        var c = cm.getClient();
        c.OnlinedCharacter.setCS(false);
        c.OnlinedCharacter.setNpcCooldown(c.CurrentServerContainer.getCurrentTime());
        c.NPCConversationManager = null;
        _scripts.Remove(c);

        if (cm.ScriptMeta != null)
            resetContext(cm.ScriptMeta.ScriptFile, c);

        c.OnlinedCharacter.flushDelayedUpdateQuests();
    }
}
