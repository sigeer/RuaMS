
using Application.Plugin.Script;
using Application.Resources.Messages;

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
                await SayOK(this.GetTalkMessage(ScriptMessage.Synth_IntroSelf, getNpc()));
                return;
            }

            await SayNext(this.GetTalkMessage(ScriptMessage.Synth_Pitch));

            if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.Synth_Offer, SYNTHESIS_RESULT, SYNTHESIS_RESULT, SYNTHESIS_COST)))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Synth_Decline));
                return;
            }

            var qty = await AskNumber(this.GetTalkMessage(ScriptMessage.Synth_AskQty, SYNTHESIS_RESULT), 1, 1, 100);
            if (qty <= 0)
                qty = 1;

            var prompt = qty == 1
                ? this.GetTalkMessage(ScriptMessage.Synth_ConfirmOne, SYNTHESIS_RESULT)
                : this.GetTalkMessage(ScriptMessage.Synth_ConfirmMany, qty, SYNTHESIS_RESULT);
            prompt += this.GetTalkMessage(ScriptMessage.Synth_MaterialIntro);

            for (var i = 0; i < SynthesisMaterials.Length; i++)
            {
                prompt += $"\r\n#i{SynthesisMaterials[i]}# {SynthesisMaterialQuantities[i] * qty} #t{SynthesisMaterials[i]}#";
            }
            prompt += this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_Meso, SYNTHESIS_COST * qty);

            if (!await AskYesNo(prompt))
                return;

            if (getMeso() < SYNTHESIS_COST * qty)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Synth_NoMeso));
                return;
            }

            if (!canHold(SYNTHESIS_RESULT, qty))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Synth_NoSlot));
                return;
            }

            for (var i = 0; i < SynthesisMaterials.Length; i++)
            {
                if (!haveItem(SynthesisMaterials[i], SynthesisMaterialQuantities[i] * qty))
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Synth_NoMaterial));
                    return;
                }
            }

            for (var i = 0; i < SynthesisMaterials.Length; i++)
            {
                await gainItem(SynthesisMaterials[i], (short)-(SynthesisMaterialQuantities[i] * qty));
            }
            await gainMeso(-SYNTHESIS_COST * qty);
            await gainItem(SYNTHESIS_RESULT, (short)qty);
            await SayOK(this.GetTalkMessage(ScriptMessage.Synth_Success));
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
            await SayOK(this.GetTalkMessage(ScriptMessage.Fredrick_MerchantOpen));
        }
    }
}
