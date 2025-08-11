using Application.Core.Game.Life;

namespace Application.Core.Channel.Events
{
    public class MonsterRewardEvent
    {
        public MonsterRewardEvent(IPlayer toPlayer, Monster monster)
        {
            ToPlayer = toPlayer;
            Monster = monster;
        }

        public IPlayer ToPlayer { get; }
        public Monster Monster { get; }
    }
}
