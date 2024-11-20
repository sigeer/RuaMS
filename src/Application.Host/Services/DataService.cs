using Application.Core.Managers;
using Application.EF;
using Application.Utility.Configs;
using server;
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

        public List<KeyValuePair<int, string>> FilterMonster(string? mobName)
        {
            if (string.IsNullOrWhiteSpace(mobName))
                return [];

            return MonsterInformationProvider.getInstance().getMobsIDsFromName(mobName);
        }

        public List<KeyValuePair<int, string>> FilterItem(string? itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName))
                return [];

            return ItemInformationProvider.getInstance().getAllItems()
                .Where(x => x.Name.Contains(itemName, StringComparison.OrdinalIgnoreCase))
                .Select(x => new KeyValuePair<int, string>(x.Id, x.Name)).ToList();
        }

        public List<KeyValuePair<int, string>> GetWorldsData()
        {
            return ServerManager.LoadAllWorld().Select((x, index) => new KeyValuePair<int, string>(index, x.Name)).ToList();
        }
    }
}
