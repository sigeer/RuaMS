using Application.Core.Game.Players.Tickables;
using Application.Core.Game.Skills;

namespace Application.Core.model
{
    public record BuffStateValuePair(BuffStat BuffStat, EffectBuff ValueHolder);
}
