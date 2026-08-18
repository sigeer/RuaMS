using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Gameplay.Plugins;

namespace Application.Core.Gameplay.Plugins.Services
{
    [PluginInvocation(PluginInvocationMode.Broadcast)]
    public interface IPluginMobService : IPluginServiceBase
    {
        void OnMobSpawned(Monster mob);
        void OnMobHealed(Monster mob, int value);
        void OnMobKilled(Monster mob, ICombatantObject? killer);
        void OnMobDamaged(Monster mob, int damage, ICombatantObject? attacker);
    }
}
