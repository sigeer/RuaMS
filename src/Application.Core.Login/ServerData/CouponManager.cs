using Application.EF;
using Application.Shared.Constants.Item;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Application.Core.Login.Datas
{
    public class CouponManager
    {
        private static Dictionary<int, int> couponRates = new(30);
        private static List<int> activeCoupons = new();

        readonly IMapper _mapper;
        readonly ILogger<CouponManager> _logger;
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly MasterServer _server;

        public CouponManager(IMapper mapper, ILogger<CouponManager> logger, IDbContextFactory<DBContext> dbContextFactory, MasterServer server)
        {
            _mapper = mapper;
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _server = server;
        }

        public async Task Initialize(DBContext dbContext)
        {
            await loadCouponRates(dbContext);
            UpdateActiveCouponsInternal(dbContext);
        }
        public Config.CouponConfig GetConfig()
        {
            var data = new Config.CouponConfig();
            data.ActiveCoupons.AddRange(activeCoupons);
            foreach (var item in couponRates)
            {
                data.CouponRates.Add(item.Key, item.Value);
            }
            return data;
        }
        #region coupon
        private async Task loadCouponRates(DBContext dbContext)
        {
            couponRates =( await dbContext.Nxcoupons.AsNoTracking().Select(x => new { x.CouponId, x.Rate }).ToListAsync()).ToDictionary(x => x.CouponId, x => x.Rate);
        }


        public void CommitActiveCoupons()
        {
            _server.Transport.SendUpdateCouponRates(GetConfig());

        }

        public void ToggleCoupon(int couponId)
        {
            if (ItemConstants.isRateCoupon(couponId))
            {
                lock (activeCoupons)
                {
                    if (activeCoupons.Contains(couponId))
                    {
                        activeCoupons.Remove(couponId);
                    }
                    else
                    {
                        activeCoupons.Add(couponId);
                    }

                    CommitActiveCoupons();
                }
            }
        }

        public void UpdateActiveCouponsInternal(DBContext dbContext)
        {
            lock (activeCoupons)
            {
                activeCoupons.Clear();
                var d = _server.GetCurrentTimeDateTimeOffset();

                int weekDay = (int)d.DayOfWeek;
                weekDay = weekDay == 0 ? 7 : weekDay;
                int hourDay = d.Hour;

                int weekdayMask = 1 << weekDay;
                activeCoupons = dbContext.Nxcoupons.Where(x => x.Starthour <= hourDay && x.Endhour > hourDay && (x.Activeday & weekdayMask) == weekdayMask)
                        .Select(x => x.CouponId).ToList();

            }
        }

        public void UpdateActiveCoupons()
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            UpdateActiveCouponsInternal(dbContext);
        }
        #endregion
    }
}
