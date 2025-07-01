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
using Application.Core.Game.Life;
using Application.Core.Channel;
using Microsoft.Extensions.Logging;
using server.maps;
using Application.Core.Channel.Services;

namespace scripting.reactor;


/**
 * @author Lerk
 */
public class ReactorScriptManager : AbstractScriptManager
{
    private Dictionary<int, List<DropEntry>> drops = new();

    public ReactorScriptManager(ILogger<AbstractScriptManager> logger, CommandExecutor commandExecutor, WorldChannel worldChannel) : base(logger, commandExecutor, worldChannel)
    {
        LoadAllReactorDrops();
    }

    public void onHit(IChannelClient c, Reactor reactor)
    {
        try
        {
            var iv = initializeInvocable(c, reactor);
            if (iv == null)
            {
                return;
            }

            iv.CallFunction("hit");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during onHit script for reactor: {ReactorId}", reactor.getId());
        }
    }

    public void act(IChannelClient c, Reactor reactor)
    {
        try
        {
            var iv = initializeInvocable(c, reactor);
            if (iv == null)
            {
                return;
            }

            iv.CallFunction("act");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during act script for reactor: {ReactorId}", reactor.getId());
        }
    }

    public List<DropEntry> getDrops(int reactorId)
    {
        return drops.GetValueOrDefault(reactorId) ?? [];
    }

    public void LoadAllReactorDrops()
    {
        drops = _channelServer.Service.RequestAllReactorDrops();
    }

    public void clearDrops()
    {
        drops.Clear();
    }

    public void touch(IChannelClient c, Reactor reactor)
    {
        touching(c, reactor, true);
    }

    public void untouch(IChannelClient c, Reactor reactor)
    {
        touching(c, reactor, false);
    }

    private void touching(IChannelClient c, Reactor reactor, bool touching)
    {
        string functionName = touching ? "touch" : "untouch";
        try
        {
            var iv = initializeInvocable(c, reactor);
            if (iv == null)
            {
                return;
            }

            iv.CallFunction(functionName);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during {ScriptFunction} script for reactor: {ReactorId}", functionName, reactor.getId());
        }
    }

    private IEngine? initializeInvocable(IChannelClient c, Reactor reactor)
    {
        var engine = getInvocableScriptEngine(GetReactorScriptPath(reactor.getId().ToString()), c);
        if (engine == null)
        {
            return null;
        }

        ReactorActionManager rm = new ReactorActionManager(c, reactor, engine);
        engine.AddHostedObject("rm", rm);

        return engine;
    }
}