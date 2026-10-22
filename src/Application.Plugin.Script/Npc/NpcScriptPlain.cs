namespace Application.Plugin.Script.Npc
{
    /// <summary>
    /// WZ 未声明 script 节点、运行期键回退为 <c>n&lt;id&gt;</c> 的 NPC 脚本。
    /// 这些方法只包含单一固定对话（<c>cm.sendOk</c>）或打开仓库，逻辑无分支。
    /// </summary>
    internal partial class NpcScript
    {
        /// <summary>
        /// 只做一件事：打开仓库（JS: <c>cm.OpenStorage(); cm.dispose();</c>）。
        /// JS 中每个文件内容完全相同，这里合并为一个方法供多个 NPC 共用。
        /// </summary>
        // Npc: 1002005, 1012009, 1022005, 1032006, 1052017, 1061008, 1091004, 1100000, 1200000, 2010006, 2020004, 2041008, 2050004, 2060008, 2070000, 2080005, 2090000, 2093003, 2100000, 2110000, 9030100, 9120009, 9201081, 9270042, 9270054
        [ScriptName("n1002005", "n1012009", "n1022005", "n1032006", "n1052017", "n1061008", "n1091004", "n1100000", "n1200000", "n2010006", "n2020004", "n2041008", "n2050004", "n2060008", "n2070000", "n2080005", "n2090000", "n2093003", "n2100000", "n2110000", "n9030100", "n9120009", "n9201081", "n9270042", "n9270054")]
        public async Task OpenStoragePlain()
        {
            await OpenStorage();
        }

        // Npc: 1092015
        [ScriptName("n1092015")]
        public async Task NpcScript_1092015()
        {
            await SayOK("我无聊！有人来和我玩吗？");
        }

        // Npc: 1096001
        [ScriptName("n1096001")]
        public async Task NpcScript_1096001()
        {
            await SayOK("我会不会完成清理呢？这艘船实在太大了…");
        }

        // Npc: 1103000
        [ScriptName("n1103000")]
        public async Task NpcScript_1103000()
        {
            await SayOK("我是高级骑士 #b#p1103000##k。多亏了你的勇敢，我和Ereve的所有人都从Eleanor的控制中被解救出来了。感谢我们女皇的仁慈，我们战斗得很顺利！");
        }

        // Npc: 1104202
        [ScriptName("n1104202")]
        public async Task NpcScript_1104202()
        {
            await SayOK("抱歉，我无法完成你的要求。");
        }

        // Npc: 1104203
        [ScriptName("n1104203")]
        public async Task NpcScript_1104203()
        {
            await SayOK("抱歉，我无法完成你的要求。");
        }

        // Npc: 1104204
        [ScriptName("n1104204")]
        public async Task NpcScript_1104204()
        {
            await SayOK("抱歉，我无法完成你的要求。");
        }

        // Npc: 1104205
        [ScriptName("n1104205")]
        public async Task NpcScript_1104205()
        {
            await SayOK("抱歉，我无法完成你的要求。");
        }

        // Npc: 1104206
        [ScriptName("n1104206")]
        public async Task NpcScript_1104206()
        {
            await SayOK("抱歉，我无法完成你的要求。");
        }

        // Npc: 1104207
        [ScriptName("n1104207")]
        public async Task NpcScript_1104207()
        {
            await SayOK("抱歉，我无法完成你的要求。");
        }

        // Npc: 1209001
        [ScriptName("n1209001")]
        public async Task NpcScript_1209001()
        {
            await SayOK("“#r黑魔法师#k的军队正在以不可阻挡的速度接近这里……我们别无选择，只能立刻逃离这个地区，留下我们的家园。哦，这是多么悲惨的事情！”");
        }

        // Npc: 1209002
        [ScriptName("n1209002")]
        public async Task NpcScript_1209002()
        {
            await SayOK("叔叔，请冷静一点。我们正在启程前往金银岛，一旦到达那里我们就会安全了。所以，加油！");
        }

        // Npc: 1209003
        [ScriptName("n1209003")]
        public async Task NpcScript_1209003()
        {
            await SayOK("我们将暂时前往#b金银岛#k。我听说#r黑魔法师#k本人还不能夺取那个地方，多亏了#b被施加在那个地区的封印#k。我们为他们的安全祈祷，但如果命运不眷顾英雄们，至少我们一旦抵达大陆就会安全了。");
        }

        // Npc: 1209004
        [ScriptName("n1209004")]
        public async Task NpcScript_1209004()
        {
            await SayOK("我希望这次旅行能够安全无恙，希望我们能在那里生活在一个更加宁静的地方……嘿，亲爱的，让我们走吧。");
        }

        // Npc: 1209005
        [ScriptName("n1209005")]
        public async Task NpcScript_1209005()
        {
            await SayOK("好的，开始进行...");
        }

        // Npc: 1300001
        [ScriptName("n1300001")]
        public async Task NpcScript_1300001()
        {
            await SayOK("让仪式开始吧，我们不能让大家等待！嘿咳~嘿咳~嘿~~");
        }

        // Npc: 1300006
        [ScriptName("n1300006")]
        public async Task NpcScript_1300006()
        {
            await SayOK("嘿，伙计们，怎么回事？我这么小就结婚了？可我还是个孩子啊！怎么会这样？");
        }

        // Npc: 2041017
        [ScriptName("n2041017")]
        public async Task NpcScript_2041017()
        {
            await SayOK("嘿，冒险家！保密哦，好吗？我们目前正在按照即时营销策略制造所谓的#b#t2049100##k。你需要吗？我们在两个方面行动：如果你想要一大堆这些东西，就和我说话。这将是一个类似#b任务#k的过程，但我需要你提供大量#b难以获得的小工具#k。完成后我需要#r3天#k的休息时间，然后才能再次为你工作。\r\n和我的伙伴谈谈，他会按照即时合成技术为你制作这些卷轴，需要一大堆#b低成本物品#k，#r任何时间任何地点#k。");
        }

        // Npc: 2041024
        [ScriptName("n2041024")]
        public async Task NpcScript_2041024()
        {
            await SayOK("对于那些能够完成伟大壮举并拥有坚定决心的人来说，#b最终目的地#k就在门后。机房一次只接受#r一个队伍#k，所以当穿过门时，请确保你的队伍已经准备好。");
        }

        // Npc: 2041029
        [ScriptName("n2041029")]
        public async Task NpcScript_2041029()
        {
            await SayOK("前方是神秘的#b埃林#k森林。如果你准备好揭开金银岛过去的一些神秘，就穿过#b时间之门#k。这将展现金银岛黎明时的模样。");
        }

        // Npc: 2101002
        [ScriptName("n2101002")]
        public async Task NpcScript_2101002()
        {
            await SayOK("如果你不想遇到危险，就离我远一点。");
        }

        // Npc: 2120003
        [ScriptName("n2120003")]
        public async Task NpcScript_2120003()
        {
            await SayOK("你好，欢迎，亲爱的客人。主人为您准备了一些精彩的游戏，让您今晚享受。");
        }

        // Npc: 2131000
        [ScriptName("n2131000")]
        public async Task NpcScript_2131000()
        {
            await SayOK("自从我们离开奥西里亚以避开黑魔法师已经有一段时间了。如果不是因为世界树，我不知道我们会在哪里。我一直在努力在这里安家立业，但并不容易。我想知道家乡的情况如何。");
        }

        // Npc: 2131002
        [ScriptName("n2131002")]
        public async Task NpcScript_2131002()
        {
            await SayOK("我的名字是#p2131002#... 你看，我是一个仙女。人们告诉我我不像仙女，但是... 我喜欢用金属物品制作东西。嘘，不要告诉任何人，但我也喜欢混合武术。");
        }

        // Npc: 2132000
        [ScriptName("n2132000")]
        public async Task NpcScript_2132000()
        {
            await SayOK("嗯！能够离开营地来到这里，你一定是一个非常坚强的个体。让我们探索新的地区，找到一个地方建立我们自己的城镇！");
        }

        // Npc: 9120023
        [ScriptName("n9120023")]
        public async Task NpcScript_9120023()
        {
            await SayOK("这些天他们推出的电影质量令人印象深刻！");
        }

        // Npc: 9270031
        [ScriptName("n9270031")]
        public async Task NpcScript_9270031()
        {
            await SayOK("我们在阿莫利亚举行了婚礼，那是一个美丽的地方，那里的人们更加和蔼可亲。现在我们在这个天堂般的地方度蜜月...啊，多么辉煌，多么辉煌！");
        }


        // Npc: 1094000, 2101000, 2101001, 2111004, 2111007, 2111008, 2111009, 2111016, 9250045
        /// <summary>JS 只调用 <c>cm.sendDefault()</c>（无参），或完全无行为。</summary>
        [ScriptName("n1094000", "n2101000", "n2101001", "n2111004", "n2111007", "n2111008", "n2111009", "n2111016", "n9250045")]
        public async Task SendDefaultPlain()
        {
            await sendDefault();
        }

        // Npc: 2111001
        /// <summary>JS: <c>cm.sendDefault(3315)</c> —— 带任务检查的默认对话。</summary>
        [ScriptName("n2111001")]
        public async Task NpcScript_2111001()
        {
            await sendDefault(3315);
        }

        // Npc: 2111005
        /// <summary>JS: <c>cm.sendDefault(3333)</c> —— 带任务检查的默认对话。</summary>
        [ScriptName("n2111005")]
        public async Task NpcScript_2111005()
        {
            await sendDefault(3333);
        }

        // Npc: 9040004, 9040008
        [ScriptName("n9040004", "n9040008")]
        public async Task DisplayGuildRanksPlain()
        {
            await displayGuildRanks();
        }
    }
}
