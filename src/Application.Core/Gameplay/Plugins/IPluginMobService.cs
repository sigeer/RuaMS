using Application.Core.Game.Life;
using Application.Core.Game.Maps;

namespace Application.Core.Gameplay.Plugins
{
    public interface IPluginMobService : IPluginServiceBase
    {
        void OnMobSpawned(Monster mob);
        void OnMobHealed(Monster mob, int value);
        void OnMobKilled(Monster mob, ICombatantObject? killer);
        void OnMobDamaged(Monster mob, int damage, ICombatantObject? attacker);
    }
}
