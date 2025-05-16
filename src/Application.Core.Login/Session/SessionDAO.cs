using Application.EF;
using Application.EF.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using net.server.coordinator.session;

namespace Application.Core.Login.Session;


public class SessionDAO
{

    readonly ILogger<SessionDAO> _logger;
    readonly IDbContextFactory<DBContext> _dbContextFactory;

    public SessionDAO(ILogger<SessionDAO> logger, IDbContextFactory<DBContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

    public void deleteExpiredHwidAccounts()
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            dbContext.Hwidaccounts.Where(X => X.ExpiresAt < DateTimeOffset.Now).ExecuteDelete();
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to delete expired hwidaccounts");
        }
    }

    public List<Hwid> getHwidsForAccount(DBContext dbContext, int accountId)
    {
        return dbContext.Hwidaccounts.Where(x => x.AccountId == accountId).Select(x => new Hwid(x.Hwid)).ToList();
    }

    public void registerAccountAccess(DBContext dbContext, int accountId, Hwid? hwid, DateTimeOffset expiry)
    {
        if (hwid == null)
        {
            throw new ArgumentException("Hwid must not be null");
        }

        dbContext.Hwidaccounts.Add(new Hwidaccount(hwid.hwid, accountId, expiry));
        dbContext.SaveChanges();
    }

    public List<HwidRelevance> getHwidRelevance(DBContext dbContext, int accountId)
    {
        return dbContext.Hwidaccounts.Where(x => x.AccountId == accountId).Select(x => new HwidRelevance(x.Hwid, x.Relevance)).ToList();
    }

    public void updateAccountAccess(DBContext dbContext, Hwid hwid, int accountId, DateTimeOffset expiry, int loginRelevance)
    {
        dbContext.Hwidaccounts.Where(x => x.AccountId == accountId && x.Hwid.Contains(hwid.hwid))
    .ExecuteUpdate(x => x.SetProperty(y => y.ExpiresAt, expiry).SetProperty(y => y.Relevance, loginRelevance));

    }
}
