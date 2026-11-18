
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Quest
{

    // 彩虹岛
    internal partial class QuestScript
    {
        // Quest: 1021 
        public async Task q1021s()
        {
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.A20_WhatsHaha),
                    this.GetTalkMessage(ScriptMessage.A20_AskingMade)
                ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A20_LetJust)))
            {
                if (getPlayer().HP >= 50)
                {
                    await getPlayer().UpdateHP(25);
                }

                if (!haveItem(2010007))
                {
                    await gainItem(2010007, 1);
                }

                await forceStartQuest();
                await SaySpeech([
                    this.GetTalkMessage(ScriptMessage.A20_SurprisedHp),
                        this.GetTalkMessage(ScriptMessage.A20_PleaseTake)
                    ]);
                await ShowEffect("UI/tutorial.img/28");
            }
        }
        // Quest: 1021 
        public async Task q1021e()
        {
            if (getPlayer().HP < 50)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A20_HpFully));
                return;
            }
            else
            {
                await SaySpeech([
                    this.GetTalkMessage(ScriptMessage.A20_EasyConsume),
                        this.GetTalkMessage(ScriptMessage.A20_AlrightHave),
                        this.GetTalkMessage(ScriptMessage.A20_AllTeach)
                    ]);
                if (isQuestCompleted(1021))
                {
                    await this.Popup(this.GetTalkMessage(ScriptMessage.Dojo_UnknownError));
                }
                else if (canHold(2010000) && canHold(2010009))
                {
                    await gainExp(10);
                    await gainItem(2010000, 3);
                    await gainItem(2010009, 3);
                    await forceCompleteQuest();
                }
                else
                {
                    await this.Popup(this.GetTalkMessage(ScriptMessage.A20_InventoryFull));
                }
            }
        }
        // Quest: 1028 
        public Task q1028s()
        {
            // TODO
            return Task.CompletedTask;
        }
        // Quest: 1048 
        public Task q1048s()
        {
            // TODO
            return Task.CompletedTask;
        }
        // Quest: 1049 
        public Task q1049s()
        {
            // TODO
            return Task.CompletedTask;
        }
        // Quest: 1050 
        public Task q1050s()
        {
            // TODO

            return Task.CompletedTask;
        }
        // Quest: 1051 
        public Task q1051s()
        {
            // TODO

            return Task.CompletedTask;
        }
        // Quest: 1052 
        public Task q1052s()
        {
            // TODO

            return Task.CompletedTask;
        }
        // Quest: 1053 
        public Task q1053s()
        {
            // TODO

            return Task.CompletedTask;
        }

    }
}