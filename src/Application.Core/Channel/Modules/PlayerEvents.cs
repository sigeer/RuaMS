using Application.Core.Game.Life;

namespace Application.Core.Channel.Events
{
    public class MonsterRewardEvent
    {
        public MonsterRewardEvent(Player toPlayer, Monster monster)
        {
            ToPlayer = toPlayer;
            Monster = monster;
        }

        public Player ToPlayer { get; }
        public Monster Monster { get; }
    }
}
