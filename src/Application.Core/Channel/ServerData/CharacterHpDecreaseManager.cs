using Application.Core.Game.Controllers;

namespace Application.Core.Channel.ServerData
{
    public class CharacterHpDecreaseManager : TimelyControllerBase
    {
        private Dictionary<IPlayer, int> playerHpDec = new Dictionary<IPlayer, int>();

        public CharacterHpDecreaseManager(WorldChannel channel) : base($"CharacterHpDecreaseController_{channel.InstanceId}"
            , TimeSpan.FromMilliseconds(YamlConfig.config.server.MAP_DAMAGE_OVERTIME_INTERVAL)
            , TimeSpan.FromMilliseconds(YamlConfig.config.server.MAP_DAMAGE_OVERTIME_INTERVAL))
        {
        }

        public void addPlayerHpDecrease(IPlayer chr)
        {
            playerHpDec.TryAdd(chr, 0);
        }

        public void removePlayerHpDecrease(IPlayer chr)
        {
            playerHpDec.Remove(chr);
        }

        protected override void HandleRun()
        {
            Dictionary<IPlayer, int> m = new();
            m.putAll(playerHpDec);

            foreach (var e in m)
            {
                IPlayer chr = e.Key;

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
