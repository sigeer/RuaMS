namespace Application.Core.Managers
{
    public class CommonManager
    {
        public static long GetRelativeWeddingTicketExpireTime(int resSlot)
        {
            return (long)resSlot * YamlConfig.config.server.WEDDING_RESERVATION_INTERVAL * 60 * 1000;
        }
    }
}
