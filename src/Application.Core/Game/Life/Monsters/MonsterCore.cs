using server.life;
using static Application.Templates.Mob.MobTemplate;

namespace Application.Core.Game.Life.Monsters
{
    /// <summary>
    /// 为什么attackinfo不放进stats？因为动画与mob强相关，属性则不一定？
    /// </summary>
    /// <param name="Stats"></param>
    /// <param name="AttackInfo"></param>
    public record MonsterCore(MonsterStats Stats, MobAttackTemplate[] AttackInfo);
}
