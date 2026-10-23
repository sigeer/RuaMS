using Application.Core.Client;
using Application.Core.scripting.Infrastructure;
using Application.Shared.Constants;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q21301QuestHandler : Application.Core.scripting.quest.QuestScriptBase
    {
        public Q21301QuestHandler(IChannelClient c, Application.Core.Server.quest.Quest quest, int npc)
            : base(c, quest, npc)
        {
        }

        // Quest: 21301 
        public async Task q21301e()
        {
            if (await AskYesNo("抓到#o9001013#了吗？呵呵呵……真不愧是我的主人！很好，把你找到的#t4032312#给我吧……啊……？怎么不说话？难道……你没找到？"))
            {
                await SaySpeech([
                    new SpeechText("什么？！没找到#t4032312#？怎么会这样？！你忘了吗？啊，怎么……再怎么被黑魔法师诅咒，再怎么被封冻几百年也不能让你变得这么笨啊……", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("不行。我不能绝望。主人糊里糊涂，我可不能糊涂……深呼吸……深呼吸……", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("再去找找，反正小偷已经逃走。看来必须重新做#t4032312#了。以前也做过一次，你知道需要哪些材料吧？快去搜集材料……", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("   #i4001173#", 0),
                    new SpeechText("……彻底没希望了。啊啊啊！", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("#b（#p1201002#正在气头上。先撤再说。说不定#p1201000#能给我什么帮助。）", NpcTalkSpeaker.PlayerRight),
                ]);

                await completeQuest();
            }
        }
    }
}
