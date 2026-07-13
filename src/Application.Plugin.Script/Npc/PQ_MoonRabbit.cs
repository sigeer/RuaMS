using Application.Core.scripting.Events.Abstraction;
using Application.Core.Scripting.Events;
using Application.Plugin.Script.Events;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {

        // Npc: 1012112 
        public async Task moonrabbit()
        {
            if (getMapId() == 100000200)
            {
                var em = GetEventManager(nameof(PQ_Henesys));

                var option = await AskMenu($"#e#b<组队任务: 迎月花山丘>\r\n#k#n{em.Template.GetRequirementDescription(c)}\r\n\r\n我是达尔利。这里有一座美丽的山丘，迎月花在那里盛开。山丘上住着一只老虎，名叫兴儿，它似乎在找吃的。你想前往迎月花山丘，与你的队友们联手帮助兴儿吗？#b\r\n" +
                    "#L0#我想参加组队任务。\r\n#L1#我想了解更多详情。\r\n#L2#我想兑换一件年糕的帽子。");
                switch (option)
                {
                    case 0:
                        var r = await em.StartInstance(getPlayer());
                        await SayOK(em.HandleCreateInstanceResult(r, c));
                        break;
                    case 1:
                        await SayOK(GetTalkMessage(nameof(ScriptTalk.HenesysPQ_Intro)));
                        break;
                    case 2:
                        if (hasItem(4001101, 20) && canHold(1002798))
                        {
                            await gainItem(4001101, -20);
                            await gainItem(1002798, 1);

                            await SayNext(GetTalkMessage(nameof(ScriptTalk.Redeem_Success)));
                        }
                        else
                        {
                            await SayNext(GetTalkMessage(nameof(ScriptTalk.Redeem_NotEnough), "#t4001101#"));
                        }
                        break;
                    default:
                        break;
                }
            }
            else if (getMapId() == 910010100)
            {
                if (await AskYesNo(GetTalkMessage(nameof(ScriptTalk.HenesysPQ_Complete))))
                {
                    var eim = GetEventInstanceTrust();
                    if (await eim.GiveClearReward(getPlayer()) == ClaimRewardResult.Success)
                    {
                        await warp(100000200);
                    }
                    else
                    {
                        await SayOK(GetTalkMessage(nameof(ScriptTalk.Redeem_InventoryFull)));
                    }
                    return;
                }
            }
            else if (getMapId() == 910010400)
            {
                if (await AskYesNo(GetTalkMessage(nameof(ScriptTalk.AreYouReturningMap), GetTalkMessage(ScriptTalk.Henesys))))
                {
                    var eim = GetEventInstanceTrust();
                    if (await eim.GiveClearReward(getPlayer()) == ClaimRewardResult.Success)
                    {
                        await warp(100000200);
                    }
                    else
                    {
                        await SayOK(GetTalkMessage(nameof(ScriptTalk.Redeem_InventoryFull)));
                    }
                    return;
                }
            }
        }


        // Npc: 1012113 
        public async Task moonrabbit_bonus()
        {
            if (getMapId() == 910010100)
            {
                await SayNext("你好！我是汤米。我们站的附近有一个猪镇。那里的猪很吵闹，无法控制，甚至偷了许多旅行者的武器。它们被赶出了自己的城镇，目前躲藏在猪镇里。");

                if (isEventLeader())
                {
                    await SayNext("你觉得和你的队员一起去那里，给那些吵闹的猪一个教训怎么样？");
                    var eim = GetEventInstanceTrust();
                    await eim.startEventTimer(5 * 60000);
                    await eim.warpEventTeam(910010200);
                }
                else
                {
                    await SayOK("感兴趣吗？告诉你的队长和我联系，前往那里！");
                }
            }
            else if (getMapId() == 910010200)
            {
                if (await AskYesNo("你想现在退出奖励吗？"))
                {
                    await warp(910010400);
                }
            }
            else if (getMapId() == 910010300)
            {
                await SayOK("你现在将被传送出去，谢谢你的帮助！");
                await warp(100000200);
            }
        }


        // Npc: 1012114 
        public async Task moonrabbit_tiger()
        {
            int option;
            if (isEventLeader())
            {
                option = await AskMenu(GetTalkMessage(nameof(ScriptTalk.HenesysPQ_TutorialTalk0)));
            }
            else
            {
                option = await AskMenu(GetTalkMessage(nameof(ScriptTalk.HenesysPQ_TutorialTalk1)));
            }

            switch (option)
            {
                case 0:
                    await SaySpeech([
                        GetTalkMessage(nameof(ScriptTalk.HenesysPQ_Tutorial0)),
                        GetTalkMessage(nameof(ScriptTalk.HenesysPQ_Tutorial1)),
                        GetTalkMessage(nameof(ScriptTalk.HenesysPQ_Tutorial2)),
                        GetTalkMessage(nameof(ScriptTalk.HenesysPQ_Tutorial3)),
                        ]);
                    break;
                case 1:
                    if (haveItem(4001101, 10))
                    {
                        await SayNext(GetTalkMessage(nameof(ScriptTalk.HenesysPQ_CommitTask_Success)));

                        if (haveItem(4001101, 10))
                        {
                            await gainItem(4001101, int.MinValue);

                            var eim = GetEventInstanceTrust();
                            eim.setProperty(1 + "stageclear", "true");
                            await ClearLudiPQStage(eim, getMapId());

                            var map = await eim.getMapInstance(getPlayer().getMapId());
                            await map.killAllMonstersNotFriendly();

                            await eim.clearPQ();
                            return;
                        }


                    }
                    await SayOK(GetTalkMessage(nameof(ScriptTalk.HenesysPQ_CommitTask_Fail)));
                    break;
                case 2:
                    if (await AskYesNo(GetTalkMessage(nameof(ScriptTalk.AreYouReturning))))
                    {
                        await warp(910010300);
                    }
                    else
                    {
                        await SayOK(GetTalkMessage(nameof(ScriptTalk.HenesysPQ_TutorialTalk2)));
                    }
                    break;
                default:
                    break;
            }
        }

    }
}
