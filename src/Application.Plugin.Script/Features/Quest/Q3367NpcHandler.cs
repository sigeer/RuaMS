using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;

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
                    await SayNext("（所有文件已整理好。向尤莱特报告找到的文件。）");
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
                            await SayNext("（你找到了一份报告文件，但由于你的其他栏已满，你选择将文件放在你找到的地方。）");
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
                    await SayNext("（整理文件。#r" + (30 - c) + "#k 剩余。）");
                }
            }
        }
    }
}
