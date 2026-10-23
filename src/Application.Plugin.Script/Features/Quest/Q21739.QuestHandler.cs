using Application.Core.Client;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q21739QuestHandler : Application.Core.scripting.quest.QuestScriptBase
    {
        public Q21739QuestHandler(IChannelClient c, Application.Core.Server.quest.Quest quest, int npc)
            : base(c, quest, npc)
        {
        }

        // Quest: 21739 
        public async Task q21739e()
        {
            await SayNext("那么，你打败巨人了吗？哦，黑色之翼卧底？他抢走了天空之城的封印石？！哦，不。太可怕了！我们需要尽快想个对策！把情况告诉明珠港的特鲁。");
            await forceCompleteQuest();
            await gainExp(29500);
        }
    }
}
