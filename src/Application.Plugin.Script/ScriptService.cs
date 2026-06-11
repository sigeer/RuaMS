using Application.Core.Channel;
using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Gameplay.Plugins;
using Application.Core.scripting.Infrastructure;
using Application.Core.Scripting.Events;
using Application.Plugin.Script.Events;
using Application.Plugin.Script.Npc;
using Application.Plugin.Script.Quest;
using Application.Shared.Constants.Job;
using Application.Shared.Constants.Map;
using Application.Shared.Quest;
using Application.Utility.Exceptions;
using scripting.quest;
using Serilog;
using server.maps;
using System.Reflection;
using tools;

namespace Application.Plugin.Script
{
    internal class ScriptService : IScriptService
    {
        Dictionary<string, MethodInfo> _portalSource;
        Dictionary<string, MethodInfo> _npcSource;
        Dictionary<string, MethodInfo> _itemSource;
        Dictionary<string, MethodInfo> _mapEnterSource;
        Dictionary<string, MethodInfo> _mapFirstEnterSource;
        Dictionary<string, MethodInfo> _reactorHitSource;
        Dictionary<string, MethodInfo> _reactorActSource;
        Dictionary<string, MethodInfo> _questSource;

        List<Type> _eventSource;
        public ScriptService()
        {
            _portalSource = ExtractMethodsToDictionary(typeof(PortalScript));
            _npcSource = ExtractMethodsToDictionary(typeof(NpcScript));
            _itemSource = ExtractMethodsToDictionary(typeof(ItemScript));

            _mapEnterSource = ExtractMethodsToDictionary(typeof(MapEnterScript));
            _mapFirstEnterSource = ExtractMethodsToDictionary(typeof(MapFirstEnterScript));

            _reactorHitSource = ExtractMethodsToDictionary(typeof(ReactorHitScript));
            _reactorActSource = ExtractMethodsToDictionary(typeof(ReactorActScript));

            _questSource = ExtractMethodsToDictionary(typeof(QuestScript));

            _eventSource = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsAssignableFrom(typeof(EventManager)) && x.IsClass && !x.IsAbstract)
                .ToList();
        }

        static Dictionary<string, MethodInfo> ExtractMethodsToDictionary(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var dict = new Dictionary<string, MethodInfo>();

            // 获取当前类声明的所有方法（包括公有、实例），不包含继承的方法
            var methods = type.GetMethods(
                BindingFlags.DeclaredOnly |
                BindingFlags.Public |
                BindingFlags.Instance
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
        public bool Enter(IChannelClient c, Portal p)
        {
            if (!_portalSource.TryGetValue(p.getScriptName()!, out var methodInfo))
            {
                // 
                throw new BusinessNotsupportException($"PortalScript: {p.getScriptName()}");
            }

            var script = new PortalScript(c, p);
            return (bool)methodInfo.Invoke(script, null)!;
        }

        public async Task<bool> Start(IChannelClient c, int npcId, NPC? npcObject, string? scriptName)
        {
            if (c.NPCConversationManager != null)
            {
                c.OnlinedCharacter.Pink("卡对话了");
                return false;
            }

            if (string.IsNullOrEmpty(scriptName))
            {
                scriptName = $"n{npcId}";
            }

            if (!c.canClickNPC())
            {
                c.OnlinedCharacter.Pink("对话太过频繁");
                return false;
            }

            if (!_npcSource.TryGetValue(scriptName, out var methodInfo))
            {
                // 
                c.OnlinedCharacter.Pink($"不支持的脚本 {scriptName}");
                c.sendPacket(PacketCreator.getNPCTalk(npcId, 0, c.CurrentCulture.GetNpcDefaultTalk(npcId, -1), "00 00", 0, 0));
                return false;
            }


            var talk = new NpcScript(c, npcId, npcObject);
            try
            {
                if (npcObject != null && c.OnlinedCharacter.getEventInstance() != npcObject.getMap().getEventInstance())
                {
                    throw new ConversationDiffInstanceException();
                }

                c.setClickedNPC();
                c.NPCConversationManager = talk;
                await (Task)methodInfo.Invoke(talk, null)!;
                return true;
            }
            catch (ConversationInterruptException)
            {
                // 对话中断
                return true;
            }
            catch (ConversationDiffInstanceException)
            {
                if (await talk.AskYesNo("你是怎么到这里来的？让我带你离开这里。"))
                {
                    talk.WarpOut();
                }

                Log.Logger.Warning("不合法的对话（EIM不同）：NpcId = {NPCId}, Script = {ScriptName}", npcId, scriptName);
                return true;
            }
            catch (ConversationDiffMapException)
            {
                await talk.SayOK(talk.GetDefault0());
                Log.Logger.Warning("不合法的对话（地图不同）：NpcId = {NPCId}, Script = {ScriptName}", npcId, scriptName);
                return true;
            }
            catch (NotImplementedException)
            {
                await talk.SayOK($"NPC {npcObject?.getName() ?? npcId.ToString()} 对话未实现。");
                Log.Logger.Warning("不支持的脚本：NpcId = {NPCId}, Script = {ScriptName}", npcId, scriptName);
                return false;
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

        public Task<bool> StartQuest(IChannelClient c, server.quest.Quest questObj, int npcId) => HandleQuestScript(c, questObj, npcId, true);
        public Task<bool> CompleteQuest(IChannelClient c, server.quest.Quest questObj, int npcId) => HandleQuestScript(c, questObj, npcId, false);


        async Task<bool> HandleQuestScript(IChannelClient c, server.quest.Quest questObj, int npcId, bool isStarted)
        {
            if (c.NPCConversationManager != null)
            {
                c.OnlinedCharacter.Pink("卡对话了");
                return false;
            }

            if (!c.canClickNPC())
            {
                c.OnlinedCharacter.Pink("对话太过频繁");
                return false;
            }

            var scriptName = isStarted ? questObj.GetStartScript() : questObj.GetEndScript();
            if (string.IsNullOrEmpty(scriptName))
            {
                throw new BusinessResException($"QuestId={questObj.getId()}客户端wz中包含了startScript/endScript节点，但是服务端没有");
            }

            if (!_questSource.TryGetValue(scriptName, out var methodInfo))
            {
                // 
                c.OnlinedCharacter.Pink($"不支持的脚本 {scriptName}");
                return false;
            }

            if (c.NPCConversationManager != null)
            {
                return false;
            }


            var talk = new QuestScript(c, questObj, npcId);
            try
            {
                c.NPCConversationManager = talk;
                c.setClickedNPC();

                await (Task)methodInfo.Invoke(talk, null)!;
                return true;
            }
            catch (ConversationInterruptException)
            {
                // 对话中断
                return true;
            }
            catch (ConversationDiffInstanceException)
            {
                if (await talk.AskYesNo("你是怎么到这里来的？让我带你离开这里。"))
                {
                    talk.WarpOut();
                }
                Log.Logger.Warning("不合法的对话（EIM不同）：NpcId = {NPCId}, Script = {ScriptName}", npcId, scriptName);
                return true;
            }
            catch (ConversationDiffMapException)
            {
                await talk.SayOK(talk.GetDefault0());
                Log.Logger.Warning("不合法的对话（地图不同）：NpcId = {NPCId}, Script = {ScriptName}", npcId, scriptName);
                return true;
            }
            catch (NotImplementedException)
            {
                await talk.SayOK($"任务 {c.CurrentCulture.GetQuestName(questObj.getId()) ?? questObj.getId().ToString()} 对话未实现。");
                Log.Logger.Warning("不支持的脚本：NpcId = {NPCId}, Script = {ScriptName}", npcId, scriptName);
                return false;
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
            if (c.NPCConversationManager is NpcScript npcTalk)
            {
                await npcTalk.Response(mode, type, selection, inputText);
            }
            else if (c.NPCConversationManager is QuestActionManager questTalk)
            {
                await questTalk.Response(mode, type, selection, inputText);
            }
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

        public void MapFirstEnter(IChannelClient c, IMap map)
        {
            if (!_mapFirstEnterSource.TryGetValue(map.SourceTemplate.OnFirstUserEnter, out var methodInfo))
            {
                // 
                c.OnlinedCharacter.Pink($"不支持的脚本{map.SourceTemplate.OnFirstUserEnter}");
                return;
            }

            var script = new MapFirstEnterScript(c, map);
            methodInfo.Invoke(script, null);
        }

        public void MapEnter(IChannelClient c, IMap map)
        {
            if (!_mapEnterSource.TryGetValue(map.SourceTemplate.OnUserEnter, out var methodInfo))
            {
                // 
                c.OnlinedCharacter.Pink($"不支持的脚本 {map.SourceTemplate.OnUserEnter}");
                return;
            }

            var script = new MapEnterScript(c, map);
            methodInfo.Invoke(script, null);
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

        public int RegisterEvents(WorldChannel channel)
        {
            return channel.EventScriptManager.ReloadEventScript([
                new PQ_Henesys(channel),
                new PQ_Kerning(channel),
                new PQ_Ellin(channel),
                new PQ_Ludi(channel),
                new PQ_WuGong(channel),

                new PQ_CPQ1(channel, nameof(PQ_CPQ1) + "1", 980000100),
                new PQ_CPQ1(channel, nameof(PQ_CPQ1) + "2", 980000200),
                new PQ_CPQ1(channel, nameof(PQ_CPQ1) + "3", 980000300),

                new PQ_Ariant(channel, nameof(PQ_Ariant) + "1", 980010100),
                new PQ_Ariant(channel, nameof(PQ_Ariant) + "2", 980010200),
                new PQ_Ariant(channel, nameof(PQ_Ariant) + "3", 980010300),

                new PQ_Zakum(channel),
                new Battle_Zakum(channel),

                new Battle_Balrog(channel),

                new PrivateContiMove(channel, "KerningTrain", [103000100, 103000310], [103000301, 103000302], 50),
                // 天空之城 - 圣地
                new PrivateContiMove(channel, "ShipOribs", [MapId.ORBIS_STATION, MapId.SKY_FERRY],[200090020, 200090021], 8 * 60),
                // 魔法密林 - 圣地
                new PrivateContiMove(channel, "ShipEllin", [MapId.ELLINIA_SKY_FERRY, MapId.SKY_FERRY],[MapId.FROM_ELLINIA_TO_EREVE, MapId.FROM_EREVE_TO_ELLINIA], 2 * 60),
                // 里恩 - 明珠港
                new PrivateContiMove(channel, "Whale", [MapId.DANGEROUS_FOREST, MapId.LITH_HARBOUR],[MapId.FROM_RIEN_TO_LITH, MapId.FROM_LITH_TO_RIEN], 60) { ArrivePortals = [0, 3]},
                // 天空之城 - 武陵
                new PrivateContiMove(channel, "Crane", [200000141, 250000100],[200090300, 200090310], 60),

                new SoloQuestEventManager(channel, QuestId.Get3rdJobQuest(Job.WARRIOR), 20 * 60, 108010300, 105070001, 108010300, 108010301),
                new SoloQuestEventManager(channel, QuestId.Get3rdJobQuest(Job.MAGICIAN), 20 * 60, 108010200, 100040106, 108010200, 108010201),
                new SoloQuestEventManager(channel, QuestId.Get3rdJobQuest(Job.BOWMAN), 20 * 60, 108010100, 105040305, 108010100, 108010101),
                new SoloQuestEventManager(channel, QuestId.Get3rdJobQuest(Job.THIEF), 20 * 60, 108010400, 107000402, 108010400, 108010401),
                new SoloQuestEventManager(channel, QuestId.Get3rdJobQuest(Job.PIRATE), 20 * 60, 108010500, 105070200, 108010500, 108010501),

                new SoloQuestEventManager(channel, QuestId.Get2ndJobQuest(Job.WARRIOR), 20 * 60, 108000300, 102020300, 108000300, 108000300) { MaxLobbys = 3, ExitPortal = 9 },
                new SoloQuestEventManager(channel, QuestId.Get2ndJobQuest(Job.MAGICIAN), 20 * 60, 108000200, 101020000, 108000200, 108000200) { MaxLobbys = 3, ExitPortal = 9 },
                new SoloQuestEventManager(channel, QuestId.Get2ndJobQuest(Job.BOWMAN), 20 * 60, 108000100, 106010000, 108000100, 108000100) { MaxLobbys = 3, ExitPortal = 9 },
                new SoloQuestEventManager(channel, QuestId.Get2ndJobQuest(Job.THIEF), 20 * 60, 108000400, 102040000, 108000400, 108000400) { MaxLobbys = 3, ExitPortal = 9 },
                new SoloQuestEventManager(channel, 2191, 20 * 60, 108000502, 120000101, 108000502, 108000502) { MaxLobbys = 2 },
                new SoloQuestEventManager(channel, 2192, 20 * 60, 108000501, 120000101, 108000501, 108000501) { MaxLobbys = 2 },
                new SoloQuestEventManager(channel, 3230, 10 * 60, 922000010,221024400,922000010,922000010 ),
                new SoloQuestEventManager(channel, 21301, 10 * 60, 108010700,140020200,108010700,108010700 ){EntryPortal = 1},

                new SoloQuestEventManager(channel, 6108, 30 * 60, 910500000,105090200,910500000,910500000 ){EntryPortal = 1},
                new SoloQuestEventManager(channel, 20718, 10 * 60, 910110000,101000000,910110000,910110000){EntryPortal = 1, ExitPortal = 26},
                new q21401(channel),
                new q21610(channel),
                new q21613(channel),
                new SoloQuestEventManager(channel, 21733, 10 * 60, 910400000,104000004,910400000,910400000),
                new SoloQuestEventManager(channel, 21739, 10 * 60, 920030000,200060000,920030000,920030001) { EntryPortal = 2 },
                new q21747(channel),
                new q2245(channel),
                new SoloQuestEventManager(channel, 2291, 30 * 60, 103040440,103040400,103040410,103040460),
                new SoloQuestEventManager(channel, 3239, 20 * 60, 922000000,922000009,922000000,922000000),
                new q6002(channel),
                new q6330(channel),
                new q6370(channel),

                new RockSpirit(channel),
                new Puppeteer(channel),
                new MK_PrimeMinister(channel),
                ]);
        }

        public async ValueTask DisposeAsync()
        {
            _eventSource.Clear();
            _itemSource.Clear();
            _mapEnterSource.Clear();
            _mapFirstEnterSource.Clear();
            _npcSource.Clear();
            _portalSource.Clear();
            _reactorActSource.Clear();
            _reactorHitSource.Clear();
        }
    }
}
