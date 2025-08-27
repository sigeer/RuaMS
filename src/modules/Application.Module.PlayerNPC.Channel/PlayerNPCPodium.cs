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
using Microsoft.Extensions.Options;
using Serilog;
using System.Drawing;

namespace Application.Module.PlayerNPC.Channel;

/**
 * @author RonanLana
 * <p>
 * Note: the podium uses getGroundBelow that in its turn uses inputted posY minus 7.
 * Podium system will implement increase-by-7 to negate that behaviour.
 */
public class PlayerNPCPodium : IPlayerPositioner
{
    readonly Configs _config;
    public PlayerNPCPodium(Configs config, int nextStep)
    {
        NextPositionData = nextStep;
        _config = config;
    }

    public int NextPositionData { get; set; }

    private void UpdateNextPositionData(int value)
    {
        NextPositionData = value;
    }

    private int getPlatformPosX(int platform)
    {
        return platform switch
        {
            0 => -50,
            1 => -170,
            _ => 70
        };
    }

    private int getPlatformPosY(int platform)
    {
        if (platform == 0)
        {
            return -47;
        }
        return 40;
    }

    private Point calcNextPos(int rank, int step)
    {
        int podiumPlatform = rank / step;
        int relativePos = (rank % step) + 1;

        Point pos = new Point(getPlatformPosX(podiumPlatform) + ((100 * relativePos) / (step + 1)), getPlatformPosY(podiumPlatform));
        return pos;
    }

    private Point rearrangePlayerNpcs(IMap map, int newStep, List<PlayerNpc> pnpcs)
    {
        int i = 0;
        foreach (var pn in pnpcs)
        {
            pn.updatePlayerNPCPosition(map, calcNextPos(i, newStep));
            i++;
        }

        return calcNextPos(i, newStep);
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

    public int GetPodiumDataByIndex(int index)
    {
        if (index == 0)
            return 1;

        int podiumData = GetPodiumDataByIndex(index - 1);
        int podiumCount = (podiumData / (1 << 5));
        int podiumStep = podiumData % (1 << 5);

        if (podiumCount >= 3 * podiumStep)
        {
            if (podiumStep >= _config.PLAYERNPC_AREA_STEPS)
            {
                return -1;
            }

            return encodePodiumData(podiumStep + 1, podiumCount + 1);
        }
        else
        {
            return encodePodiumData(podiumStep, podiumCount + 1);
        }
    }

    private int encodePodiumData(int podiumStep, int podiumCount)
    {
        return (podiumCount * (1 << 5)) + podiumStep;
    }

    private Point? getNextPlayerNpcPosition(IMap map, int podiumData)
    {
        // automated playernpc position thanks to Ronan
        int podiumCount = (podiumData / (1 << 5));
        int podiumStep = podiumData % (1 << 5);

        if (podiumCount >= 3 * podiumStep)
        {
            if (podiumStep >= _config.PLAYERNPC_AREA_STEPS)
            {
                return null;
            }

            var mmoList = map.GetMapObjects(x => x.getType() == MapObjectType.PLAYER_NPC);
            var podimuData = encodePodiumData(podiumStep + 1, podiumCount + 1);
            UpdateNextPositionData(podimuData);
            return reorganizePlayerNpcs(map, podiumStep + 1, mmoList);
        }
        else
        {
            var outPodiumData = encodePodiumData(podiumStep, podiumCount + 1);
            UpdateNextPositionData(outPodiumData);
            return calcNextPos(podiumCount, podiumStep);
        }
    }

    public Point? GetNextPlayerNpcPosition(IMap map)
    {
        var pos = getNextPlayerNpcPosition(map, NextPositionData);
        if (pos == null)
        {
            return null;
        }

        return map.getGroundBelow(pos.Value);
    }
}
