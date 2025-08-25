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
using Microsoft.Extensions.Logging;
using server.maps;

namespace scripting.portal;



public class PortalScriptManager : AbstractScriptManager
{
    readonly EngineStorage _scripts = new EngineStorage();

    public PortalScriptManager(ILogger<AbstractScriptManager> logger, CommandExecutor commandExecutor, WorldChannel worldChannel, IEnumerable<IAddtionalRegistry> addtionalRegistries)
        : base(logger, commandExecutor, worldChannel, addtionalRegistries)
    {
    }

    private IEngine? getPortalScript(string? scriptName)
    {
        if (string.IsNullOrEmpty(scriptName))
            return null;

        var scriptPath = GetPortalScriptPath(scriptName);
        var script = _scripts[scriptPath.CacheKey];
        if (script != null)
        {
            return script;
        }

        var engine = getInvocableScriptEngine(scriptPath);
        if (engine == null)
        {
            return null;
        }

        _scripts[scriptPath.CacheKey] = engine;
        return script;
    }

    public bool executePortalScript(Portal portal, IChannelClient c)
    {
        try
        {
            var script = getPortalScript(portal.getScriptName());
            if (script != null)
            {
                _logger.LogDebug("Portal script {ScriptName}", portal.getScriptName());
                return script.CallFunction("enter", new PortalPlayerInteraction(c, portal)).ToObject<bool>();
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Portal script error in: {ScriptName}", portal.getScriptName());
        }
        return false;
    }

    public void reloadPortalScripts()
    {
        _scripts.Clear();
    }
}