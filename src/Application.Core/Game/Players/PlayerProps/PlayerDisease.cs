using server.life;

namespace Application.Core.Game.Players.PlayerProps
{
    public record PlayerDisease(Disease Disease, long StartTime, long Length, MobSkill FromMobSkill);
}
