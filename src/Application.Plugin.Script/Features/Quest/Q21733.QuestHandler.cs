using Application.Core.Client;
using Application.Core.scripting.Infrastructure;
using Application.Core.scripting.quest;
using Application.Shared.Constants;
using Application.Shared.Constants.Skill;
// 外层命名空间 Features 下有 Features.Aran，裸写 Aran 会先绑定到它而非技能常量类。
using SkillAran = Application.Shared.Constants.Skill.Aran;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q21733QuestHandler : QuestScriptBase
    {
        public Q21733QuestHandler(IChannelClient c, Application.Core.Server.quest.Quest quest, int npc) : base(c, quest, npc)
        {
        }

        // Quest: 21733 
        public async Task q21733s()
        {
            await AskMenu("喂，你在哪？有一件急事！\r\n#b#L0#(喂……？#p1002104#以前不是叫我英雄的吗……)#l\r\n#k");
            if (await SayAcceptDecline("我有很重要的情报！赶紧到#b#m104000004##k来！"))
            {
                await forceStartQuest();
            }
            else
            {
                await SayOK("你说什么呢？很着急！别废话赶紧来！");
            }
        }
        // Quest: 21733 
        public async Task q21733e()
        {
            await SaySpeech([
                    new SpeechText("啊……没想到还会碰上这种事情。怎么都没想到人偶师还会潜伏到这里来。平时大概是疏于修炼了，完全被对方给算计了。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("对不起，都是因为我……", NpcTalkSpeaker.PlayerRight),
                    new SpeechText("啊？英雄大人不必内疚。你也不知道那家伙会来啊。不必道歉。不过，这也暴露出了他们的弱点。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("弱点？", NpcTalkSpeaker.PlayerRight),
                    new SpeechText("人偶师讨厌的那个文件。如果那个文件是假的，人偶师是不会这么兴师动众，带着一群人跑来折腾的。那个文件充分证明了黑色之翼的目标其实是金银岛封印石。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("话虽这么说，但我的位置也暴露了。", NpcTalkSpeaker.PlayerRight),
                    ]);
            if (await AskYesNo("别担心。这次我为了拿利琳寄过来的文件才出去的，没想到中了敌人的招。平时我不会这么不小心的。好歹也是个情报商人，总会为自己准备一条退路的。现在关键的是#b精准矛#k这个技能你知道吗？"))
            {
                await gainExp(8000);
                await teachSkill(SkillAran.POLEARM_MASTERY, 0, 20, -1); // polearm mastery

                await forceCompleteQuest();

                await SayOK("黑色之翼再怎么兴风作浪也没法阻止你日益强大起来。继续努力，直到击败黑魔法师为止。我也会尽最大努力为你收集信息的。");
            }
        }
    }
}
