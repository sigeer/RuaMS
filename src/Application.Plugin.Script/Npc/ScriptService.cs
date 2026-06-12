using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.Gameplay.Plugins;
using Application.Core.scripting.Infrastructure;
using scripting.quest;
using Serilog;
using System.Reflection;
using tools;

namespace Application.Plugin.Script.Events
{
    internal class ScriptService : IScriptNpcService
    {
        Dictionary<string, MethodInfo> _npcSource;

        public ScriptService()
        {
            _npcSource = TypeUtils.ExtractMethodsToDictionary(typeof(NpcScript));
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


        public async ValueTask DisposeAsync()
        {
            _npcSource.Clear();
        }
    }
}
