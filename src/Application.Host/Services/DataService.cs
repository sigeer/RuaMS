using Application.EF;
using server.life;

namespace Application.Host.Services
{
    public class DataService
    {
        readonly DBContext _dbContext;

        public DataService(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<KeyValuePair<int, string>> FilterMonster(string mobName)
        {
            return MonsterInformationProvider.getInstance().getMobsIDsFromName(mobName);
        }
    }
}
