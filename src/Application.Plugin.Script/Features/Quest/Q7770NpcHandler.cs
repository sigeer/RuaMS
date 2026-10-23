using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 任务 7770 相关脚本：yurete2_dead。
    /// 该任务在 NPC 与传送门两侧都有判定，故集中在此。
    /// </summary>
    internal sealed class Q7770NpcHandler : NpcScriptBase
    {
        public Q7770NpcHandler(IChannelClient c, int npc, NPC? npcObj) : base(c, npc, npcObj)
        {
        }

        // Npc: 2112011 
        public async Task yurete2_dead()
        {
            await AskMenu(
                "被打败了...这就是犹泰的遗产将如何结束的方式，哦，这是多么的悲哀...希望你们现在很开心，因为我将度过余生在一个黑暗的地窖里。我所做的一切都是为了马加提亚的利益！！（哭泣）",
                ["嘿，伙计，振作点！这里没有太多无法解决的损害。马加提亚制定了这些严厉的法律，是为了保护它的人民免受像这样的强大力量落入错误的手中所带来的危害。这并不是你的终结，接受社会的康复，一切都会好起来的！"]
                );
            await SayNext("…在我所做的一切之后，你们原谅我了吗？嗯，我想我被那种可以通过这种方式发现的巨大力量冲昏了头脑，也许他们说得对，人类不能简单地理解并运用这些力量，而不在途中腐化自己…我深感抱歉，为了弥补自己对每个人，我愿意在炼金术的进展中再次帮助各个组织。谢谢。");
            if (!isQuestCompleted(7770))
            {
                await completeQuest(7770);
            }
            await warp(926110600, 0);
        }
    }
}
