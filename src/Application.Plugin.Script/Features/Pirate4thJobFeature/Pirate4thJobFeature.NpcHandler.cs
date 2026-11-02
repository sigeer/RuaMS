using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;

namespace Application.Plugin.Script.Features.Pirate4thJobFeature
{
    internal sealed class Pirate4thJobFeatureNpcHandler : NpcScriptBase
    {
        public Pirate4thJobFeatureNpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 1092019 
        public async Task s4strike()
        {
            if (!isQuestStarted(6400))
            {
                await SayOK(GetDefaultTalk(6400));
            }
            else
            {
                var seagullProgress = getQuestProgressInt(6400, 1);
                if (seagullProgress == 0)
                {
                    string[] seagullQuestion = [
                    "One day, I went to the ocean and caught 62 Octopi for dinner. But then some kid came by and gave me 10 Octopi as a gift! How many Octopi do I have then, in total?"
                    ];
                    string[] seagullAnswer = ["72"];
                    await SayNext("好的！我现在就给你第一个问题！你最好准备好，因为这个问题很难。甚至这里的海鸥都觉得这个问题相当棘手。这是一个相当困难的问题。");
                    for (int i = 0; i < seagullQuestion.Length; i++)
                    {
                        var inputAnswer = await AskText(seagullQuestion[i]);
                        if (inputAnswer == seagullAnswer[i])
                        {
                            continue;
                        }
                        else
                        {
                            await SayOK("嗯，那不太符合我的记忆。再试一次吧！");
                            return;
                        }
                    }
                    await setQuestProgress(6400, 1, 1);
                    await SayNext("什么！我简直不敢相信你有多聪明！不可思议！在海鸥世界里，这种智慧会让你获得博士学位，甚至更多。你真是太了不起了……我简直不敢相信……我简直不敢相信！");
                    return;
                }
                else if (seagullProgress == 1)
                {
                    await SaySpeech([
                        "现在~让我们继续下一个问题。这个问题真的很难。我要让巴特来帮我。你认识巴特，对吧？",
                        "我要把你送到诺特勒斯号的一个空房间里。你会在那里看到9个巴特。哈哈哈~他们是双胞胎吗？不，不，当然不是。我在这个意志测试中使用了一点魔法。",
                        "无论如何，9个巴特中只有一个是真正的巴特。你知道海盗以他们与其他海盗的友谊和同伴关系而闻名。如果你是一个真正的海盗，你应该能够轻松地找到自己的伙伴。好了，那么我会把你送到巴特所在的房间。"
                        ]);
                    var em = GetEventManager(Pirate4thJobFeature.StrikeEventName);
                    await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
                }
                else
                {
                    await SayOK("哦！哇，这真是令人印象深刻！我认为我的考验相当困难，而你竟然通过了……你确实是海盗家族中不可或缺的一员，也是海鸥的朋友。我们现在因着持久的友谊而结为知己！最重要的是，朋友就是在你陷入困境时伸出援手的。如果你遇到紧急情况，就呼唤我们海鸥吧。");
                }

            }
        }

        // Npc: 1022103 
        public Task s4strike_statue()
        {
            // TODO
            return Task.CompletedTask;
        }

        #region mind
        // Npc: 2095000 
        public async Task s4mind()
        {
            if (getMapId() != 925010400)
            {
                var em = GetEventManager(Pirate4thJobFeature.MindEventName);

                var option = await AskMenu($"#e#b<组队任务：拯救 红粉天书>\r\n#k#n{em.Template.GetRequirementDescription(c)}\r\n\r\n啊，#r#p1095000##k 让你来的？她担心我吗？... 很抱歉听到这个消息，但我现在真的不能回去，一些怪物受到黑魔法师的影响，我需要解救它们！... 你似乎也不会接受这个事实，对吗？你愿意和队友一起帮助我吗？如果愿意，请让你的 #b队长#k 和我交谈。", [
                    "我想参加组队任务。",
                    "我想了解更多细节。"
                ]);

                if (option == 0)
                {
                    var r = await em.StartInstance(getPlayer());
                    await SayOK(em.HandleCreateInstanceResult(r, c));
                }
                else
                {
                    await SayOK("#e#b<组队任务：拯救 红粉天书>#k#n\r\n一场伏击正在进行中！我必须在战场上站立大约6分钟才能完成解放，请在此期间保护我，以便完成我的任务。");
                }
            }
            else
            {
                if (await AskYesNo("任务成功了，感谢你的护送！我可以带你去#b#m120000104##k，你准备好了吗？"))
                {
                    await warp(120000104);
                }
            }
        }

        // Npc: 1092008 
        public async Task s4mind_in()
        {
            if (!isQuestStarted(6410))
            {
                await SayOK(GetDefaultTalk(6410));
                return;
            }
            else
            {
                if (await AskYesNo("让我们去救 #r#p2095000##k 吧？"))
                {
                    await warp(925010000, 0);
                }
            }
        }

        // Npc: 1095000 
        public async Task s4mind_out()
        {
            if (await AskYesNo("#b#p2095000##k一定有办法爬上这个悬崖，根据我们最新的报告... 或者你是想要 #r离开这里#k 吗？"))
            {
                await warp(120000104);
            }
        }
        #endregion
    }
}
