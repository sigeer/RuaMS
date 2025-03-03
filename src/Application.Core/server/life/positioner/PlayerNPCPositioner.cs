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


using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using net.server;
using server.maps;
using tools;

namespace server.life.positioner;
/**
 * @author RonanLana
 */
public class PlayerNPCPositioner
{
    private static ILogger log = LogFactory.GetLogger(LogType.PlayerNPC);

    private static bool isPlayerNpcNearby(List<Point> otherPos, Point searchPos, int xLimit, int yLimit)
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

    private static int calcDx(int newStep)
    {
        return YamlConfig.config.server.PLAYERNPC_AREA_X / (newStep + 1);
    }

    private static int calcDy(int newStep)
    {
        return (YamlConfig.config.server.PLAYERNPC_AREA_Y / 2) + (YamlConfig.config.server.PLAYERNPC_AREA_Y / (1 << (newStep + 1)));
    }

    private static List<Point> rearrangePlayerNpcPositions(IMap map, int newStep, int pnpcsSize)
    {
        Rectangle mapArea = map.getMapArea();

        int leftPx = mapArea.X + YamlConfig.config.server.PLAYERNPC_INITIAL_X, px, py = mapArea.Y + YamlConfig.config.server.PLAYERNPC_INITIAL_Y;
        int outx = mapArea.X + mapArea.Width - YamlConfig.config.server.PLAYERNPC_INITIAL_X, outy = mapArea.Y + mapArea.Height;
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

    private static Point? rearrangePlayerNpcs(IMap map, int newStep, List<PlayerNPC> pnpcs)
    {
        Rectangle mapArea = map.getMapArea();

        int leftPx = mapArea.X + YamlConfig.config.server.PLAYERNPC_INITIAL_X, px, py = mapArea.Y + YamlConfig.config.server.PLAYERNPC_INITIAL_Y;
        int outx = mapArea.X + mapArea.Width - YamlConfig.config.server.PLAYERNPC_INITIAL_X, outy = mapArea.Y + mapArea.Height;
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

                        PlayerNPC pn = pnpcs[i];
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

    private static Point? reorganizePlayerNpcs(IMap map, int newStep, List<IMapObject> mmoList)
    {
        if (mmoList.Count > 0)
        {
            if (YamlConfig.config.server.USE_DEBUG)
            {
                log.Debug("Re-organizing pnpc map, step {Step}", newStep);
            }

            List<PlayerNPC> playerNpcs = mmoList.OfType<PlayerNPC>().OrderBy(x => x.getScriptId()).ToList();

            foreach (var ch in Server.getInstance().getChannelsFromWorld(map.getWorld()))
            {
                var m = ch.getMapFactory().getMap(map.getId());

                foreach (var pn in playerNpcs)
                {
                    m.removeMapObject(pn);
                    m.broadcastMessage(PacketCreator.removeNPCController(pn.getObjectId()));
                    m.broadcastMessage(PacketCreator.removePlayerNPC(pn.getObjectId()));
                }
            }

            var ret = rearrangePlayerNpcs(map, newStep, playerNpcs);

            foreach (var ch in Server.getInstance().getChannelsFromWorld(map.getWorld()))
            {
                var m = ch.getMapFactory().getMap(map.getId());

                foreach (var pn in playerNpcs)
                {
                    m.addPlayerNPCMapObject(pn);
                    m.broadcastMessage(PacketCreator.spawnPlayerNPC(pn));
                    m.broadcastMessage(PacketCreator.getPlayerNPC(pn));
                }
            }

            return ret;
        }

        return null;
    }

    private static Point? getNextPlayerNpcPosition(IMap map, int initStep)
    {   // automated playernpc position thanks to Ronan
        var mmoList = map.getMapObjectsInRange(new Point(0, 0), double.PositiveInfinity, Arrays.asList(MapObjectType.PLAYER_NPC));
        List<Point> otherPlayerNpcs = new();
        foreach (var mmo in mmoList)
        {
            otherPlayerNpcs.Add(mmo.getPosition());
        }

        int cx = calcDx(initStep), cy = calcDy(initStep);
        Rectangle mapArea = map.getMapArea();
        int outx = mapArea.X + mapArea.Width - YamlConfig.config.server.PLAYERNPC_INITIAL_X, outy = mapArea.Y + mapArea.Height;
        bool reorganize = false;

        int i = initStep;
        while (i < YamlConfig.config.server.PLAYERNPC_AREA_STEPS)
        {
            int leftPx = mapArea.X + YamlConfig.config.server.PLAYERNPC_INITIAL_X, px, py = mapArea.Y + YamlConfig.config.server.PLAYERNPC_INITIAL_Y;

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
                                map.getWorldServer().setPlayerNpcMapStep(map.getId(), i);
                            }

                            if (reorganize && YamlConfig.config.server.PLAYERNPC_ORGANIZE_AREA)
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
            if (YamlConfig.config.server.PLAYERNPC_ORGANIZE_AREA)
            {
                otherPlayerNpcs = rearrangePlayerNpcPositions(map, i, mmoList.Count);
            }
        }

        if (i > initStep)
        {
            map.getWorldServer().setPlayerNpcMapStep(map.getId(), YamlConfig.config.server.PLAYERNPC_AREA_STEPS - 1);
        }
        return null;
    }

    public static Point? getNextPlayerNpcPosition(IMap map)
    {
        return getNextPlayerNpcPosition(map, map.getWorldServer().getPlayerNpcMapStep(map.getId()));
    }
}
