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


using tools;
using static server.maps.ReactorStats;

namespace server.maps;


public class ReactorFactory
{
    private static DataProvider data = DataProviderFactory.getDataProvider(WZFiles.REACTOR);
    private static Dictionary<int, ReactorStats> reactorStats = new();

    public static ReactorStats getReactorS(int rid)
    {
        var stats = reactorStats.GetValueOrDefault(rid);
        if (stats == null)
        {
            int infoId = rid;
            var reactorData = data.getData(StringUtil.getLeftPaddedStr(infoId + ".img", '0', 11));
            var link = reactorData.getChildByPath("info/link");
            if (link != null)
            {
                infoId = DataTool.getIntConvert("info/link", reactorData);
                stats = reactorStats.GetValueOrDefault(infoId);
            }
            if (stats == null)
            {
                stats = new ReactorStats();
                reactorData = data.getData(StringUtil.getLeftPaddedStr(infoId + ".img", '0', 11));
                if (reactorData == null)
                {
                    return stats;
                }
                bool canTouch = DataTool.getInt("info/activateByTouch", reactorData, 0) > 0;
                bool areaSet = false;
                bool foundState = false;
                for (sbyte i = 0; true; i++)
                {
                    var reactorD = reactorData.getChildByPath(i.ToString());
                    if (reactorD == null)
                    {
                        break;
                    }
                    var reactorInfoData_ = reactorD.getChildByPath("event");
                    if (reactorInfoData_ != null && reactorInfoData_.getChildByPath("0") != null)
                    {
                        var reactorInfoData = reactorInfoData_.getChildByPath("0");
                        ItemQuantity? reactItem = null;
                        int type = DataTool.getIntConvert("type", reactorInfoData);
                        if (type == 100)
                        {
                            //reactor waits for item
                            reactItem = new(DataTool.getIntConvert("0", reactorInfoData), DataTool.getIntConvert("1", reactorInfoData, 1));
                            if (!areaSet)
                            {
                                //only set area of effect for item-triggered reactors once
                                stats.setTL(DataTool.getPoint("lt", reactorInfoData) ?? Point.Empty);
                                stats.setBR(DataTool.getPoint("rb", reactorInfoData) ?? Point.Empty);
                                areaSet = true;
                            }
                        }
                        foundState = true;
                        stats.addState(i,
                            type,
                            reactItem,
                            (sbyte)DataTool.getIntConvert("state", reactorInfoData),
                            DataTool.getIntConvert("timeOut", reactorInfoData_, -1),
                            (byte)(canTouch ? 2 : (DataTool.getIntConvert("2", reactorInfoData, 0) > 0 || reactorInfoData?.getChildByPath("clickArea") != null || type == 9 ? 1 : 0)));
                    }
                    else
                    {
                        stats.addState(i, 999, null, (sbyte)(foundState ? -1 : (i + 1)), 0, 0);
                    }
                }
                reactorStats.AddOrUpdate(infoId, stats);
                if (rid != infoId)
                {
                    reactorStats.AddOrUpdate(rid, stats);
                }
            }
            else
            { // stats exist at infoId but not rid; add to map
                reactorStats.AddOrUpdate(rid, stats);
            }
        }
        return stats;
    }

    public static ReactorStats getReactor(int rid)
    {
        var stats = reactorStats.GetValueOrDefault(rid);
        if (stats == null)
        {
            int infoId = rid;
            var reactorData = data.getData(StringUtil.getLeftPaddedStr(infoId + ".img", '0', 11));
            var link = reactorData.getChildByPath("info/link");
            if (link != null)
            {
                infoId = DataTool.getIntConvert("info/link", reactorData);
                stats = reactorStats.GetValueOrDefault(infoId);
            }
            var activateOnTouch = reactorData.getChildByPath("info/activateByTouch");
            bool loadArea = false;
            if (activateOnTouch != null)
            {
                loadArea = DataTool.getInt("info/activateByTouch", reactorData, 0) != 0;
            }
            if (stats == null)
            {
                reactorData = data.getData(StringUtil.getLeftPaddedStr(infoId + ".img", '0', 11));
                var reactorInfoData = reactorData.getChildByPath("0");
                stats = new ReactorStats();
                List<StateData> statedatas = new();
                if (reactorInfoData != null)
                {
                    bool areaSet = false;
                    sbyte i = 0;
                    while (reactorInfoData != null)
                    {
                        var eventData = reactorInfoData.getChildByPath("event");
                        if (eventData != null)
                        {
                            int timeOut = -1;

                            foreach (Data fknexon in eventData.getChildren())
                            {
                                var fknexonName = fknexon.getName();
                                if (fknexonName != null && fknexonName.Equals("timeOut", StringComparison.OrdinalIgnoreCase))
                                {
                                    timeOut = DataTool.getInt(fknexon);
                                }
                                else
                                {
                                    ItemQuantity? reactItem = null;
                                    int type = DataTool.getIntConvert("type", fknexon);
                                    if (type == 100)
                                    {
                                        //reactor waits for item
                                        reactItem = new(DataTool.getIntConvert("0", fknexon), DataTool.getIntConvert("1", fknexon));
                                        if (!areaSet || loadArea)
                                        {
                                            //only set area of effect for item-triggered reactors once
                                            stats.setTL(DataTool.getPoint("lt", fknexon) ?? Point.Empty);
                                            stats.setBR(DataTool.getPoint("rb", fknexon) ?? Point.Empty);
                                            areaSet = true;
                                        }
                                    }
                                    var activeSkillID = fknexon.getChildByPath("activeSkillID");
                                    List<int>? skillids = null;
                                    if (activeSkillID != null)
                                    {
                                        skillids = new();
                                        foreach (Data skill in activeSkillID.getChildren())
                                        {
                                            skillids.Add(DataTool.getInt(skill));
                                        }
                                    }
                                    sbyte nextState = (sbyte)DataTool.getIntConvert("state", fknexon);
                                    statedatas.Add(new StateData(type, reactItem, skillids, nextState));
                                }
                            }
                            stats.addState(i, statedatas, timeOut);
                        }
                        i++;
                        reactorInfoData = reactorData.getChildByPath(i.ToString());
                        statedatas = new();
                    }
                }
                else //sit there and look pretty; likely a reactor such as Zakum/Papulatus doors that shows if player can enter
                {
                    statedatas.Add(new StateData(999, null, null, 0));
                    stats.addState(0, statedatas, -1);
                }
                reactorStats.AddOrUpdate(infoId, stats);
                if (rid != infoId)
                {
                    reactorStats.AddOrUpdate(rid, stats);
                }
            }
            else // stats exist at infoId but not rid; add to map
            {
                reactorStats.AddOrUpdate(rid, stats);
            }
        }
        return stats;
    }
}
