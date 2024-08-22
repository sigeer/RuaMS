using Microsoft.EntityFrameworkCore;

namespace net.server.coordinator.session;


public class SessionDAO
{
    private static ILogger log = LogFactory.GetLogger(LogType.Session);

    public static void deleteExpiredHwidAccounts()
    {
        try
        {
            using var dbContext = new DBContext();
            dbContext.Hwidaccounts.Where(X => X.ExpiresAt < DateTimeOffset.Now).ExecuteDelete();
        }
        catch (Exception e)
        {
            log.Warning(e, "Failed to delete expired hwidaccounts");
        }
    }

    public static List<Hwid> getHwidsForAccount(DBContext dbContext, int accountId)
    {
        return dbContext.Hwidaccounts.Where(x => x.AccountId == accountId).Select(x => new Hwid(x.Hwid)).ToList();
    }

    public static void registerAccountAccess(DBContext dbContext, int accountId, Hwid? hwid, DateTimeOffset expiry)
    {
        if (hwid == null)
        {
            throw new ArgumentException("Hwid must not be null");
        }

        dbContext.Hwidaccounts.Add(new Hwidaccount(hwid.hwid, accountId, expiry));
        dbContext.SaveChanges();
    }

    public static List<HwidRelevance> getHwidRelevance(DBContext dbContext, int accountId)
    {
        return dbContext.Hwidaccounts.Where(x => x.AccountId == accountId).Select(x => new HwidRelevance(x.Hwid, x.Relevance)).ToList();
    }

    public static void updateAccountAccess(DBContext dbContext, Hwid hwid, int accountId, DateTimeOffset expiry, int loginRelevance)
    {
        dbContext.Hwidaccounts.Where(x => x.AccountId == accountId && x.Hwid.Contains(hwid.hwid))
    .ExecuteUpdate(x => x.SetProperty(y => y.ExpiresAt, expiry).SetProperty(y => y.Relevance, loginRelevance));

    }
}
