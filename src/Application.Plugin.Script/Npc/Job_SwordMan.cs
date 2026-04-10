using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Plugin.Script
{
    /// <summary>
    /// 职业-战士相关
    /// </summary>
    internal partial class NpcScript
    {
        // Npc: 10202 
        public async Task infoSwordman()
        {
            await SayNext("战士拥有巨大的力量和持久力来支持它，他们在近战战斗中表现最出色。普通攻击本身就很强大，再加上复杂的技能，这个职业非常适合进行爆发性攻击。");
            if (await SayYesNo("你想体验一下成为一个战士是什么感觉吗？"))
            {
                lockUI();
                warp(1020100, 0);
                dispose();
            }
            else
            {
                await SayNext("如果你想体验成为一个战士的感觉，再来找我吧。");
            }
        }


        // Npc: 1072000 
        public async Task change_swordman()
        {
            if (isQuestCompleted(100004))
            {
                await SayOK("你真是一个真正的英雄！");
                return;
            }
            else if (isQuestCompleted(100003))
            {
                await SayNext("好的，我会让你进去！打败里面的怪物，收集30个#t4031013#，然后和我里面的一位同事交谈。他会给你#b#t4031012##k，证明你已经通过了测试。祝你好运。");
                warp(108000300, 0);
            }
            else if (isQuestStarted(100003))
            {
                await SaySpeech([
                    "嗯...这绝对是来自#b#t4031008##k的信件...所以你来到这里参加测试，进行战士的第二次转职。好吧，我来给你解释一下测试。不要太担心，它并不那么复杂。",
                    "我会把你送到一个隐藏的地图。你会看到一些平常不会见到的怪物。它们看起来和普通的怪物一样，但态度完全不同。它们既不会提升你的经验等级，也不会给你提供物品。",
                    "你将能够在打倒这些怪物时获得一种名为#b#t4031013##k的大理石。这是一种由它们邪恶的心灵制成的特殊大理石。收集30个，然后去找我的一个同事谈谈。这就是你通过考验的方法。",
                    ], finalNext: true);
                if (await SayYesNo("一旦你进去，就不能离开，直到完成你的任务。如果你死了，你的经验等级会下降...所以你最好做好准备...那么，你现在想去吗？"))
                {
                    await SayNext("好的，我会让你进去！打败里面的怪物，收集30个#t4031013#，然后和我里面的一位同事交谈。他会给你#b#t4031012##k，证明你已经通过了测试。祝你好运。");
                    warp(108000300, 0);
                    completeQuest(100003);
                    startQuest(100004);
                    gainItem(4031008, -1);
                }
            }
            else
            {
                await SayOK("一旦你准备好了，我可以告诉你路线。");
            }
        }

        // Npc: 1072004 
        public async Task inside_swordman()
        {
            if (haveItem(4031013, 30))
            {
                await SayOK("哦哦哦.. 你收集了所有30个#t4031013#！！这应该很困难... 简直不可思议！好吧。你通过了测试，为此，我会奖励你 #b#t4031012##k。拿着它回佩里安去吧。");
                removeAll(4031013);
                completeQuest(100004);
                startQuest(100005);
                gainItem(4031012);
            }
            else
            {
                await SayOption("你需要收集 #b30 个 #t4031013##k。祝你好运。", ["我想离开"]);
            }
            warp(102020300, 9);
        }

    }
}
