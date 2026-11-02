using Application.Core.Client;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q21747QuestHandler : Application.Core.scripting.quest.QuestScriptBase
    {
        public Q21747QuestHandler(IChannelClient c, Application.Core.Server.quest.Quest quest, int npc)
            : base(c, quest, npc)
        {
        }

        // Quest: 21747 
        public async Task q21747s()
        {
            if (await SayAcceptDecline("没想到在数百年的岁月之后，英雄的后裔又重新出现了......也不知道对冒险岛世界是福还是祸......怎样都无所谓了。好吧......我告诉你有关武陵封印石的事情。"))
            {
                await SayNext("武陵的封印石所在的地方叫做封印的寺院。那里的入口被隐藏在武陵寺院内。你去仔细观察武陵寺院入口处熊猫提着的灯盏。如果能从中找出#b刻有入口字样的灯盏#k，就可以进入封印的寺院了。暗号是#b道可道非常道。#k");
                await forceStartQuest();
            }

        }

        // Quest: 21747 
        public async Task q21747e()
        {
            await SayNext("成功打败了影子武士吗？表情怎么这么凝重......难道说你失败了......");
            await SayNext("原来是这样，武陵的封印石最终还是被抢走了......很遗憾，不过也没办法。我现在也不明白英雄们为什么要把封印石交给武陵。");
            await gainExp(16000);
            await forceCompleteQuest();
        }
    }
}
