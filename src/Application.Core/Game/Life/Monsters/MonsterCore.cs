using server.life;

namespace Application.Core.Game.Life.Monsters
{
    public record MonsterCore(MonsterStats Stats, List<MobAttackInfoHolder> AttackInfo);
}
