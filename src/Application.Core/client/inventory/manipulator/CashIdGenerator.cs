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


using Microsoft.EntityFrameworkCore;

namespace client.inventory.manipulator;


/**
 * @author RonanLana
 */
public class CashIdGenerator
{
    private static ILogger log = LogFactory.GetLogger("CashIdGenerator");
    private static HashSet<int> existentCashids = new(10000);
    private static int runningCashid = 0;

    private static void loadExistentCashIdsFromQuery(DBContext dbContext, string query)
    {
        var list = dbContext.Database.SqlQueryRaw<int>(query);
        foreach (var id in list)
        {
            existentCashids.Add(id);
        }
    }

    static object loadLock = new object();
    public static void loadExistentCashIdsFromDb(DBContext dbContext)
    {
        lock (loadLock)
        {

            loadExistentCashIdsFromQuery(dbContext, "SELECT id FROM rings");
            loadExistentCashIdsFromQuery(dbContext, "SELECT petid FROM pets");

            runningCashid = 0;
            do
            {
                runningCashid++;    // hopefully the id will never surpass the allotted amount for pets/rings?
            } while (existentCashids.Contains(runningCashid));
        }
    }

    private static void getNextAvailableCashId()
    {
        runningCashid++;
        if (runningCashid >= 777000000)
        {
            existentCashids.Clear();
            try
            {
                using var con = new DBContext();
                loadExistentCashIdsFromDb(con);
            }
            catch (Exception e)
            {
                log.Warning(e, "Failed to reset overflowing cash ids");
            }
        }
    }

    static object cashIdGenLock = new object();
    public static int generateCashId()
    {
        lock (cashIdGenLock)
        {
            while (true)
            {
                if (!existentCashids.Contains(runningCashid))
                {
                    int ret = runningCashid;
                    getNextAvailableCashId();

                    // existentCashids.Add(ret)... no need to do this since the wrap over already refetches already used cashids from the DB
                    return ret;
                }

                getNextAvailableCashId();
            }
        }

    }

    static object cashIdFreeLock = new object();
    public static void freeCashId(int cashId)
    {
        lock (cashIdFreeLock)
        {
            existentCashids.Remove(cashId);
        }

    }

}
