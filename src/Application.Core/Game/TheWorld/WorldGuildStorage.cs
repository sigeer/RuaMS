using Application.Core.Game.Relation;

namespace Application.Core.Game.TheWorld
{
    public class WorldGuildStorage : DataStorage<IGuild>
    {
        public IGuild? GetOrAdd(int guildId)
        {
            if (_dataSource.ContainsKey(guildId))
                return _dataSource[guildId];

            using var dbContext = new DBContext();
            var m = dbContext.Guilds.FirstOrDefault(x => x.GuildId == guildId);

            if (m == null)
                return null;

            var d = GlobalConfigs.Mapper.Map<Guild>(m);
            _dataSource.Add(guildId, d);
            return d;
        }
    }
}
