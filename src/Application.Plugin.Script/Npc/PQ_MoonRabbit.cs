using Application.Core.scripting.Events.Abstraction;
using Application.Core.Scripting.Events;
using Application.Plugin.Script.Events;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Events
{
    internal partial class NpcScript
    {

        // Npc: 1012112 
        public async Task moonrabbit()
        {
            if (getMapId() == 100000200)
            {
                var em = GetEventManager(nameof(PQ_Henesys));

                var option = await AskMenu($"#e#b<组队任务: 迎月花山丘>\r\n#k#n{em.GetRequirementDescription(c)}\r\n\r\n我是达尔利。这里有一座美丽的山丘，迎月花在那里盛开。山丘上住着一只老虎，名叫兴儿，它似乎在找吃的。你想前往迎月花山丘，与你的队友们联手帮助兴儿吗？#b\r\n" +
                    "#L0#我想参加组队任务。#L1#我想了解更多详情。\r\n#L2#我想兑换一件年糕的帽子。");
                switch (option)
                {
                    case 0:
                        var r = em.StartInstance(getPlayer());
                        await SayOK(em.HandleCreateInstanceResult(r, c));
                        break;
                    case 1:
                        await SayOK(GetTalkMessage(nameof(ScriptTalk.HenesysPQ_Intro)));
                        break;
                    case 2:
                        if (hasItem(4001101, 20) && canHold(1002798))
                        {
                            gainItem(4001101, -20);
                            gainItem(1002798, 1);

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
                    if (eim.GiveClearReward(getPlayer()) == ClaimRewardResult.Success)
                    {
                        warp(100000200);
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
                    if (eim.GiveClearReward(getPlayer()) == ClaimRewardResult.Success)
                    {
                        warp(100000200);
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
        public Task moonrabbit_bonus()
        {
            // TODO
            return Task.CompletedTask;
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
                            gainItem(4001101, int.MinValue);

                            var eim = GetEventInstanceTrust();
                            eim.setProperty(1 + "stageclear", "true");
                            ClearLudiPQStage(eim, getMapId());

                            var map = eim.getMapInstance(getPlayer().getMapId());
                            map.killAllMonstersNotFriendly();

                            eim.clearPQ();
                            return;
                        }


                    }
                    await SayOK(GetTalkMessage(nameof(ScriptTalk.HenesysPQ_CommitTask_Fail)));
                    break;
                case 2:
                    if (await AskYesNo(GetTalkMessage(nameof(ScriptTalk.AreYouReturning))))
                    {
                        warp(910010300);
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
