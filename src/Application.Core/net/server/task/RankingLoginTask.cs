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


using client;
using Microsoft.EntityFrameworkCore;

namespace net.server.task;




/// <summary>
/// 整点执行，间隔 <see cref="YamlConfig.config.server.RANKING_INTERVAL"/>
/// <para></para>
/// @author Matze
/// @author Quit
/// @author Ronan
/// </summary>
public class RankingLoginTask : AbstractRunnable
{
    private DateTimeOffset lastUpdate = DateTimeOffset.Now;

    private void resetMoveRank(DBContext dbContext, bool job)
    {
        dbContext.Characters.ExecuteUpdate(x => job ? x.SetProperty(y => y.JobRankMove, 0) : x.SetProperty(y => y.RankMove, 0));
    }

    private void updateRanking(DBContext dbContext, int job, int world)
    {
        var jobRangeStart = job * 100;
        var jobRangeEnd = job * 100 + 99;
        var dataList = (from a in dbContext.Characters.Where(x => x.World == world && (job == -1 || x.JobId >= jobRangeStart && x.JobId <= jobRangeEnd))
                        join b in dbContext.Accounts.Where(x => x.GMLevel < 2) on a.AccountId equals b.Id
                        orderby a.Level descending, a.Exp descending, a.LastExpGainTime ascending, a.Fame descending, a.Meso descending
                        select new { a.Id, rank = job != -1 ? a.JobRank : a.Rank, rankMove = job != -1 ? a.JobRankMove : a.RankMove, b.Lastlogin }).ToList();
        var filteredIds = dataList.Select(x => x.Id).ToList();
        var filteredCharacters = dbContext.Characters.Where(x => filteredIds.Contains(x.Id)).ToList();
        int rank = 0;
        foreach (var item in dataList)
        {
            int rankMove = 0;
            rank++;
            if (item.Lastlogin < lastUpdate)
            {
                rankMove = item.rankMove;
            }
            rankMove += item.rank - rank;

            var dbModel = filteredCharacters.FirstOrDefault(x => x.Id == item.Id);
            if (dbModel != null)
            {
                if (job != -1)
                {
                    dbModel.JobRank = rank;
                    dbModel.JobRankMove = rankMove;
                }
                else
                {
                    dbModel.Rank = rank;
                    dbModel.RankMove = rankMove;
                }
            }
        }
        dbContext.SaveChanges();
    }
    public override void HandleRun()
    {
        try
        {
            using var dbContext = new DBContext();
            using var dbTrans = dbContext.Database.BeginTransaction();

            if (YamlConfig.config.server.USE_REFRESH_RANK_MOVE)
            {
                resetMoveRank(dbContext, true);
                resetMoveRank(dbContext, false);
            }

            for (int j = 0; j < Server.getInstance().getWorldsSize(); j++)
            {
                updateRanking(dbContext, -1, j);    //overall ranking
                for (int i = 0; i <= JobUtils.getMax(); i++)
                {
                    updateRanking(dbContext, i, j);
                }
            }
            dbTrans.Commit();

            lastUpdate = DateTimeOffset.Now;
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }
    }
}
