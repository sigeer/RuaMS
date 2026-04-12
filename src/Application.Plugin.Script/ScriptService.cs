using Application.Core.Client;
using Application.Core.Game.Maps;
using Application.Core.Gameplay.Plugins;
using Application.Core.scripting.Infrastructure;
using Application.Shared.Constants.Npc;
using Application.Utility.Exceptions;
using server.maps;
using System.Reflection;

namespace Application.Plugin.Script
{
    internal class ScriptService : IScriptService
    {
        static Dictionary<string, MethodInfo> _portalSource;
        static Dictionary<string, MethodInfo> _npcSource;
        static Dictionary<string, MethodInfo> _itemSource;
        static Dictionary<string, MethodInfo> _mapEnterSource;
        static Dictionary<string, MethodInfo> _mapFirstEnterSource;
        static Dictionary<string, MethodInfo> _reactorHitSource;
        static Dictionary<string, MethodInfo> _reactorActSource;
        static ScriptService()
        {
            _portalSource = ExtractMethodsToDictionary(typeof(PortalScript));
            _npcSource = ExtractMethodsToDictionary(typeof(NpcScript));
            _itemSource = ExtractMethodsToDictionary(typeof(ItemScript));

            _mapEnterSource = ExtractMethodsToDictionary(typeof(MapEnterScript));
            _mapFirstEnterSource = ExtractMethodsToDictionary(typeof(MapFirstEnterScript));

            _reactorHitSource = ExtractMethodsToDictionary(typeof(ReactorHitScript));
            _reactorActSource = ExtractMethodsToDictionary(typeof(ReactorActScript));
        }

        static Dictionary<string, MethodInfo> ExtractMethodsToDictionary(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var dict = new Dictionary<string, MethodInfo>();

            // 获取当前类声明的所有方法（包括公有、私有、实例、静态），不包含继承的方法
            var methods = type.GetMethods(
                BindingFlags.DeclaredOnly |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.Static
            );

            foreach (var method in methods)
            {
                // 获取方法上所有的 ScriptNameAttribute（不检查继承）
                var attributes = method.GetCustomAttribute<ScriptNameAttribute>(inherit: false);
                if (attributes != null)
                {
                    // 方法有特性：每个特性生成一个键值对
                    foreach (var attr in attributes.Name)
                    {
                        if (string.IsNullOrEmpty(attr))
                            throw new InvalidOperationException(
                                $"方法 '{method.Name}' 上的 ScriptNameAttribute 的 Name 属性不能为 null 或空。"
                            );

                        // 尝试添加，若键已存在则抛出异常
                        dict.Add(attr, method);
                    }
                }
                else
                {
                    dict.Add(method.Name, method);
                }
            }

            return dict;
        }
        public Task<bool> Enter(IChannelClient c, Portal p)
        {
            if (!_portalSource.TryGetValue(p.getScriptName()!, out var methodInfo))
            {
                // 
                throw new BusinessNotsupportException($"PortalScript: {p.getScriptName()}");
            }

            var script = new PortalScript(c, p);
            return (Task<bool>)methodInfo.Invoke(script, null)!;
        }

        public async Task Start(IChannelClient c, int npcId, int npcObjectId, string scriptName)
        {
            if (!_npcSource.TryGetValue(scriptName, out var methodInfo))
            {
                // 
                c.OnlinedCharacter.Pink($"不支持的脚本{scriptName}");
                return;
            }

            var talk = new NpcScript(c, npcId, npcObjectId);
            try
            {
                c.NPCConversationManager = talk;
                await (Task)methodInfo.Invoke(talk, null)!;
            }
            catch (ConversationInterruptException)
            {
                // 对话中断
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                talk.dispose();
            }

        }

        public async Task Action(IChannelClient c, sbyte mode, sbyte type, int selection, string? inputText = null)
        {
            if (c.NPCConversationManager is not NpcScript npcTalk)
            {
                return;
            }
            await npcTalk.Response(mode, type, selection, inputText);
        }

        public async Task ItemScript(IChannelClient c, int npcId, string scriptName)
        {
            if (!_itemSource.TryGetValue(scriptName, out var methodInfo))
            {
                // 
                c.OnlinedCharacter.Pink($"不支持的脚本{scriptName}");
                return;
            }

            var talk = new ItemScript(c, npcId);
            c.NPCConversationManager = talk;
            await (Task)methodInfo.Invoke(talk, null)!;
            talk.dispose();
        }

        public async Task MapFirstEnter(IChannelClient c, IMap map)
        {
            if (!_mapFirstEnterSource.TryGetValue(map.SourceTemplate.OnFirstUserEnter, out var methodInfo))
            {
                // 
                c.OnlinedCharacter.Pink($"不支持的脚本{map.SourceTemplate.OnFirstUserEnter}");
                return;
            }

            var script = new MapFirstEnterScript(c, map);
            await (Task)methodInfo.Invoke(script, null)!;
        }

        public async Task MapEnter(IChannelClient c, IMap map)
        {
            if (!_mapEnterSource.TryGetValue(map.SourceTemplate.OnUserEnter, out var methodInfo))
            {
                // 
                c.OnlinedCharacter.Pink($"不支持的脚本 {map.SourceTemplate.OnUserEnter}");
                return;
            }

            var script = new MapEnterScript(c, map);
            await (Task)methodInfo.Invoke(script, null)!;
        }
        public async Task ReactorHit(IChannelClient c, Reactor r)
        {
            if (!_reactorHitSource.TryGetValue(r.getStats().Action, out var methodInfo))
            {
                // 
                c.OnlinedCharacter.Pink($"不支持的脚本 {r.getStats().Action}");
                return;
            }

            var script = new ReactorHitScript(c, r);
            await (Task)methodInfo.Invoke(script, null)!;
        }

        public async Task ReactorAct(IChannelClient c, Reactor r)
        {
            if (!_reactorActSource.TryGetValue(r.getStats().Action, out var methodInfo))
            {
                // 
                c.OnlinedCharacter.Pink($"不支持的脚本 {r.getStats().Action}");
                return;
            }

            var script = new ReactorActScript(c, r);
            await (Task)methodInfo.Invoke(script, null)!;
        }
    }
}
