using Application.Core.Client;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q2291QuestHandler : Application.Core.scripting.quest.QuestScriptBase
    {
        public Q2291QuestHandler(IChannelClient c, Application.Core.Server.quest.Quest quest, int npc)
            : base(c, quest, npc)
        {
        }

        // Quest: 2291 
        public async Task q2291e()
        {
            if (!haveItem(4032521, 10))
            {
                await SayNext("嘿，你还没有得到#b10张#t4032521##k吗？");
                return;
            }
            else
            {

                await SayNext("你身上有#b#i4032521##k，很好，让我给你带路。");

                var em = GetSoloQuestEventManager(Q2291.QuestIdValue);
                if (await em.StartInstance(getPlayer()) != Core.scripting.Events.Abstraction.CreateInstanceResult.Success)
                {
                    await SayOK("嗯...看起来前面的房间现在有点拥挤。请在这儿等一会，好吗？");
                    return;
                }

                await gainItem(4032521, -10);
                await forceCompleteQuest();
            }
        }
    }
}
