using Application.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Services
{
    public class IdService
    {
        private HashSet<int> _cashIdSet = new HashSet<int>(10000);

        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly ILogger<IdService> _logger;

        public IdService(IDbContextFactory<DBContext> dbContextFactory, ILogger<IdService> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public int GenerateCashId()
        {
            return 1;
        }
    }
}
