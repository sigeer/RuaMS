namespace Application.Plugin.Script
{
    internal partial class NpcScript
    {
        // Npc: 1052001 
        public Task rogue()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 10203 
        public async Task infoRogue()
        {
            await SayNext("飞侠是幸运、灵巧和力量的完美结合，擅长对无助的敌人进行突袭攻击。高水平的闪避能力和速度使得飞侠能够从各个角度攻击敌人。");
            if (await SayYesNo("想体验一下飞侠是什么感觉嘛？"))
            {
                lockUI();
                warp(1020400, 0);
                dispose();
            }
            else
            {
                await SayNext("如果你想体验成为一个弓箭手的感觉，再来找我吧。");
            }
        }
        // Npc: 1052114 
        public Task enter_thief()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1072003 
        public async Task change_rogue()
        {
            if (isQuestCompleted(100010))
            {
                await SayOK("你真是一个真正的英雄！");
                return;
            }
            else if (isQuestCompleted(100010))
            {
                await SayNext("好的，我会让你进去！打败里面的怪物，收集30个#t4031013#，然后和我里面的一位同事交谈。他会给你#b#t4031012##k，证明你已经通过了测试。祝你好运。");
                warp(108000200, 0);
            }
            else if (isQuestStarted(100010))
            {
                await SaySpeech([
                    "嗯...这绝对是#b#t4031011##k...所以你来到这里是为了接受测试，进行弓箭手第二次职业转职。好吧，我来给你解释一下测试。不要太担心，它并不是那么复杂。",
                    "我会把你送到一个隐藏的地图。你会看到一些平常不会见到的怪物。它们看起来和普通的怪物一样，但态度完全不同。它们既不会提升你的经验等级，也不会给你提供物品。",
                    "你将能够在打倒这些怪物时获得一种名为#b#t4031013##k的大理石。这是一种由它们邪恶的心灵制成的特殊大理石。收集30个，然后去找我的一个同事谈谈。这就是你通过考验的方法。",
                    ], finalNext: true);
                if (await SayYesNo("一旦你进去，就不能离开，直到完成你的任务。如果你死了，你的经验等级会下降...所以你最好做好准备...那么，你现在想去吗？"))
                {
                    await SayNext("好的，我会让你进去！打败里面的怪物，收集30个#t4031013#，然后和我里面的一位同事交谈。他会给你#b#t4031012##k，证明你已经通过了测试。祝你好运。");
                    warp(108000400, 0);
                    completeQuest(100010);
                    startQuest(100010);
                    gainItem(4031011, -1);
                }
            }
            else
            {
                await SayOK("一旦你准备好了，我可以告诉你路线。");
            }
        }
        // Npc: 1072007 
        public async Task inside_rogue()
        {
            if (haveItem(4031013, 30))
            {
                await SayOK("你是一个真正的英雄！拿着这个，达克尔会承认你的。");
                removeAll(4031013);
                completeQuest(100010);
                startQuest(100011);
                gainItem(4031012);
            }
            else
            {
                await SayOption("你需要收集 #b30 个 #t4031013##k。祝你好运。", ["我想离开"]);
            }
            warp(102040000, 9);
        }
    }
}
