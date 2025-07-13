/*
    This file is part of the HeavenMS MapleStory Server
    Copyleft (L) 2016 - 2019 RonanLana

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


using Application.Core.Game.Maps;
using Application.Module.PlayerNPC.Channel.Models;
using Application.Module.PlayerNPC.Common;
using Application.Shared.MapObjects;
using Application.Utility.Compatible;
using Application.Utility.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using net.server;
using Serilog;
using System.Drawing;
using tools;

namespace Application.Module.PlayerNPC.Channel;

/// <summary>
/// @author RonanLana
/// </summary>
public class PlayerNPCPositioner : IPlayerPositioner
{
    public int NextPositionData { get; set; }

    readonly Configs _config;
    public PlayerNPCPositioner(Configs config, int nextStep)
    {
        NextPositionData = nextStep;
        _config = config;
    }

    public void UpdateNextPositionData(int value)
    {
        NextPositionData = value;
    }

    private bool isPlayerNpcNearby(List<Point> otherPos, Point searchPos, int xLimit, int yLimit)
    {
        int xLimit2 = xLimit / 2, yLimit2 = yLimit / 2;

        Rectangle searchRect = new Rectangle(searchPos.X - xLimit2, searchPos.Y - yLimit2, xLimit, yLimit);
        foreach (Point pos in otherPos)
        {
            Rectangle otherRect = new Rectangle(pos.X - xLimit2, pos.Y - yLimit2, xLimit, yLimit);

            if (otherRect.IntersectsWith(searchRect))
            {
                return true;
            }
        }

        return false;
    }

    private int calcDx(int newStep)
    {
        return _config.PLAYERNPC_AREA_X / (newStep + 1);
    }

    private int calcDy(int newStep)
    {
        return (_config.PLAYERNPC_AREA_Y / 2) + (_config.PLAYERNPC_AREA_Y / (1 << (newStep + 1)));
    }

    private List<Point> rearrangePlayerNpcPositions(IMap map, int newStep, int pnpcsSize)
    {
        Rectangle mapArea = map.getMapArea();

        int leftPx = mapArea.X + _config.PLAYERNPC_INITIAL_X, px, py = mapArea.Y + _config.PLAYERNPC_INITIAL_Y;
        int outx = mapArea.X + mapArea.Width - _config.PLAYERNPC_INITIAL_X, outy = mapArea.Y + mapArea.Height;
        int cx = calcDx(newStep), cy = calcDy(newStep);

        List<Point> otherPlayerNpcs = new();
        while (py < outy)
        {
            px = leftPx;

            while (px < outx)
            {
                var searchPos = map.getPointBelow(new Point(px, py));
                if (searchPos != null)
                {
                    if (!isPlayerNpcNearby(otherPlayerNpcs, searchPos.Value, cx, cy))
                    {
                        otherPlayerNpcs.Add(searchPos.Value);

                        if (otherPlayerNpcs.Count == pnpcsSize)
                        {
                            return otherPlayerNpcs;
                        }
                    }
                }

                px += cx;
            }

            py += cy;
        }

        return null;
    }

    private Point? rearrangePlayerNpcs(IMap map, int newStep, List<PlayerNpc> pnpcs)
    {
        Rectangle mapArea = map.getMapArea();

        int leftPx = mapArea.X + _config.PLAYERNPC_INITIAL_X, px, py = mapArea.Y + _config.PLAYERNPC_INITIAL_Y;
        int outx = mapArea.X + mapArea.Width - _config.PLAYERNPC_INITIAL_X, outy = mapArea.Y + mapArea.Height;
        int cx = calcDx(newStep), cy = calcDy(newStep);

        List<Point> otherPlayerNpcs = new();
        int i = 0;

        while (py < outy)
        {
            px = leftPx;

            while (px < outx)
            {
                var searchPos = map.getPointBelow(new Point(px, py));
                if (searchPos != null)
                {
                    if (!isPlayerNpcNearby(otherPlayerNpcs, searchPos.Value, cx, cy))
                    {
                        if (i == pnpcs.Count)
                        {
                            return searchPos;
                        }

                        var pn = pnpcs[i];
                        i++;

                        pn.updatePlayerNPCPosition(map, searchPos.Value);
                        otherPlayerNpcs.Add(searchPos.Value);
                    }
                }

                px += cx;
            }

            py += cy;
        }

        return null;    // this area should not be reached under any scenario
    }

    private Point? reorganizePlayerNpcs(IMap map, int newStep, List<IMapObject> mmoList)
    {
        if (mmoList.Count > 0)
        {
            if (YamlConfig.config.server.USE_DEBUG)
            {
                Log.Logger.Debug("Re-organizing pnpc map, step {Step}", newStep);
            }

            var playerNpcs = mmoList.OfType<PlayerNpc>().OrderBy(x => x.GetSourceId()).ToList();

            return rearrangePlayerNpcs(map, newStep, playerNpcs);
        }

        return null;
    }

    private Point? getNextPlayerNpcPosition(IMap map, int initStep)
    {
        // automated playernpc position thanks to Ronan
        var mmoList = map.getMapObjectsInRange(new Point(0, 0), double.PositiveInfinity, Arrays.asList(MapObjectType.PLAYER_NPC));
        List<Point> otherPlayerNpcs = new();
        foreach (var mmo in mmoList)
        {
            otherPlayerNpcs.Add(mmo.getPosition());
        }

        int cx = calcDx(initStep), cy = calcDy(initStep);
        Rectangle mapArea = map.getMapArea();
        int outx = mapArea.X + mapArea.Width - _config.PLAYERNPC_INITIAL_X, outy = mapArea.Y + mapArea.Height;
        bool reorganize = false;

        int i = initStep;
        while (i < _config.PLAYERNPC_AREA_STEPS)
        {
            int leftPx = mapArea.X + _config.PLAYERNPC_INITIAL_X, px, py = mapArea.Y + _config.PLAYERNPC_INITIAL_Y;

            while (py < outy)
            {
                px = leftPx;

                while (px < outx)
                {
                    var searchPos = map.getPointBelow(new Point(px, py));
                    if (searchPos != null)
                    {
                        if (!isPlayerNpcNearby(otherPlayerNpcs, searchPos.Value, cx, cy))
                        {
                            if (i > initStep)
                            {
                                UpdateNextPositionData(i);
                            }

                            if (reorganize && _config.PLAYERNPC_ORGANIZE_AREA)
                            {
                                return reorganizePlayerNpcs(map, i, mmoList);
                            }

                            return searchPos.Value;
                        }
                    }

                    px += cx;
                }

                py += cy;
            }

            reorganize = true;
            i++;

            cx = calcDx(i);
            cy = calcDy(i);
            if (_config.PLAYERNPC_ORGANIZE_AREA)
            {
                otherPlayerNpcs = rearrangePlayerNpcPositions(map, i, mmoList.Count);
            }
        }

        if (i > initStep)
        {
            UpdateNextPositionData(_config.PLAYERNPC_AREA_STEPS - 1);
        }
        return null;
    }

    public Point? GetNextPlayerNpcPosition(IMap map)
    {
        return getNextPlayerNpcPosition(map, NextPositionData);
    }
}
