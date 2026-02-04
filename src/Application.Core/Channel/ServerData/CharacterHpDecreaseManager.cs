namespace Application.Core.Channel.ServerData
{
    public class CharacterHpDecreaseManager
    {
        private Dictionary<Player, int> playerHpDec = new Dictionary<Player, int>();
        readonly WorldChannel _server;
        public CharacterHpDecreaseManager(WorldChannel server) 
        {
            _server = server;
        }

        public void addPlayerHpDecrease(Player chr)
        {
            playerHpDec.TryAdd(chr, 0);
        }

        public void removePlayerHpDecrease(Player chr)
        {
            playerHpDec.Remove(chr);
        }

        public void HandleRun()
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
