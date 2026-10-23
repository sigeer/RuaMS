using Application.Core.Client;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q2191QuestHandler : Application.Core.scripting.quest.QuestScriptBase
    {
        public Q2191QuestHandler(IChannelClient c, Application.Core.Server.quest.Quest quest, int npc)
            : base(c, quest, npc)
        {
        }

        // Quest: 2191
        /// <summary>拳手转职：送到考场。</summary>
        public async Task q2191s()
        {
            if (!await SayAcceptDecline("已经到了等级30了啊。为了当上拳手而来找我的吗？那么你还真是找对人了。"))
            {
                await SayOK("你不想当拳手？");
                return;
            }

            await SayNext("但我不能直接就让你转职，你要向我见证你的力量和强壮。拳手是一种利用全身和拳头来击退敌人的海盗。所以想做一个优秀的拳手的话要会好好利用拳头才行。一定要记住这一点。");
            await SayNext("见证的方法很简单。我把你送到考试的地方，你在那里好好对抗#r附子章鱼#k并收集15个#b强大力量的结晶#k带回来，你觉得很简单？不要这么轻敌！他们只能用单体攻击，你一定要记住。其他的技巧或者攻击都没有作用。可能有点难了，但为了成为一个优秀的拳手还是要好好拿出精神来大干一场...");
            await startQuest();
            await warp(108000502, 0);
        }
    }
}
