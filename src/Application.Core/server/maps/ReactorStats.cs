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

namespace server.maps;


/**
 * @author Lerk
 * @author Ronan
 */
public class ReactorStats
{
    private Point tl;
    private Point br;
    private Dictionary<sbyte, List<StateData>> stateInfo = new();
    private Dictionary<sbyte, int> timeoutInfo = new();

    public void setTL(Point tl)
    {
        this.tl = tl;
    }

    public void setBR(Point br)
    {
        this.br = br;
    }

    public Point getTL()
    {
        return tl;
    }

    public Point getBR()
    {
        return br;
    }

    public void addState(sbyte state, List<StateData> data, int timeOut)
    {
        stateInfo.AddOrUpdate(state, data);
        if (timeOut > -1)
            timeoutInfo.AddOrUpdate(state, timeOut);
    }

    public void addState(sbyte state, int type, ItemQuantity? reactItem, sbyte nextState, int timeOut, byte canTouch)
    {
        stateInfo.AddOrUpdate(state, [new StateData(type, reactItem, null, nextState)]);
    }

    public int getTimeout(sbyte state)
    {
        return timeoutInfo.GetValueOrDefault(state, -1);
    }

    public sbyte getTimeoutState(sbyte state)
    {
        var value = stateInfo.GetValueOrDefault(state);
        return value == null ? throw new BusinessException() : value.Last().getNextState();
    }

    public byte getStateSize(sbyte state)
    {
        return (byte)stateInfo.GetValueOrDefault(state)!.Count;
    }

    public sbyte getNextState(sbyte state, byte index)
    {
        return stateInfo.GetValueOrDefault(state)?.ElementAtOrDefault(index)?.getNextState() ?? -1;
    }

    public List<int>? getActiveSkills(sbyte state, byte index)
    {
        return stateInfo.GetValueOrDefault(state)?.ElementAtOrDefault(index)?.getActiveSkills();
    }

    public int getType(sbyte state)
    {
        return stateInfo.GetValueOrDefault(state)?.ElementAtOrDefault(0)?.getType() ?? -1;
    }

    public ItemQuantity? getReactItem(sbyte state, sbyte index)
    {
        return stateInfo.GetValueOrDefault(state)?.ElementAtOrDefault(index)?.getReactItem();
    }


    public class StateData
    {
        private int type;
        private ItemQuantity? reactItem;
        private List<int>? activeSkills;
        private sbyte nextState;

        public StateData(int type, ItemQuantity? reactItem, List<int>? activeSkills, sbyte nextState)
        {
            this.type = type;
            this.reactItem = reactItem;
            this.activeSkills = activeSkills;
            this.nextState = nextState;
        }

        public int getType()
        {
            return type;
        }

        public sbyte getNextState()
        {
            return nextState;
        }

        public ItemQuantity? getReactItem()
        {
            return reactItem;
        }

        public List<int>? getActiveSkills()
        {
            return activeSkills;
        }
    }
}
