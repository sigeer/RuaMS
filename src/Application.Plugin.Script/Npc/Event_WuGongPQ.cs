using Application.Core.Scripting.Events;
using Application.Scripting.JS;
using Application.Shared.Constants.Item;
using Application.Shared.Constants.Map;
using client.inventory;
using Microsoft.Extensions.Logging;
using server.maps;
using server.quest;
using System;
using System.Collections.Generic;
using System.Net.ServerSentEvents;
using System.Text;

namespace Application.Plugin.Script
{
    internal partial class NpcScript
    {
        // Npc: 9310004 
        public async Task shanghai001()
        {
            var itemId = 4031289;
            var enterMap = 701010321;
            if (isQuestStarted(4103) || isQuestStarted(8512))
            {
                if (haveItem(itemId, 1))
                {
                    await SayNext($"嗯？\r\n看不出来你还有些本事哦，连#b#e#v{itemId}##t{itemId}##k#n都能搞到手。\r\n既然如此，那你就先去#b#e#m${enterMap}##k#n调查看看。");
                }

                else
                {
                    await SayNext("最近的家畜又突然变得非常暴躁，我们推测是#b#e#o9300188##k#n又又又卷土重来了，麻烦你去处理一下吧。");
                }

                if (await SayYesNo($"我现在送你进入#b#e#m{enterMap}##k#n，准备好了吗？"))
                {
                    gainItem(itemId, -1);
                    warp(enterMap);
                }
                else
                {
                    await SayOK($"当你准备好去#b#e#m{enterMap}##k#n的时候再来找我。");
                }
            }
            else
            {
                await SayOK($"敬礼！\r\n你好，#b#h ##k，我是#e#b#p{getNpc()}##k#n，在这片区域进行巡逻。\r\n如果你没什么事的话，还是快点回去吧！");
            }
        }


        // Npc: 9310005 
        public async Task shanghai002()
        {
            var questId = 4109;
            var lobbyMap = 701010321;
            var enterMap = lobbyMap + 1;
            var outMap = lobbyMap - 1;
            var itemID = 4000194;
            var collected = getItemQuantity(itemID);

            var chrQuestStatus = getQuestStatus(questId);
            if (chrQuestStatus == 0)
            {
                await SayNext("敬礼！\r\n要做一些检查！\r\n这里是禁区，闲人免进。\r\n没有许可的人是不允许进入的！\r\n什么？已经获得许可了？");
                if (await SayYesNo("哦...刚刚接到信息。您是特别行动小组的人啊。看来是位了不起的人啊，如果您帮我个小忙的话，我就让您通过。您可以帮我吗？"))
                {
                    startQuest(questId);
                }
                else
                {
                    await SayNext("如果不协助执行公务的话，就不能让你通过。请回吧。");
                }
            }
            else
            {
                var option = await SayOption($"你需要收集 #e#b#v{itemID}##t${itemID}##k#n × #r#e50#k#n \r\n才能证明你有点本事，否则我不放心让你去白给！\r\n", [
                    $"进入 #b#e#m${enterMap}##k#n 继续调查",
                    $"离开 #b#e#m${lobbyMap}##k#n 回到 #b#e#m${outMap}"
                ]);
                switch (option)
                {
                    case 0:

                        if (chrQuestStatus == 2)
                        {
                            if (await SayYesNo($"我现在送你进入#b#e#m{enterMap}##k#n，准备好了吗？"))
                            {
                                var quest = Quest.getInstance(questId);
                                quest.reset(getPlayer());
                                warp(enterMap, "h000");
                                return;
                            }
                        }
                        else
                        {
                            await SayOK("连这小小的要求都做不到，没点儿本事你难道还想去白给啊？");
                        }

                        break;
                    case 1:
                        if (await SayYesNo($"你已经决定要离开吗？\r\n当你准备好去#b#e#m{enterMap}##k#n的时候再来找我。"))
                        {
                            warp(outMap);
                        }
                        break;
                    default:
                        break;
                }
            }





        }


        // Npc: 9310006 
        public async Task shanghai003()
        {
            var em = GetEventManager<PartyQuestEventManager>("WuGongPQ");
            var option = await SayOption($"敬礼！\r\n你好，#e#b#h ##k#n，我是#b#e#p{getNpc()}##k#n\r\n\r\n#L0##b秘密任务#l\r\b\r\n#L1#离开#l#k");
            switch (option)
            {
                case 0:
                    await SayNext("你居然能来到这儿……果然不简单!\r\n不过你听说过这附近有很危险的怪物吗？");
                    var party = getParty();
                    if (party == null)
                    {
                        await SayOK("该项#b秘密任务#k十分危险，你需要进行一个组队才能进入执行。");
                    }
                    else if (!isLeader())
                    {
                        await SayOK("请让你的队长来开始这个任务。");
                    }
                    else
                    {
                        await SayNext("走到下一个地方时，你会遇到很危险的怪物。千万小心。\r\n再见!");
                        if (!isQuestCompleted(4103) && !isQuestCompleted(8512))
                        {
                            //完成了赤珠任务将传送到一个地图
                            var level = getLevel();
                            if (level >= LevelMin && level <= LevelMax)
                            {
                                warp(701010324);
                            }
                            else
                            {
                                await SayOK($"你目前无法执行这个#b秘密任务#k，因你不符合要求：\r\n\r\n:等级要求：{LevelMin}~ {LevelMax}");
                            }
                        }
                        else
                        {
                            var eli = em.getEligibleParty(party);
                            if (eli.Count > 0)
                            {
                                if (!em.StartPQInstance(getPlayer(), eli, 1))
                                {
                                    await SayOK($"已经有其他人员在执行#b秘密任务#k了，请稍后再试。");
                                }
                            }
                            else
                            {
                                await SayOK($"你目前无法执行这个#b秘密任务#k，因你不符合要求");
                                // msg = '你目前无法执行这个#b秘密任务#k，因你不符合要求：' + em.getProperty('party');
                            }
                        }
                    }
                    break;
                case 1:
                    var mapId = 701010320;
                    if (await SayYesNo($"你要离开#b#e#m{getMapId()}##k#n 回到 #b#e#m{mapId}##k#n 吗？"))
                    {
                        warp(mapId);
                    }
                    break;
                default:
                    break;
            }
        }


        // Npc: 9310007 
        public async Task shanghai004()
        {
            var mapId = 701010320;
            if (await SayYesNo($"你要离开#b#e#m{getMapId()}##k#n 回到 #b#e#m{mapId}##k#n 吗？"))
            {
                warp(mapId);
            }
        }
    }
}
