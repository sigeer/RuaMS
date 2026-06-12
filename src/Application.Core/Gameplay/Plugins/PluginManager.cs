using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Plugins;
using System.Collections.Immutable;

namespace Application.Core.Gameplay.Plugins
{
    /// <summary>
    /// 常规插件
    /// </summary>
    public sealed class PluginManager : AbstractPluginManager<IPluginServiceBase>
    {
        private volatile ImmutableList<IPluginMobService> _mobListeners = ImmutableList<IPluginMobService>.Empty;
        private volatile ImmutableList<IPluginMapService> _mapListeners = ImmutableList<IPluginMapService>.Empty;
        private volatile ImmutableList<IPluginLifeService> _lifeListeners = ImmutableList<IPluginLifeService>.Empty;

        public PluginManager(WorldChannelServer server) : base(server)
        {
        }

        protected override void OnPluginContainerChanged()
        {
            DiscoverListeners();
        }

        protected override void OnPluginMounted(string pluginDllName)
        {
            base.OnPluginMounted(pluginDllName);

            foreach (var listener in _lifeListeners)
            {
                try
                {
                    listener.OnMounted(_server);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "插件{PluginName}.OnMounted 失败");
                }
            }
        }

        protected override void OnPluginUnmounted(string pluginDllName)
        {
            base.OnPluginUnmounted(pluginDllName);

            foreach (var listener in _lifeListeners)
            {
                try
                {
                    listener.OnUnmounted(_server);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "插件{PluginName}.OnUnmounted 初始化失败");
                }
            }
        }

        private void DiscoverListeners()
        {
            var mobListenersBuilder = ImmutableList.CreateBuilder<IPluginMobService>();
            var mapListenersBuilder = ImmutableList.CreateBuilder<IPluginMapService>();
            var commandListenersBuilder = ImmutableList.CreateBuilder<IPluginLifeService>();
            foreach (var plugin in GetAllPlugins())
            {
                if (plugin is IPluginMobService mobListener)
                    mobListenersBuilder.Add(mobListener);
                if (plugin is IPluginMapService mapListener)
                    mapListenersBuilder.Add(mapListener);
                if (plugin is IPluginLifeService commandListener)
                    commandListenersBuilder.Add(commandListener);
            }
            _mobListeners = mobListenersBuilder.ToImmutable();
            _mapListeners = mapListenersBuilder.ToImmutable();
            _lifeListeners = commandListenersBuilder.ToImmutable();
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
    }
}
