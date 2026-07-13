using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {

        // Npc: 9209001 
        public async Task MapleMarket7_Enter()
        {
            await SayOK("你好，冒险岛第七天市场目前不可用。");
        }


        // Npc: 9209000 
        public async Task dealerA()
        {
            var greeting = "嗨，我是#p9209000#，技能&熟练度书籍通报道！";

            if (getPlayer().isCygnus())
            {
                await SayOK(greeting + "圣骑士没有技能或熟练度书籍可用。");
                return;
            }

            var jobrank = getJobId() % 10;
            if (jobrank < 2)
            {
                await SayOK(greeting + "继续训练，直到你达到职业的#r第4次转职#k。当你达到这一壮举时，新的提升机会就会出现！");
                return;
            }

            var skillbook = getAvailableSkillBooks();
            var masterybook = getAvailableMasteryBooks();

            if (skillbook.Length == 0 && masterybook.Length == 0)
            {
                await SayOK(greeting + "目前没有可用于进一步提升职业技能的书籍。要么你已经#b把所有的技能都练满了#k，要么你还没有达到使用某些技能书籍的最低要求。");
                return;
            }

            int selected = 0;
            if (skillbook.Length > 0 && masterybook.Length > 0)
            {
                selected = await AskMenu(greeting + "已经为你找到了提升技能的新机会！选择一种类型来看看。\r\n\r\n#b#L1#技能书#l\r\n#L2#熟练度书籍#l");
            }
            else if (skillbook.Length > 0)
            {
                selected = 1;
                await SayNext(greeting + "已经为你找到了提升技能的新机会！目前只有技能学习可用。");
            }
            else
            {
                selected = 2;
                await SayNext(greeting + "已经为你找到了提升技能的新机会！目前只有技能提升可用。");
            }

            var table = selected == 1 ? skillbook : masterybook;
            var sendStr = "以下书籍目前可用：\r\n\r\n";
            for (int i = 0; i < table.Length; i++)
            {
                if (table[i] > 0)
                {
                    var itemid = table[i];
                    sendStr += $"  #L{i}# #i{itemid}#  #t{itemid}##l\r\n";
                }
                else
                {
                    var skillid = -table[i];
                    sendStr += $"  #L{i}# #s{skillid}#  #q{skillid}##l\r\n";
                }
            }

            var itemSel = await AskMenu(sendStr);
            var chosen = table[itemSel];

            var resultStr = "";
            if (chosen > 0)
            {
                var mobList = getNamesWhoDropsItem(chosen);
                if (mobList.Length == 0)
                {
                    resultStr = $"没有掉落 '#b#t{chosen}##k' 的怪物。\r\n\r\n";
                }
                else
                {
                    resultStr = $"掉落 '#b#t{chosen}##k' 的怪物如下：\r\n\r\n";
                    foreach (var mob in mobList)
                    {
                        resultStr += $"  #r{mob}#k\r\n";
                    }
                    resultStr += "\r\n\r\n";
                }
            }

            resultStr += getSkillBookInfo(chosen);
            await SayNext(resultStr);
        }

        // Npc: 9209100 
        public async Task xmas_party_ent()
        {
            var chimney = getMap().getPortal("chimney01");
            if (chimney != null)
            {
                var plyPos = getPlayer().getPosition();
                var portPos = chimney.getPosition();
                var dist = Math.Sqrt(Math.Pow(portPos.X - plyPos.X, 2) + Math.Pow(portPos.Y - plyPos.Y, 2));
                if (dist < 77)
                {
                    await SayOK("嘿，嘿~~请不要未经允许潜入别人家里，你可不想今年在圣诞老人的名单上被列为调皮的吧？");
                }
                else
                {
                    await SayOK("嘿嘿嘿~~你有一个充满健康、实现和幸福的美好一年！");
                }
            }
            else
            {
                await SayOK("嘿嘿嘿~~你有一个充满健康、实现和幸福的美好一年！");
            }
        }

        // Npc: 9220004 
        public async Task wxmasB()
        {
            var sel = await AskMenu("#b<突袭任务：快乐村>#k\r\n突袭就是许多人联合起来试图击败极其强大的生物。这里也不例外。每个人都可以参与击败出现的生物。你会做什么？\r\n#b\r\n#L0#召唤雪人孩子。\r\n#L1#召唤迷失的鲁道夫。\r\n#L2#什么都不做，只是放松一下。#k");
            if (sel == 0)
            {
                if (getMap().countMonsters() > 1)
                {
                    await SayOK("在该区域清除所有的怪物，召唤雪人宝宝。");
                    return;
                }
                await getMap().spawnMonsterOnGroundBelow(9500317, 1700, 80);
            }
            else if (sel == 1)
            {
                if (getMap().countMonsters() > 6)
                {
                    await SayOK("这个地方现在太拥挤了。在再次尝试之前清理一些怪物。");
                    return;
                }
                await getMap().spawnMonsterOnGroundBelow(9500320, 1700, 80);
            }
            else
            {
                await SayOK("好的。");
            }
        }

    }
}
