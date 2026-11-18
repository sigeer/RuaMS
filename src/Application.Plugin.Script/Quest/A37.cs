using Application.Shared.Constants.Inventory;
using Application.Shared.Constants.Job;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Quest
{

    // 时间停止之湖
    internal partial class QuestScript
    {

        // Quest: 3414 
        public async Task q3414e()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A37_WhoaSample));

            if (getPlayer().getInventory(InventoryType.USE).getNumFreeSlot() < 1)
            {
                await this.Popup(this.GetTalkMessage(ScriptMessage.A37_UseInventory));
                return;
            }

            var talkStr = this.GetTalkMessage(ScriptMessage.A37_PleaseSelect);
            var stance = getPlayer().getJobStyle();
            int[] vecItem;
            if (stance == Job.WARRIOR || stance == Job.BEGINNER)
            {
                vecItem = [2043002, 2043102, 2043202, 2044002, 2044102, 2044202, 2044402, 2044302];
            }
            else if (stance == Job.MAGICIAN)
            {
                vecItem = [2043702, 2043802];
            }
            else if (stance == Job.BOWMAN || stance == Job.CROSSBOWMAN)
            {
                vecItem = [2044502, 2044602];
            }
            else if (stance == Job.THIEF)
            {
                vecItem = [2043302, 2044702];
            }
            else
            {
                vecItem = [2044802, 2044902];
            }

            for (var i = 0; i < vecItem.Length; i++)
            {
                talkStr += "\r\n#L" + i + "# #i" + vecItem[i] + "# #t" + vecItem[i] + "#";
            }
            var selection = await AskMenu(talkStr);
            var item = vecItem[selection];
            await gainItem(item, 1);
            await gainItem(4031103, -1);
            await gainItem(4031104, -1);
            await gainItem(4031105, -1);
            await gainItem(4031106, -1);
            await gainExp(12000);

            await completeQuest();
        }
        // Quest: 3437 
        public async Task q3437e()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A37_TellingYouve));

            if (getPlayer().getInventory(InventoryType.EQUIP).getNumFreeSlot() < 1)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A37_PleaseFree));
                return;
            }

            var talkStr = this.GetTalkMessage(ScriptMessage.A37_LikeGlove);
            var stance = getPlayer().getJobStyle();
            int item;
            if (stance == Job.WARRIOR)
            {
                item = 1082024;
            }
            else if (stance == Job.MAGICIAN)
            {
                item = 1082063;
            }
            else if (stance == Job.BOWMAN || stance == Job.CROSSBOWMAN)
            {
                item = 1082072;
            }
            else if (stance == Job.THIEF)
            {
                item = 1082076;
            }
            else if (stance == Job.BRAWLER || stance == Job.GUNSLINGER)
            {
                item = 1082195;
            }
            else
            {
                item = 1082149;
            }

            await SayNext(talkStr);
            await completeQuest();
            await gainItem(item, 1);
            await gainItem(4000122, -120);
            await gainExp(6100);
            await SayOK(this.GetTalkMessage(ScriptMessage.A37_ThankMuch));
        }
        // Quest: 3452 
        public async Task q3452e()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A37_TakeMana));
            if (canHold(2000011, 50))
            {
                await gainItem(4000099, -1);
                await gainItem(2000011, 50);
                await gainExp(8000);

                await forceCompleteQuest();
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A37_HmLooks));
            }
        }
        // Quest: 3454 
        public async Task q3454e()
        {
            if (getPlayer().getInventory(InventoryType.ETC).getNumFreeSlot() < 1)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A37_MakeRoom));
                return;
            }

            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.A37_AllMaterials),
                this.GetTalkMessage(ScriptMessage.A37_ClankClank),
                this.GetTalkMessage(ScriptMessage.A37_ClangClang),
                this.GetTalkMessage(ScriptMessage.A37_Boom)
                ]);

            await gainItem(4000125, -1);
            await gainItem(4031926, -10);
            await gainItem(4000119, -30);
            await gainItem(4000118, -30);

            await gainItem(4031928, 1);//
            // gainItem(4031927, 1);

            await forceCompleteQuest();
            await SayOK(this.GetTalkMessage(ScriptMessage.A37_DecoderDone));
        }
        // Quest: 7103 
        public async Task q7103s()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.A37_OnlyThing)))
            {
                await SaySpeech([
                    this.GetTalkMessage(ScriptMessage.A37_IllExplain),
                    this.GetTalkMessage(ScriptMessage.A37_EnterRoom)
                    ]);

                if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A37_DropReturned)))
                {
                    if (!haveItem(4031179, 1))
                    {
                        if (!canHold(4031179, 1))
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.A37_PleaseHave));
                            return;
                        }

                        await gainItem(4031179, 1);
                    }
                    await forceStartQuest();
                }
            }
        }

    }
}