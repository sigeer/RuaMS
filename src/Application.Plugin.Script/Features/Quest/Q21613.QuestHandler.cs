using Application.Core.Client;
using Application.Core.scripting.Infrastructure;
using Application.Shared.Constants;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q21613QuestHandler : Application.Core.scripting.quest.QuestScriptBase
    {
        public Q21613QuestHandler(IChannelClient c, Application.Core.Server.quest.Quest quest, int npc)
            : base(c, quest, npc)
        {
        }

        // Quest: 21613 
        public async Task q21613s()
        {
            await SaySpeech([
                    new SpeechText("我们是一群狼，在寻找我们丢失的孩子。我听说你在照顾我们的孩子。我们很感激你的好意，但现在是时候把孩子还给我们了.", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face),
                    new SpeechText("狼人也是我的朋友，我不能就这样放弃一个朋友", NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd),
                ]);
            if (await SayAcceptDecline("我们明白，但我们不会丢下我们的小狗离开。这样吧，我们来考验你，看看你是否有资格养一只狼。#r狼的考试#k"))
            {
                await TryStartSoloQuest(getQuest(), forceStartQuest);
            }
        }
    }
}
