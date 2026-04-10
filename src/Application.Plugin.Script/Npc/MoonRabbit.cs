using Application.Core.Scripting.Events;
using Application.Resources.Messages;

namespace Application.Plugin.Script
{
    internal partial class NpcScript
    {
        // Npc: 1012112 
        public async Task moonrabbit()
        {
            if (getMapId() == 100000200)
            {
                var em = GetEventManager<PartyQuestEventManager>("HenesysPQ");

                var option = await SayOption("#e#b<组队任务: 迎月花山丘>\r\n#k#n" + em.getProperty("party") + "\r\n\r\n我是达尔利。这里有一座美丽的山丘，迎月花在那里盛开。山丘上住着一只老虎，名叫兴儿，它似乎在找吃的。你想前往迎月花山丘，与你的队友们联手帮助兴儿吗？#b\r\n" +
                    "#L0#我想参加组队任务。#L1#我想了解更多详情。\r\n#L2#我想兑换一件年糕的帽子。");
                switch (option)
                {
                    case 0:
                        var party = getParty();
                        if (party == null)
                        {
                            await SayOK(GetTalkMessage(nameof(ScriptTalk.HenesysPQ_EnterTalk1)));
                            return;
                        }
                        else if (!isLeader())
                        {
                            await SayOK(GetTalkMessage(nameof(ScriptTalk.PartyQuest_NeedLeaderTalk)));
                            return;
                        }
                        else
                        {
                            var eli = em.getEligibleParty(party);
                            if (eli.Count > 0)
                            {
                                if (!em.StartPQInstance(getPlayer(), eli))
                                {
                                    await SayOK(GetTalkMessage(nameof(ScriptTalk.PartyQuest_CannotStart_ChannelFull)));
                                }
                            }
                            else
                            {
                                await SayOK(GetTalkMessage(nameof(ScriptTalk.PartyQuest_CannotStart_Req)));
                            }
                        }
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
                if (await SayYesNo(GetTalkMessage(nameof(ScriptTalk.HenesysPQ_Complete))))
                {
                    var eim = getEventInstance();
                    if (eim != null && !eim.giveEventReward(getPlayer()))
                    {
                        await SayOK(GetTalkMessage(nameof(ScriptTalk.Redeem_InventoryFull)));
                        return;
                    }
                    warp(100000200);
                }
            }
            else if (getMapId() == 910010400)
            {
                if (await SayYesNo(GetTalkMessage(nameof(ScriptTalk.AreYouReturningMap), GetTalkMessage(ScriptTalk.Henesys))))
                {
                    var eim = getEventInstance();
                    if (eim != null && !eim.giveEventReward(getPlayer()))
                    {
                        await SayOK(GetTalkMessage(nameof(ScriptTalk.Redeem_InventoryFull)));
                        return;
                    }
                    warp(100000200);
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
        public Task moonrabbit_tiger()
        {
            // TODO
            return Task.CompletedTask;
        }

    }
}
