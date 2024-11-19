using Application.EF;

namespace Application.Host.Services
{
    public class GachaponService
    {
        readonly DBContext _dbContext;

        public GachaponService(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

    }
}
