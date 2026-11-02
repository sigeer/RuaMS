using Application.Core.Client;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q2245QuestHandler : Application.Core.scripting.quest.QuestScriptBase
    {
        public Q2245QuestHandler(IChannelClient c, Application.Core.Server.quest.Quest quest, int npc)
            : base(c, quest, npc)
        {
        }

        // Quest: 2245 
        public async Task q2245s()
        {
            await TryStartSoloQuest(getQuest(), forceStartQuest);
        }
    }
}
