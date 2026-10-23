using Application.Core.Client;
using Application.Shared.Constants.Job;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q21401QuestHandler : Application.Core.scripting.quest.QuestScriptBase
    {
        public Q21401QuestHandler(IChannelClient c, Application.Core.Server.quest.Quest quest, int npc)
            : base(c, quest, npc)
        {
        }

        // Quest: 21401 
        public async Task q21401s()
        {
            await SaySpeech([
                "...问我怎么会变成这样？...本来我不太想说的...不，我当然瞒不过主人您了...",
                "...你被封印在冰川里的几百年时间...我也呆在冰川里，没有主人，让我感到非常的孤独...因此心中出现了黑暗。",
                "不过，你重新苏醒后，我心中的黑暗也跟着完全消失了。既然主人回来了，心里也没有什么可难过的了。本以为这样就没事了...没想到这只是我的错觉。"
                ]);
            if (await SayAcceptDecline("拜托你了，战神...请阻止我。能阻止我的暴走的，只有主人你。我已经再也无法抑制了！请你一定要把暴走的我打倒！"))
            {
                await TryStartSoloQuest(getQuest(), forceStartQuest);
            }
        }

        // Quest: 21401 
        public async Task q21401e()
        {
            await SayNext("谢谢你，战神。多亏了你，才阻止了我的暴走。真是万幸……以主人的实力，这点小事当然不在话下了！");
            if (await AskYesNo("现在来看，你的等级已经很高了。既然能够打倒暴走状态下的我……那么唤醒你过去全部的力量也应该是可以的了。"))
            {
                if (!isQuestCompleted(21401))
                {
                    if (!canHold(2280003, 1))
                    {
                        await SayOK("你的#b物品栏#k已满. 请留出多余的空间来获取物品.");
                        return;
                    }

                    await gainItem(2280003, 1);
                    await changeJobById(JobId.ARAN4);

                    await completeQuest();
                }
                await SayNext("沉睡的技能全都唤醒了……毕竟好久没用了，还需要熟悉熟悉。不过，应该进步会很快的。");
            }
        }
    }
}
