using Application.Core.Game.TheWorld;
using Microsoft.EntityFrameworkCore;

namespace Application.Core.Managers
{
    public class FamilyManager
    {
        static readonly ILogger log = LogFactory.GetLogger(LogType.Family);
        public static void resetEntitlementUsage(IWorld world)
        {
            var resetTime = DateTimeOffset.Now.AddMinutes(1).ToUnixTimeMilliseconds();
            try
            {
                using var dbContext = new DBContext();
                using var dbTrans = dbContext.Database.BeginTransaction();
                try
                {
                    dbContext.FamilyCharacters.Where(x => x.Lastresettime <= resetTime).ExecuteUpdate(x => x.SetProperty(y => y.Todaysrep, 0).SetProperty(y => y.Reptosenior, 0));
                }
                catch (Exception e)
                {
                    log.Error(e, "Could not reset daily rep for families");
                }
                try
                {
                    dbContext.FamilyEntitlements.Where(x => x.Timestamp <= resetTime).ExecuteDelete();
                }
                catch (Exception e)
                {
                    log.Error(e, "Could not do daily reset for family entitlements");
                }
                dbTrans.Commit();
            }
            catch (Exception e)
            {
                log.Error(e, "Could not get connection to DB");
            }
        }
    }
}
