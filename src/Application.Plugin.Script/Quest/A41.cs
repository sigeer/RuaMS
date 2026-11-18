
using Application.Plugin.Script;
using Application.Resources.Messages;

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
                await SayNext(this.GetTalkMessage(ScriptMessage.Npc3714_NoItem));
                return;
            }

            if (haveItem(2041200, 1))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc3714_AlreadyHaveCharm));
                return;
            }

            await SayNext(this.GetTalkMessage(ScriptMessage.Npc3714_ThanksNext));

            if (!canHold(2041200, 1))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc3714_NoUseSlot));
                return;
            }

            await forceCompleteQuest();
            await gainItem(4001094, -1);
            await gainItem(2041200, 1);    // 任务奖励问题找到并修复感谢 MedicOP & Thora
            await gainExp(42000);
        }

    }
}