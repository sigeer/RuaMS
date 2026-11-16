using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 任务 3367 相关脚本：q3367npc。
    /// 该任务在 NPC 与传送门两侧都有判定，故集中在此。
    /// </summary>
    internal sealed class Q3367NpcHandler : NpcScriptBase
    {
        public Q3367NpcHandler(IChannelClient c, int npc, NPC? npcObj) : base(c, npc, npcObj)
        {
        }

        // Npc: 2112016 
        public async Task q3367npc()
        {
            if (isQuestStarted(3367))
            {
                var c = getQuestProgressInt(3367, 30);
                if (c >= 30)
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Q3367_AllOrganized));
                    return;
                }

                var book = (getNpcObjectId() % 30);
                var prog = getQuestProgressInt(3367, book);
                if (prog == 0)
                {
                    c++;

                    if (book < 20)
                    {
                        if (!canHold(4031797, 1))
                        {
                            await SayNext(this.GetTalkMessage(ScriptMessage.Q3367_EtcFull));
                            return;
                        }
                        else
                        {
                            await gainItem(4031797, 1);
                            await setQuestProgress(3367, 31, getQuestProgressInt(3367, 31) + 1);
                        }
                    }

                    await setQuestProgress(3367, book, 1);
                    await setQuestProgress(3367, 30, c);
                    await SayNext(this.GetTalkMessage(ScriptMessage.Q3367_Organized, 30 - c));
                }
            }
        }
    }
}
