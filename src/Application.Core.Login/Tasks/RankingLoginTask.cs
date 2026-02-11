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


using Application.Core.Login.Models;
using Application.Utility.Tasks;

namespace Application.Core.Login.Tasks;




/// <summary>
/// 整点执行，间隔 <see cref="YamlConfig.config.server.RANKING_INTERVAL"/>
/// <para></para>
/// @author Matze
/// @author Quit
/// @author Ronan
/// </summary>
public class RankingLoginTask : AbstractRunnable
{
    readonly MasterServer _server;

    public RankingLoginTask(MasterServer server) : base($"{server.InstanceName}_{nameof(RankingLoginTask)}")
    {
        _server = server;
    }

    private void resetMoveRank(List<CharacterModel> all)
    {
        foreach (var item in all)
        {
            item.JobRankMove = 0;
            item.RankMove = 0;
        }
    }

    public void RecalcRankMove(List<CharacterModel> all, int job)
    {
        var jobRangeStart = job * 100;
        var jobRangeEnd = job * 100 + 99;
        var sorted = all.Where(x => (job == -1 || x.JobId >= jobRangeStart && x.JobId <= jobRangeEnd))
            .OrderByDescending(x => x.Level)
            .ThenByDescending(x => x.Exp)
            .ThenByDescending(x => x.LastExpGainTime)
            .ThenByDescending(x => x.Fame)
            .ThenByDescending(x => x.Meso).ToList();

        int rank = 0;
        foreach (var item in sorted)
        {
            int rankMove = 0;
            rank++;
            rankMove += (job != -1 ? item.JobRank : item.Rank) - rank;

            if (job != -1)
            {
                item.JobRank = rank;
                item.JobRankMove = rankMove;
            }
            else
            {
                item.Rank = rank;
                item.RankMove = rankMove;
            }
        }
    }

    public override void HandleRun()
    {
        try
        {
            var all = _server.CharacterManager.GetAllCachedPlayers();
            resetMoveRank(all);
            RecalcRankMove(all, -1);    //overall ranking
            for (int i = 0; i <= JobUtils.getMax(); i++)
            {
                RecalcRankMove(all, i);
            }
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }
    }
}
