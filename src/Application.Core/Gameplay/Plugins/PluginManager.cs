using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Plugins;

namespace Application.Core.Gameplay.Plugins
{
    /// <summary>
    /// 常规插件
    /// </summary>
    public sealed class PluginManager : AbstractPluginManager<IPluginServiceBase>
    {
        private readonly List<IMobListener> _mobListeners = new();
        private readonly List<IMapListener> _mapListeners = new();

        public override async Task<bool> LoadPlugin(string pluginDllName)
        {
            if (await base.LoadPlugin(pluginDllName))
            {
                DiscoverListeners();
                return true;
            }
            return false;
        }

        public override async Task<bool> UnloadPlugin(string pluginDllName)
        {
            if (await base.UnloadPlugin(pluginDllName))
            {
                DiscoverListeners();
                return true;
            }
            return false;
        }

        private void DiscoverListeners()
        {
            _mobListeners.Clear();
            _mapListeners.Clear();

            foreach (var plugin in GetAllPlugins())
            {
                if (plugin is IMobListener mobListener)
                    _mobListeners.Add(mobListener);

                if (plugin is IMapListener mapListener)
                    _mapListeners.Add(mapListener);
            }
        }

        public void OnMobSpawned(Monster mob)
        {
            foreach (var listener in _mobListeners)
            {
                try
                {
                    listener.OnMobSpawned(mob);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "MobListener.OnMobSpawned error");
                }
            }
        }

        public void OnMobHealed(Monster mob, int value)
        {
            foreach (var listener in _mobListeners)
            {
                try
                {
                    listener.OnMobHealed(mob, value);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "MobListener.OnMobHealed error");
                }
            }
        }

        public void OnMobKilled(Monster mob, ICombatantObject? killer)
        {
            foreach (var listener in _mobListeners)
            {
                try
                {
                    listener.OnMobKilled(mob, killer);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "MobListener.OnMobKilled error");
                }
            }
        }

        public void OnMobDamaged(Monster mob, int damage, ICombatantObject? attacker)
        {
            foreach (var listener in _mobListeners)
            {
                try
                {
                    listener.OnMobDamaged(mob, damage, attacker);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "MobListener.OnMobDamaged error");
                }
            }
        }

        public void OnMapObjectEnterField(IMap map, IMapObject mapObject)
        {
            foreach (var listener in _mapListeners)
            {
                try
                {
                    listener.OnMapObjectEnterField(map, mapObject);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "MapListener.OnMapObjectEnterField error");
                }
            }
        }

        public void OnMapObjectLeaveField(IMap map, IMapObject mapObject)
        {
            foreach (var listener in _mapListeners)
            {
                try
                {
                    listener.OnMapObjectLeaveField(map, mapObject);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "MapListener.OnMapObjectLeaveField error");
                }
            }
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();

            _mapListeners.Clear();
            _mobListeners.Clear();
        }
    }
}
