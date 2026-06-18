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
using Application.Core.Game.Life;
using Microsoft.Extensions.Logging;
using server.maps;

namespace scripting.reactor;


/**
 * @author Lerk
 */
public class ReactorScriptManager : AbstractScriptManager
{

    public ReactorScriptManager(ILogger<AbstractScriptManager> logger, CommandExecutor commandExecutor, WorldChannel worldChannel, IEnumerable<IAddtionalRegistry> addtionalRegistries)
        : base(logger, commandExecutor, worldChannel, addtionalRegistries)
    {

    }

    public async Task onHit(IChannelClient c, Reactor reactor)
    {
        await c.CurrentServer.NodeService.PluginManager.ReactorHit(c, reactor);
    }

    public async Task act(IChannelClient c, Reactor reactor)
    {
        await c.CurrentServer.NodeService.PluginManager.ReactorAct(c, reactor);
    }

    public List<DropEntry> getDrops(int reactorId)
    {
        return _channelServer.NodeService.DataService.GetReactorDrops(reactorId);
    }

    public void clearDrops()
    {
        _channelServer.NodeService.DataService.ClearReactorDrops();
    }

    public async Task touch(IChannelClient c, Reactor reactor)
    {
        await c.CurrentServer.NodeService.PluginManager.ReactorTouch(c, reactor);
    }

    public async Task untouch(IChannelClient c, Reactor reactor)
    {
        await c.CurrentServer.NodeService.PluginManager.ReactorUntouch(c, reactor);
    }

}