namespace Application.Core.Managers
{
    public class GuildManager
    {

        public static int getIncreaseGuildCost(int size)
        {
            int cost = YamlConfig.config.server.EXPAND_GUILD_BASE_COST + Math.Max(0, (size - 15) / 5) * YamlConfig.config.server.EXPAND_GUILD_TIER_COST;

            if (size > 30)
            {
                return Math.Min(YamlConfig.config.server.EXPAND_GUILD_MAX_COST, Math.Max(cost, 5000000));
            }
            else
            {
                return cost;
            }
        }
    }
}
