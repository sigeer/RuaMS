using Application.Core.Client;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q21746QuestHandler : Application.Core.scripting.quest.QuestScriptBase
    {
        public Q21746QuestHandler(IChannelClient c, Application.Core.Server.quest.Quest quest, int npc)
            : base(c, quest, npc)
        {
        }

        // Quest: 21746 
        public async Task q21746s()
        {
            await SayNext("封印石......那是很久很久以前，由武陵看管的东西......难道说觊觎它的人又出现了......");

            await TryStartSoloQuest(getQuest(), forceStartQuest);
        }
    }
}
