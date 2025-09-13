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


using Application.Templates.Providers;
using Application.Templates.XmlWzReader.Provider;
using static server.maps.ReactorStats;

namespace server.maps;


public class ReactorFactory
{
    private static ReactorProvider data = ProviderFactory.GetProvider<ReactorProvider>();
    private static Dictionary<int, ReactorStats> reactorStats = new();

    public static ReactorStats getReactorS(int rid)
    {
        if (reactorStats.TryGetValue(rid, out var stats))
            return stats;

        stats = new ReactorStats();
        var reactorData = data.GetItem(rid);
        if (reactorData == null)
        {
            return stats;
        }

        bool areaSet = false;
        bool foundState = false;
        foreach (var stateInfo in reactorData.StateInfoList)
        {
            if (stateInfo.EventInfos.Length == 0)
                stats.addState((sbyte)stateInfo.State, 999, null, (sbyte)(foundState ? -1 : (stateInfo.State + 1)), 0);
            else
            {
                var evt = stateInfo.EventInfos[0];
                ItemQuantity? reactItem = null;
                int type = evt.EventType;
                if (type == 100)
                {
                    //reactor waits for item
                    reactItem = new(evt.Int0Value, evt.Int1Value);
                    if (!areaSet)
                    {
                        //only set area of effect for item-triggered reactors once
                        stats.setTL(evt.Lt);
                        stats.setBR(evt.Rb);
                        areaSet = true;
                    }
                }
                foundState = true;
                stats.addState((sbyte)stateInfo.State,
                    type,
                    reactItem,
                    (sbyte)evt.NextState,
                    stateInfo.TimeOut);
            }
        }
        return stats;
    }


    public static ReactorStats getReactor(int rid)
    {
        var stats = reactorStats.GetValueOrDefault(rid);
        if (stats == null)
        {
            stats = new ReactorStats();
            var reactorData = data.GetItem(rid);
            if (reactorData == null)
                return stats;

            bool areaSet = false;
            bool loadArea = reactorData.ActivateByTouch;
            foreach (var item in reactorData.StateInfoList)
            {
                List<StateData> statedatas = new();
                foreach (var evt in item.EventInfos)
                {
                    ItemQuantity? reactItem = null;
                    if (evt.EventType == 100)
                    {
                        //reactor waits for item
                        reactItem = new(evt.Int0Value, evt.Int1Value);
                        if (!areaSet || loadArea)
                        {
                            //only set area of effect for item-triggered reactors once
                            stats.setTL(evt.Lt);
                            stats.setBR(evt.Rb);
                            areaSet = true;
                        }
                    }
                    statedatas.Add(new StateData(evt.EventType, reactItem, evt.ActiveSkillId.ToList(), (sbyte)evt.NextState));
                }
                stats.addState((sbyte)item.State, statedatas, item.TimeOut);
            }

            reactorStats[rid] = stats;
        }
        return stats;
    }
}
