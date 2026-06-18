namespace Application.Plugin.Script.Quest
{

    // 神木村
    internal partial class QuestScript
    {
        // Quest: 3714 
        public async Task q3714s()
        {
            if (!haveItem(4001094, 1))
            {
                await SayNext("你没有 #b#t4001094##k...");
                return;
            }

            if (haveItem(2041200, 1))
            {
                await SayOK("（自从到达这个地方后，我包里的 #b#t2041200##k 变得更加明亮了... 再次注意到，那边的小龙似乎对它怒视着。）");
                return;
            }

            await SayNext("你带来了一个 #b#t4001094##k，感谢你为我们的巢穴带回了一个同类！请接受这个...\r\n\r\n....... (bleuuhnuhgh) (blahrgngnhhng) ...\r\n\r\n呃，#b#t2041200##k 作为我们同类的感激之情。还有一个请求，请把那个东西带走...");

            if (!canHold(2041200, 1))
            {
                await SayOK("请在消耗栏中腾出空位来领取奖励。");
                return;
            }

            await forceCompleteQuest();
            await gainItem(4001094, -1);
            await gainItem(2041200, 1);    // 任务奖励问题找到并修复感谢 MedicOP & Thora
            await gainExp(42000);
        }

    }
}