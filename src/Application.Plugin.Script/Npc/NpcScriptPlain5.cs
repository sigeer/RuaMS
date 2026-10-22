namespace Application.Plugin.Script.Npc
{
    /// <summary>
    /// 隐藏地图中的合成商人与远程商店 NPC（WZ 无 script 节点，运行期键 <c>n&lt;id&gt;</c>）。
    /// </summary>
    internal partial class NpcScript
    {
        /// <summary>合成产出的「神秘卷轴」。</summary>
        const int SYNTHESIS_RESULT = 2049100;
        /// <summary>合成一次的费用。</summary>
        const int SYNTHESIS_COST = 1_200_000;

        static readonly int[] SynthesisMaterials = [4031203, 4001356, 4000136, 4000082, 4001126, 4080100, 4000021, 4003005];
        static readonly int[] SynthesisMaterialQuantities = [100, 60, 40, 80, 10, 8, 200, 120];

        // Npc: 9000017
        /// <summary>
        /// 花 1,200,000 金币 + 一堆材料合成 #t2049100#。
        /// 只在启用自定义 NPC 脚本时提供该服务，否则只做自我介绍。
        /// </summary>
        [ScriptName("n9000017")]
        public async Task SynthesisMerchant()
        {
            getPlayer().setCS(true);

            if (!Application.Utility.Configs.YamlConfig.config.server.USE_ENABLE_CUSTOM_NPC_SCRIPT)
            {
                await SayOK($"嗨，我是 #b#p{getNpc()}##k。");
                return;
            }

            await SayNext("嘿，旅行者！来，靠近一点……我们为你提供一个#b巨大的商业机会#k。如果你想知道是什么，就继续听下去……");

            if (!await AskYesNo($"我们这里有合成强大的#b#t{SYNTHESIS_RESULT}##k的技术！当然，制作一个并不容易……但别担心！只要给我收集一些材料，再付#b1,200,000 金币#k的服务费，就能#b获得它#k。你还想做吗？"))
            {
                await SayOK("哦，好吧…… 你想做生意的时候再来找我们吧。");
                return;
            }

            var qty = await AskNumber($"那么，你想让我们做几个#t{SYNTHESIS_RESULT}##k？", 1, 1, 100);
            if (qty <= 0)
                qty = 1;

            var prompt = qty == 1
                ? $"你想让我们做一个#t{SYNTHESIS_RESULT}##k吗？"
                : $"你想让我们做 {qty} 个#t{SYNTHESIS_RESULT}##k吗？";
            prompt += " 那样的话，我们需要你提供一些特定材料来制作。不过请确保你的背包有空位！#b";

            for (var i = 0; i < SynthesisMaterials.Length; i++)
            {
                prompt += $"\r\n#i{SynthesisMaterials[i]}# {SynthesisMaterialQuantities[i] * qty} #t{SynthesisMaterials[i]}#";
            }
            prompt += $"\r\n#i4031138# {SYNTHESIS_COST * qty} 金币";

            if (!await AskYesNo(prompt))
                return;

            if (getMeso() < SYNTHESIS_COST * qty)
            {
                await SayOK("快点！我们不是来帮你的忙的！我们都需要钱来过好生活，所以拿钱来让我们做成交并开始合成。");
                return;
            }

            if (!canHold(SYNTHESIS_RESULT, qty))
            {
                await SayOK("在我们交易之前，你没有检查一下你的背包里是否有空位吗？");
                return;
            }

            for (var i = 0; i < SynthesisMaterials.Length; i++)
            {
                if (!haveItem(SynthesisMaterials[i], SynthesisMaterialQuantities[i] * qty))
                {
                    await SayOK("你在开玩笑吧？没有所有的材料在手，我们将无法启动这个过程。去把它们都拿来，然后再来找我们谈！");
                    return;
                }
            }

            for (var i = 0; i < SynthesisMaterials.Length; i++)
            {
                await gainItem(SynthesisMaterials[i], (short)-(SynthesisMaterialQuantities[i] * qty));
            }
            await gainMeso(-SYNTHESIS_COST * qty);
            await gainItem(SYNTHESIS_RESULT, (short)qty);
            await SayOK("哇…… 真不敢相信它起作用了！想想一会儿它竟然能够…… 咳咳。当然它起作用了，我们所有的工作都非常高效！很高兴和你做生意。");
        }

        // Npc: 9030000
        /// <summary>
        /// Fredrick（远程商店取回）。
        /// ⚠ C# 尚未实现远程商店：<c>LoadFredrickRegistry</c> / <c>ShowFredrick</c> 在核心层不存在，
        /// 只有 <c>FredrickHandler</c> 的包处理骨架。此处先给出与 JS 一致的"无物品可回收"分支，
        /// 待远程商店系统移植后再补齐。
        /// </summary>
        [ScriptName("n9030000")]
        public async Task Fredrick()
        {
            await SayOK("你没有任何物品或金币可以取回。");
        }
    }
}
