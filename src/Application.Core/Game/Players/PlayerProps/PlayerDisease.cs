using Application.Core.Server.life;
using Application.Shared.GameProps;

namespace Application.Core.Game.Players.PlayerProps
{
    /// <summary>
    /// 玩家身上一个疾病状态（<paramref name="Stat"/> 是 <c>BuffStat</c> 里对应的位，元数据见 <c>DiseaseInfo</c>）。
    /// </summary>
    public record PlayerDisease(BuffStat Stat, long StartTime, long Length, MobSkill FromMobSkill);
}
