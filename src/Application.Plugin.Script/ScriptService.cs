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

            _questSource = TypeUtils.ExtractMethodsToDictionary(typeof(QuestScript));
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
