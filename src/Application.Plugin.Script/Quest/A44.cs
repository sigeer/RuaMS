using Application.Shared.Constants.Inventory;
using Application.Shared.GameProps;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Quest
{

    // 武陵桃园+尼哈沙漠
    internal partial class QuestScript
    {
        // Quest: 2257 
        public async Task q2257e()
        {
            await forceCompleteQuest();

            await SayNext(this.GetTalkMessage(ScriptMessage.A44_WantRide));
        }
        // Quest: 2258 
        public async Task q2258s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A44_MeerkatsSpreads)))
            {
                await forceStartQuest();

                await SayNext(this.GetTalkMessage(ScriptMessage.A44_GreatHave));
            }
        }
        // Quest: 2258 
        public async Task q2258e()
        {
            await forceCompleteQuest();
            await SayNext(this.GetTalkMessage(ScriptMessage.A44_MeerkatsAround));
        }
        // Quest: 2259 
        public async Task q2259s()
        {
            await forceStartQuest();

            await SayNext(this.GetTalkMessage(ScriptMessage.A44_MeetInformation));
        }
        // Quest: 2259 
        public async Task q2259e()
        {
            if (getMapId() == 260020000)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A44_EhYoure));
                return;
            }

            await forceCompleteQuest();
            await SayNext(this.GetTalkMessage(ScriptMessage.A44_TherereMeerkats));
        }
        // Quest: 2260 
        public async Task q2260s()
        {
            await forceStartQuest();
            await SayNext(this.GetTalkMessage(ScriptMessage.A44_OnceYouve));
        }
        // Quest: 2260 
        public async Task q2260e()
        {
            if (getPlayer().getJob().Rank == 1)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A44_EhDidnt));
                return;
            }

            await forceCompleteQuest();
            await SayNext(this.GetTalkMessage(ScriptMessage.A44_SeemReady));
        }

        List<int> getOreArray()
        {
            List<int> ores = [];
            var y = 0;
            for (var x = 4020000; x <= 4020008; x++)
            {
                if (haveItem(x, 2))
                {
                    ores.Add(x);
                }
            }
            return ores;
        }
        string getOreString(List<int> ids)
        { // Parameter 'ids' is just the array of getOreArray()
            var thestring = "#b";
            string extra;
            for (int x = 0; x < ids.Count; x++)
            {
                extra = "#L" + x + "##t" + ids[x] + "##l\r\n";
                thestring += extra;
            }
            thestring += "#k";
            return thestring;
        }
        // Quest: 3301 
        public async Task q3301e()
        {
            var oreArray = getOreArray();
            if (oreArray.Count > 0)
            {
                var selection = await AskMenu(this.GetTalkMessage(ScriptMessage.A44_LooksLike) + getOreString(oreArray));

                if (!haveItem(oreArray[selection], 2))
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.A44_DontHave));

                    return;
                }

                await gainItem(oreArray[selection], -2); // Take 2 ores
                await forceCompleteQuest();

                await SayNext(this.GetTalkMessage(ScriptMessage.A44_WaitAwhile));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A44_WhatsHavent));
                return;

            }
        }
        // Quest: 3303 
        public async Task q3303e()
        {
            var oreArray = getOreArray();
            if (oreArray.Count > 0)
            {
                var selection = await AskMenu(this.GetTalkMessage(ScriptMessage.A44_LooksLikeSomeones) + getOreString(oreArray));
                if (!haveItem(oreArray[selection], 2))
                {

                    await SayNext(this.GetTalkMessage(ScriptMessage.A44_WhatsHaventGot));

                    return;
                }

                await gainItem(oreArray[selection], -2); // Take 2 ores
                await forceCompleteQuest();

                await SayNext(this.GetTalkMessage(ScriptMessage.A44_WaitAwhile));

            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A44_DontHaveJewel));    // script would loop undefinitely at completion, thanks IxianMace for noticing
            }
        }
        // Quest: 3305 
        public async Task q3305s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A44_HaveLost));
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A44_MakeNew)))
            {
                await forceStartQuest();

                await SayOK(this.GetTalkMessage(ScriptMessage.A44_ComeBack));
            }
        }
        // Quest: 3306 
        public async Task q3306s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A44_HaveLostAlcadno));
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A44_MakeNewCape)))
            {
                await forceStartQuest();

                await SayOK(this.GetTalkMessage(ScriptMessage.A44_ComeBack));
            }
        }
        // Quest: 3314 
        public async Task q3314e()
        {
            if (getPlayer().getBuffSource(BuffStat.HPREC) == 2022198)
            {
                if (canHoldAll([2050004, 2022224], [10, 20]))
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.A44_TakeExperiments));

                    await gainExp(12500);
                    await gainItem(2050004, 10);

                    var i = Random.Shared.Next(5);
                    await gainItem(2022224 + i, 10);

                    await forceCompleteQuest();
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.A44_HuhInventory));
                }
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A44_SeemPretty));
            }
        }
        // Quest: 3320 
        public async Task q3320s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A44_HaveIdea)))
            {
                await forceStartQuest();
                await warp(926120200, 1);
            }
        }
        // Quest: 3321 
        public async Task q3321s()
        {
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.A44_KnowDr),
                this.GetTalkMessage(ScriptMessage.A44_HuroidsCreation)
                ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A44_EvenStopped)))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A44_GratitudeUnderstanding));
                await SayNext(this.GetTalkMessage(ScriptMessage.A44_HavePersonal));
                await SayNext(this.GetTalkMessage(ScriptMessage.A44_RememberOrder, this.GetTalkMessage(ScriptMessage.Npc2111017_Password)));

                await forceStartQuest();
            }

        }
        // Quest: 3353 
        public async Task q3353s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A44_SeeDe));
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A44_CaseGo)))
            {
                await forceStartQuest();
                await warp(926120200, 1);
            }
        }
        // Quest: 3354 
        public async Task q3354s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A44_HaveRequest)))
            {
                await forceStartQuest();
            }
        }
        // Quest: 3360 
        public async Task q3360s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A44_FinallyHave));
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A44_AllRight)))
            {
                var pass = string.Join("", "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".OrderBy(x => Guid.NewGuid()).Take(10));
                await SayOK(this.GetTalkMessage(ScriptMessage.A44_3360_KeyCode, pass));

                await forceStartQuest();
                await setQuestProgress(3360, pass);
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A44_ComeHurry));
            }
        }
        // Quest: 3382 
        public async Task q3382e()
        {
            if (haveItem(4001159, 25) && haveItem(4001160, 25) && !haveItemWithId(1122010, true))
            {
                if (canHold(1122010))
                {
                    await gainItem(4001159, -25);
                    await gainItem(4001160, -25);
                    await gainItem(1122010, 1);
                    await forceCompleteQuest();

                    await SayOK(this.GetTalkMessage(ScriptMessage.A44_ThankRetrieving));
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.A44_FreeSlot));
                    return;
                }
            }
            else if (haveItem(4001159, 10) && haveItem(4001160, 10))
            {
                if (canHold(2041212))
                {
                    await gainItem(4001159, -10);
                    await gainItem(4001160, -10);
                    await gainItem(2041212, 1);
                    await forceCompleteQuest();

                    await SayOK(this.GetTalkMessage(ScriptMessage.A44_ThankRetrievingMarbles));
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.A44_FreeSlotUse));
                    return;
                }
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A44_NeedLeast));
                return;
            }
        }
        // Quest: 3833 
        public async Task q3833e()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.A44_GreatManaged));
            if (getPlayer().getInventory(InventoryType.USE).getNumFreeSlot() >= 2)
            {
                if (haveItem(4000294, 1000))
                {
                    await gainItem(4000294, -1000);
                    await gainItem(2040501, 1);
                    await gainItem(2000005, 50);
                    await gainExp(54000);
                    await forceCompleteQuest();
                }
                else if (haveItem(4000294, 600))
                {
                    await gainItem(4000294, -600);
                    await gainItem(2020013, 50);
                    await gainExp(54000);
                    await forceCompleteQuest();
                }
                else if (haveItem(4000294, 500))
                {
                    await gainItem(4000294, -500);
                    await gainExp(54000);
                    await forceCompleteQuest();
                }
                else if (haveItem(4000294, 100))
                {
                    await gainItem(4000294, -100);
                    await gainExp(45000);
                    await forceCompleteQuest();
                }
                else if (haveItem(4000294, 50))
                {
                    await gainItem(4000294, -50);
                    await gainItem(2020007, 50);
                    await gainExp(10000);
                    await forceCompleteQuest();
                }
                else if (haveItem(4000294, 1))
                {
                    await gainItem(4000294, -1);
                    await gainItem(2000000, 1);
                    await gainExp(10);
                    await forceCompleteQuest();
                }

            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A44_MakeSlots));
            }
        }

        // Quest: 3941 
        public async Task q3941s()
        {
            if (getPlayer().getBuffSource(BuffStat.MORPH) != 2210005)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A44_WhatsCant));
                return;
            }

            await forceStartQuest();
        }
        // Quest: 3941 
        public async Task q3941e()
        {
            if (getPlayer().getBuffSource(BuffStat.MORPH) != 2210005)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A44_WhatsCant));
                return;
            }

            if (canHold(4031571, 1))
            {
                await gainItem(4031571);

                await SayNext(this.GetTalkMessage(ScriptMessage.A44_GoPlease));
                await forceCompleteQuest();
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A44_YoureLacking));
            }
        }
        // Quest: 3953 
        public async Task q3953e()
        {
            await AskMenu(this.GetTalkMessage(ScriptMessage.A44_EhStill));
            await AskMenu(this.GetTalkMessage(ScriptMessage.A44_WonderMerchants));
            await AskMenu(this.GetTalkMessage(ScriptMessage.A44_EhhYeah));
            await AskMenu(this.GetTalkMessage(ScriptMessage.A44_HaveDeparted));

            await forceCompleteQuest();
            await gainItem(4011008, -1);
            await gainExp(20000);

            await SayNext(this.GetTalkMessage(ScriptMessage.A44_WereGreat));
        }

    }
}