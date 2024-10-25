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


using Application.Core.Game.Life;
using Application.Core.Scripting.Infrastructure;
using server.maps;

namespace scripting.reactor;


/**
 * @author Lerk
 */
public class ReactorScriptManager : AbstractScriptManager
{
    private static ReactorScriptManager instance = new ReactorScriptManager();

    private Dictionary<int, List<DropEntry>> drops = new();
    private ReactorScriptManager()
    {
        LoadAllReactorDrops();
    }

    public static ReactorScriptManager getInstance()
    {
        return instance;
    }

    public void onHit(IClient c, Reactor reactor)
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
            log.Error(e, "Error during onHit script for reactor: {ReactorId}", reactor.getId());
        }
    }

    public void act(IClient c, Reactor reactor)
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
            log.Error(e, "Error during act script for reactor: {ReactorId}", reactor.getId());
        }
    }

    public List<DropEntry> getDrops(int reactorId)
    {
        var ret = drops.GetValueOrDefault(reactorId);
        if (ret == null)
        {
            ret = new();
            try
            {
                using var dbContext = new DBContext();
                ret = dbContext.Reactordrops.Where(x => x.Reactorid == reactorId && x.Chance >= 0)
                    .Select(x => new { x.Itemid, x.Chance, x.Questid })
                    .ToList()
                    .Select(x => new DropEntry(x.Itemid, x.Chance, (short)x.Questid))
                    .ToList();
            }
            catch (Exception e)
            {
                log.Error(e, "Error getting drops for reactor: {ReactorId}", reactorId);
            }
            drops.AddOrUpdate(reactorId, ret);
        }
        return ret;
    }

    public void LoadAllReactorDrops()
    {
        using var dbContext = new DBContext();
        drops = dbContext.Reactordrops.Where(x => x.Chance >= 0)
            .Select(x => new { x.Itemid, x.Chance, x.Questid, x.Reactorid })
            .ToList()
            .GroupBy(x => x.Reactorid)
            .Select(x => new KeyValuePair<int, List<DropEntry>>(x.Key, x.Select(y => new DropEntry(y.Itemid, y.Chance, (short)y.Questid)).ToList()))
            .ToDictionary();
    }

    public void clearDrops()
    {
        drops.Clear();
    }

    public void touch(IClient c, Reactor reactor)
    {
        touching(c, reactor, true);
    }

    public void untouch(IClient c, Reactor reactor)
    {
        touching(c, reactor, false);
    }

    private void touching(IClient c, Reactor reactor, bool touching)
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
            log.Error(e, "Error during {ScriptFunction} script for reactor: {ReactorId}", functionName, reactor.getId());
        }
    }

    private IEngine? initializeInvocable(IClient c, Reactor reactor)
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