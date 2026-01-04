namespace Application.Core.Channel.ServerData
{
    public class CharacterHpDecreaseManager : TaskBase
    {
        private Dictionary<Player, int> playerHpDec = new Dictionary<Player, int>();

        public CharacterHpDecreaseManager(WorldChannelServer server) : base($"ChannelServer:{server.ServerName}_{nameof(CharacterHpDecreaseManager)}"
            , TimeSpan.FromMilliseconds(YamlConfig.config.server.MAP_DAMAGE_OVERTIME_INTERVAL)
            , TimeSpan.FromMilliseconds(YamlConfig.config.server.MAP_DAMAGE_OVERTIME_INTERVAL))
        {
        }

        public void addPlayerHpDecrease(Player chr)
        {
            playerHpDec.TryAdd(chr, 0);
        }

        public void removePlayerHpDecrease(Player chr)
        {
            playerHpDec.Remove(chr);
        }

        protected override void HandleRun()
        {
            Dictionary<Player, int> m = new();
            m.putAll(playerHpDec);

            foreach (var e in m)
            {
                Player chr = e.Key;

                if (!chr.isAwayFromWorld())
                {
                    int c = e.Value;
                    c = (c + 1) % YamlConfig.config.server.MAP_DAMAGE_OVERTIME_COUNT;
                    playerHpDec.AddOrUpdate(chr, c);

                    if (c == 0)
                    {
                        chr.doHurtHp();
                    }
                }
            }
        }

    }
}
