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


using net.server;
using net.server.channel;
using server.maps;
using tools;

namespace server.life.positioner;

/**
 * @author RonanLana
 * <p>
 * Note: the podium uses getGroundBelow that in its turn uses inputted posY minus 7.
 * Podium system will implement increase-by-7 to negate that behaviour.
 */
public class PlayerNPCPodium
{
    private static ILogger log = LogFactory.GetLogger("PlayerNPCPodium");

    private static int getPlatformPosX(int platform)
    {
        return platform switch
        {
            0 => -50,
            1 => -170,
            _ => 70
        };
    }

    private static int getPlatformPosY(int platform)
    {
        if (platform == 0)
        {
            return -47;
        }
        return 40;
    }

    private static Point calcNextPos(int rank, int step)
    {
        int podiumPlatform = rank / step;
        int relativePos = (rank % step) + 1;

        Point pos = new Point(getPlatformPosX(podiumPlatform) + ((100 * relativePos) / (step + 1)), getPlatformPosY(podiumPlatform));
        return pos;
    }

    private static Point rearrangePlayerNpcs(MapleMap map, int newStep, List<PlayerNPC> pnpcs)
    {
        int i = 0;
        foreach (PlayerNPC pn in pnpcs)
        {
            pn.updatePlayerNPCPosition(map, calcNextPos(i, newStep));
            i++;
        }

        return calcNextPos(i, newStep);
    }

    private static Point? reorganizePlayerNpcs(MapleMap map, int newStep, List<MapObject> mmoList)
    {
        if (mmoList.Count > 0)
        {
            if (YamlConfig.config.server.USE_DEBUG)
            {
                log.Debug("Re-organizing pnpc map, step {Step}", newStep);
            }

            List<PlayerNPC> playerNpcs = new(mmoList.Count);
            foreach (MapObject mmo in mmoList)
            {
                playerNpcs.Add((PlayerNPC)mmo);
            }

            playerNpcs.Sort((p1, p2) =>
            {
                return p1.getScriptId() - p2.getScriptId(); // scriptid as playernpc history
            });

            foreach (Channel ch in Server.getInstance().getChannelsFromWorld(map.getWorld()))
            {
                MapleMap m = ch.getMapFactory().getMap(map.getId());

                foreach (PlayerNPC pn in playerNpcs)
                {
                    m.removeMapObject(pn);
                    m.broadcastMessage(PacketCreator.removeNPCController(pn.getObjectId()));
                    m.broadcastMessage(PacketCreator.removePlayerNPC(pn.getObjectId()));
                }
            }

            Point ret = rearrangePlayerNpcs(map, newStep, playerNpcs);

            foreach (Channel ch in Server.getInstance().getChannelsFromWorld(map.getWorld()))
            {
                MapleMap m = ch.getMapFactory().getMap(map.getId());

                foreach (PlayerNPC pn in playerNpcs)
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

    private static int encodePodiumData(int podiumStep, int podiumCount)
    {
        return (podiumCount * (1 << 5)) + podiumStep;
    }

    private static Point? getNextPlayerNpcPosition(MapleMap map, int podiumData)
    {   // automated playernpc position thanks to Ronan
        int podiumStep = podiumData % (1 << 5), podiumCount = (podiumData / (1 << 5));

        if (podiumCount >= 3 * podiumStep)
        {
            if (podiumStep >= YamlConfig.config.server.PLAYERNPC_AREA_STEPS)
            {
                return null;
            }

            List<MapObject> mmoList = map.getMapObjectsInRange(new Point(0, 0), double.PositiveInfinity, Arrays.asList(MapObjectType.PLAYER_NPC));
            map.getWorldServer().setPlayerNpcMapPodiumData(map.getId(), encodePodiumData(podiumStep + 1, podiumCount + 1));
            return reorganizePlayerNpcs(map, podiumStep + 1, mmoList);
        }
        else
        {
            map.getWorldServer().setPlayerNpcMapPodiumData(map.getId(), encodePodiumData(podiumStep, podiumCount + 1));
            return calcNextPos(podiumCount, podiumStep);
        }
    }

    public static Point? getNextPlayerNpcPosition(MapleMap map)
    {
        var pos = getNextPlayerNpcPosition(map, map.getWorldServer().getPlayerNpcMapPodiumData(map.getId()));
        if (pos == null)
        {
            return null;
        }

        return map.getGroundBelow(pos.Value);
    }
}
