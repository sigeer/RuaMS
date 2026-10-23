using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;
using Application.Shared.Items;

namespace Application.Plugin.Script.Features.Zakum
{
    internal sealed class ZakumPQNpcHandler : NpcScriptBase
    {
        public ZakumPQNpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 2030008
        public async Task Zakum00()
        {
            if (haveItem(4001109, 1))
            {
                await warp(921100000, "out00");
                return;
            }

            var em = GetEventManager(ZakumPQ.PartyQuestEventName);
            if (em == null)
            {
                await SayOK("扎昆组队任务遇到了一个错误。");
                return;
            }

            if (IsQuestNotStarted(ZakumFeature.QuestId))
            {
                if (getLevel() >= em.MinLevel)
                {
                    await SayOK("小心，古老的力量并未被遗忘……如果你希望有朝一日击败#r扎昆#k，首先要获得#b长老会#k的批准，然后#b面对考验#k，只有这样你才有资格进行战斗。");
                }
                else
                {
                    await SayOK("小心，古老的力量并未被遗忘……");
                }
                return;
            }

            var selection = await AskMenu("#e#b<组队任务：扎昆组队任务>\r\n#k#n" + em.Template.GetRequirementDescription(c) + "\r\n\r\n小心，古老的力量并未被遗忘... #b\r\n#L0#进入未知的死亡矿井（第1阶段）#l\r\n#L1#面对熔岩之息（第2阶段）#l\r\n#L2#锻造#t4001017#（第3阶段）#l");

            if (selection == 0)
            {
                await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
            }
            else if (selection == 1)
            {
                if (haveItem(4031061) && !haveItem(4031062))
                {
                    if (await AskYesNo("你已经成功通过了第一阶段。你还有很长的路才能到达扎昆的祭台。所以，你想好挑战下一个阶段了吗？"))
                    {
                        await warp(280020000, 0);
                    }
                }
                else
                {
                    if (haveItem(4031062))
                    {
                        await SayOK("你已经得到了#b熔岩之息#k，你不需要完成这个阶段。");
                    }
                    else
                    {
                        await SayOK("请先完成之前的试炼。");
                    }
                }
            }
            else
            {
                if (haveItem(4031061) && haveItem(4031062))
                {
                    if (!haveItem(4000082, 30))
                    {
                        await SayOK("你已经完成了试炼，但是还需要 #b#v4000082##t4000082# * 30#k 来锻造 #b5 个 #v4001017##t4001017##k。");
                    }
                    else
                    {
                        await completeQuest(ZakumFeature.QuestId);
                        await gainItem(4031061, -1);
                        await gainItem(4031062, -1);
                        await gainItem(4000082, -30);
                        await gainItem(4001017, 5);
                        await SayOK("你 #r已经完成了试炼#k，从现在开始我批准你挑战扎昆。");
                    }
                }
                else
                {
                    await SayOK("你缺少一些必要的物品\r\n#b#v4031061##t4031061# * 1#k\r\n#b#v4031062##t4031062# * 1#k\r\n来锻造#b#v4001017##t4001017##k。");
                }
            }
        }


        // Npc: 2030010
        public async Task Zakum06()
        {
            if (getMapId() == 280030000)
            {
                if (!GetEventInstanceTrust().isEventCleared())
                {
                    if (await AskYesNo("如果你现在离开，你将不得不重新开始。你确定要离开吗？"))
                    {
                        await warp(ZakumPQ.ExitMap);
                    }
                }
                else
                {
                    if (await AskYesNo("你们终于打败了扎昆，真是了不起的壮举！恭喜！你确定现在要离开吗？"))
                    {
                        await warp(ZakumPQ.ExitMap);
                    }
                }
            }
            else
            {
                if (await AskYesNo("如果你现在离开，你将不得不重新开始。你确定要离开吗？"))
                {
                    await warp(ZakumPQ.ExitMap);
                }
            }
        }


        // Npc: 2030011
        public async Task Zakum04()
        {
            await SayOK("下次见。");
            await warp(ZakumPQ.ExitMap);
            await removeAll(4001015);
            await removeAll(4001016);
            await removeAll(4001018);
        }


        // Npc: 2032002
        public async Task Zakum01()
        {
            var eim = getPlayer().getEventInstance();

            if (!eim.isEventCleared())
            {
                var selection = await AskMenu("...#b\r\n#L0#我在这里应该做什么？#l\r\n#L1#我带来了物品！#l\r\n#L2#我想要离开！#l");

                if (selection == 0)
                {
                    await SayOK("为了揭示扎昆的力量，你需要重新制造它的核心。在这个地牢的某个地方隐藏着#b一块 #v4001018##t4001018# #k，这是制作核心所需的材料之一。找到它，然后带给我。\r\n哦，你能帮我一个忙吗？\r\n这附近的石头下面也有一些#b #v4001015##t4001015# #k。如果你能找到#b30份#k，我会奖励你的努力。");
                }
                else if (selection == 1)
                {
                    if (!isEventLeader())
                    {
                        await SayOK("请让你们的队长把材料带给我，以完成这个考验。");
                        return;
                    }

                    if (!haveItem(4001018))
                    {
                        await SayOK("请找到#b #v4001018##t4001018# #k 再带来给我。");
                    }
                    else
                    {
                        var gotAllDocs = haveItem(4001015, 30);
                        if (!gotAllDocs)
                        {
                            if (await AskYesNo("所以，你带了#b #v4001018##t4001018# #k来了？我可以给你和你的每个队员#b一块#v4031061##t4031061##k，这应该足够制作扎昆的核心。确保你的整个队伍在继续之前有足够的背包空间。"))
                            {
                                await gainItem(4001018, -1);
                                await eim.giveEventPlayersExp(12000);
                                await eim.clearPQ();
                            }
                        }
                        else
                        {
                            if (await AskYesNo("所以，你带来了#b #v4001018##t4001018# #k和#b #v4001015##t4001015# #k吗？我可以给你和你的每个队员#b一块#v4031061##t4031061##k，这应该足够制作扎昆的核心了。\r\n\r\n另外，既然你带来了#b #v4001015##t4001015# * 30#k，我还可以给你#b#v2030007##t2030007# * 5#k，可以随时带你到矿井入口。在继续之前，请确保你的整个队伍的背包有足够的空间。"))
                            {
                                await gainItem(4001018, -1);
                                await gainItem(4001015, -30);
                                eim.setProperty("gotDocuments", 1);
                                await eim.giveEventPlayersExp(20000);
                                await eim.clearPQ();
                            }
                        }
                    }
                }
                else if (selection == 2)
                {
                    if (await AskYesNo("你确定要退出吗？如果你是队伍的队长，你的队伍也将离开矿区。"))
                    {
                        await warp(ZakumPQ.ExitMap);
                    }
                }
            }
            else
            {
                await SayNext("你完成了这次磨难，现在领取你的奖品。");

                if (eim.getIntProperty("gotDocuments") == 1)
                {
                    if (eim.gridCheck(getPlayer()) == -1)
                    {
                        if (CanHoldAll([new ItemQuantity(2030007, 5), new ItemQuantity(4031061, 1)]))
                        {
                            await gainItem(2030007, 5);
                            await gainItem(4031061, 1);
                            eim.gridInsert(getPlayer(), 1);
                        }
                        else
                        {
                            await SayOK("确保在继续之前你的背包有足够的空间。");
                        }
                    }
                    else
                    {
                        await SayOK("你已经领取了你的份额。你现在可以通过那边的传送门离开矿井了。");
                    }
                }
                else
                {
                    if (eim.gridCheck(getPlayer()) == -1)
                    {
                        if (canHold(4031061, 1))
                        {
                            await gainItem(4031061, 1);
                            eim.gridInsert(getPlayer(), 1);
                        }
                        else
                        {
                            await SayOK("确保在继续之前你的背包有足够的空间。");
                        }
                    }
                    else
                    {
                        await SayOK("你已经领取了你的份额。你现在可以通过那边的传送门离开矿井了。");
                    }
                }
            }
        }


        // Npc: 2032003
        public async Task Zakum02()
        {
            await SayNext("恭喜你走到了这一步！好吧，我想我最好给你#b#t4031062##k。你确实赢得了它！");

            if (!canHold(4031062))
            {
                await SayOK("尝试释放一个空位来接收#b#t4031062##k。");
                return;
            }

            await SayNext("好了，你是时候离开了。");

            await gainItem(4031062, 1);
            await gainExp((int)(10000 * getPlayer().getExpRate()));
            await warp(ZakumPQ.ExitMap);
        }
    }
}
