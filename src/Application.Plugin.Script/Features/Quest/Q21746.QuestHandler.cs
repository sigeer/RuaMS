using Application.Core.Client;
using Application.Plugin.Script;
using Application.Resources.Messages;

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
            await SayNext(this.GetTalkMessage(ScriptMessage.Q21746_SealStone));

            await TryStartSoloQuest(getQuest(), forceStartQuest);
        }
    }
}
