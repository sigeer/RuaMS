using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Gameplay.Plugins;
using Application.Core.scripting.Infrastructure;
using Application.Plugin.Script.Npc;
using Application.Plugin.Script.Quest;
using Application.Utility.Exceptions;
using scripting.quest;
using Serilog;
using server.maps;
using System.Reflection;
using tools;

namespace Application.Plugin.Script
{
    internal class ScriptService : IScriptPortalService,
        IScriptNpcService,
        IScriptQuestService,
        IScriptItemService,
        IScriptMapService,
        IScriptReactorService
    {
        Dictionary<string, MethodInfo> _portalSource;
        Dictionary<string, MethodInfo> _npcSource;
        Dictionary<string, MethodInfo> _itemSource;
        Dictionary<string, MethodInfo> _mapEnterSource;
        Dictionary<string, MethodInfo> _mapFirstEnterSource;
        Dictionary<string, MethodInfo> _reactorHitSource;
        Dictionary<string, MethodInfo> _reactorActSource;
        Dictionary<string, MethodInfo> _reactorTouchSource;
        Dictionary<string, MethodInfo> _reactorUntouchSource;
        Dictionary<string, MethodInfo> _questSource;

        public ScriptService()
        {
            _portalSource = TypeUtils.ExtractMethodsToDictionary(typeof(PortalScript));
            _npcSource = TypeUtils.ExtractMethodsToDictionary(typeof(NpcScript));
            _itemSource = TypeUtils.ExtractMethodsToDictionary(typeof(ItemScript));

            _mapEnterSource = TypeUtils.ExtractMethodsToDictionary(typeof(MapEnterScript));
            _mapFirstEnterSource = TypeUtils.ExtractMethodsToDictionary(typeof(MapFirstEnterScript));

            _reactorHitSource = TypeUtils.ExtractMethodsToDictionary(typeof(ReactorHitScript));
            _reactorActSource = TypeUtils.ExtractMethodsToDictionary(typeof(ReactorActScript));
            _reactorTouchSource = TypeUtils.ExtractMethodsToDictionary(typeof(ReactorTouchScript));
            _reactorUntouchSource = TypeUtils.ExtractMethodsToDictionary(typeof(ReactorUntouchScript));

            _questSource = TypeUtils.ExtractMethodsToDictionary(typeof(QuestScript));
        }


        public async Task<bool> Enter(IChannelClient c, Portal p)
        {
            if (!_portalSource.TryGetValue(p.getScriptName()!, out var methodInfo))
            {
                // 
                throw new BusinessNotsupportException($"PortalScript: {p.getScriptName()}");
            }

            var script = new PortalScript(c, p);
            return await (Task<bool>)methodInfo.Invoke(script, null)!;
        }

        public async Task<bool> Start(IChannelClient c, int npcId, NPC? npcObject, string? scriptName)
        {
            if (c.NPCConversationManager != null)
            {
                await c.OnlinedCharacter.Pink("卡对话了");
                return false;
            }

            if (string.IsNullOrEmpty(scriptName))
            {
                scriptName = $"n{npcId}";
            }

            if (!c.canClickNPC())
            {
                await c.OnlinedCharacter.Pink("对话太过频繁");
                return false;
            }

            if (!_npcSource.TryGetValue(scriptName, out var methodInfo))
            {
                await c.OnlinedCharacter.Debug(5, $"不支持的脚本 {scriptName}");
                return false;
            }


            await using var talk = new NpcScript(c, npcId, npcObject);
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
                    await talk.WarpOut();
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
        }

        public Task<bool> StartQuest(IChannelClient c, server.quest.Quest questObj, int npcId) => HandleQuestScript(c, questObj, npcId, true);
        public Task<bool> CompleteQuest(IChannelClient c, server.quest.Quest questObj, int npcId) => HandleQuestScript(c, questObj, npcId, false);


        async Task<bool> HandleQuestScript(IChannelClient c, server.quest.Quest questObj, int npcId, bool isStarted)
        {
            if (c.NPCConversationManager != null)
            {
                await c.OnlinedCharacter.Pink("卡对话了");
                return false;
            }

            if (!c.canClickNPC())
            {
                await c.OnlinedCharacter.Pink("对话太过频繁");
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
                await c.OnlinedCharacter.Pink($"不支持的脚本 {scriptName}");
                return false;
            }

            if (c.NPCConversationManager != null)
            {
                return false;
            }


            await using var talk = new QuestScript(c, questObj, npcId);
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
                    await talk.WarpOut();
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
        }

        public async Task Action(IChannelClient c, sbyte mode, sbyte type, int selection, string? inputText = null)
        {
            if (c.NPCConversationManager != null)
            {
                await c.NPCConversationManager.Response(mode, type, selection, inputText);
            }
        }

        public async Task ItemScript(IChannelClient c, int npcId, string scriptName)
        {
            if (!_itemSource.TryGetValue(scriptName, out var methodInfo))
            {
                throw new BusinessScriptNotFoundException($"不支持的脚本 {scriptName}");
            }

            await using var talk = new ItemScript(c, npcId);
            c.NPCConversationManager = talk;
            await (Task)methodInfo.Invoke(talk, null)!;
        }

        public async Task MapFirstEnter(IChannelClient c, IMap map)
        {
            if (!_mapFirstEnterSource.TryGetValue(map.SourceTemplate.OnFirstUserEnter, out var methodInfo))
            {
                throw new BusinessScriptNotFoundException($"不支持的脚本 {map.SourceTemplate.OnFirstUserEnter}");
            }

            var script = new MapFirstEnterScript(c, map);
            await (Task)methodInfo.Invoke(script, null)!;
        }

        public async Task MapEnter(IChannelClient c, IMap map)
        {
            if (!_mapEnterSource.TryGetValue(map.SourceTemplate.OnUserEnter, out var methodInfo))
            {
                throw new BusinessScriptNotFoundException($"不支持的脚本 {map.SourceTemplate.OnUserEnter}");
            }

            var script = new MapEnterScript(c, map);
            await(Task)methodInfo.Invoke(script, null)!;
        }
        public async Task ReactorHit(IChannelClient c, Reactor r)
        {
            if (!_reactorHitSource.TryGetValue(r.getStats().Action, out var methodInfo))
            {
                throw new BusinessScriptNotFoundException($"不支持的脚本 {r.getStats().Action}");
            }

            var script = new ReactorHitScript(c, r);
            await (Task)methodInfo.Invoke(script, null)!;
        }

        public async Task ReactorAct(IChannelClient c, Reactor r)
        {
            if (!_reactorActSource.TryGetValue(r.getStats().Action, out var methodInfo))
            {
                throw new BusinessScriptNotFoundException($"不支持的脚本 {r.getStats().Action}");
            }

            var script = new ReactorActScript(c, r);
            await (Task)methodInfo.Invoke(script, null)!;
        }

        public async Task ReactorTouch(IChannelClient c, Reactor r)
        {
            if (!_reactorTouchSource.TryGetValue(r.getStats()!.Action, out var methodInfo))
            {
                throw new BusinessScriptNotFoundException($"不支持的脚本 {r.getStats().Action}");
            }

            var script = new ReactorTouchScript(c, r);
            await (Task)methodInfo.Invoke(script, null)!;
        }

        public async Task ReactorUntouch(IChannelClient c, Reactor r)
        {
            if (!_reactorUntouchSource.TryGetValue(r.getStats().Action, out var methodInfo))
            {
                throw new BusinessScriptNotFoundException($"不支持的脚本 {r.getStats().Action}");
            }

            var script = new ReactorUntouchScript(c, r);
            await (Task)methodInfo.Invoke(script, null)!;
        }
        public async ValueTask DisposeAsync()
        {
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
