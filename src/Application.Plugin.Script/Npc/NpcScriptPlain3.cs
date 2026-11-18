using Application.Shared.Constants.Inventory;
using tools;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Npc
{
    /// <summary>
    /// B 组中剩余的、带分支逻辑的 NPC（WZ 无 script 节点，运行期键 <c>n&lt;id&gt;</c>）。
    /// 覆盖：新手村引导、艾琳村村民闲聊、物品兑换商、罗盘商店 NPC 等。
    /// </summary>
    internal partial class NpcScript
    {
        // Npc: 2101004, 2101005, 2101006, 2101007, 2101008, 2101009, 2131004, 2132001, 2132002, 2132003
        /// <summary>
        /// 一句话闲聊 NPC：JS 只是 <c>cm.sendNext("...")</c> 后立即 dispose。
        /// 每段文案不同，这里按 NPC 分派。
        /// </summary>
        [ScriptName("n2101004", "n2101005", "n2101006", "n2101007", "n2101008",
            "n2101009", "n2131004", "n2132001", "n2132002", "n2132003")]
        public async Task VillageGossip()
        {
            var text = getNpc() switch
            {
                2101004 => this.GetTalkMessage(ScriptMessage.Plain3_ItsMuch),
                2101005 => this.GetTalkMessage(ScriptMessage.Plain3_ThinkAriant),
                2101006 => this.GetTalkMessage(ScriptMessage.Plain3_HomeBoring),
                2101007 => this.GetTalkMessage(ScriptMessage.Plain3_AbdullahSaid),
                2101008 => this.GetTalkMessage(ScriptMessage.Plain3_KingQueen),
                2101009 => this.GetTalkMessage(ScriptMessage.Plain3_Yawn),
                2131004 => this.GetTalkMessage(ScriptMessage.Plain3_Zzz),
                2132001 => this.GetTalkMessage(ScriptMessage.Plain3_CallDark),
                2132002 => this.GetTalkMessage(ScriptMessage.Plain3_MagicForest),
                _ => this.GetTalkMessage(ScriptMessage.Plain3_BeingYoung),
            };

            await SayNext(text);
        }

        // Npc: 2131001, 2131003, 2131005, 2131006, 2131007
        /// <summary>
        /// 每 100 个收集道具兑换 1 个「完美音感」（4310000）的 NPC。
        /// 五个 NPC 只有开场白与兑换道具不同，逻辑完全一致。
        /// </summary>
        [ScriptName("n2131001", "n2131003", "n2131005", "n2131006", "n2131007")]
        public async Task ResourceExchange()
        {
            const int rewardItem = 4310000;

            var (exchangeItem, greeting) = getNpc() switch
            {
                2131001 => (4000439, this.GetTalkMessage(ScriptMessage.Plain3_NameStrongest)),
                2131003 => (4000437, this.GetTalkMessage(ScriptMessage.Plain3_ManyWounded)),
                2131005 => (4000436, this.GetTalkMessage(ScriptMessage.Plain3_WishHad)),
                2131006 => (4000440, this.GetTalkMessage(ScriptMessage.Plain3_MonstersAdvancing)),
                _ => (4000438, this.GetTalkMessage(ScriptMessage.Plain3_MonstersFar)),
            };

            await AskMenu(greeting);

            const int required = 100;
            if (!haveItem(exchangeItem, required))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Plain3_DontHave));
                return;
            }

            var maxExchange = Math.Min(300, getItemQuantity(exchangeItem) / required);
            var amount = await AskNumber(
                this.GetTalkMessage(ScriptMessage.Plain3_ExchangeOffer, exchangeItem, exchangeItem, rewardItem, getItemQuantity(exchangeItem)),
                maxExchange, 1, maxExchange);

            if (amount < 1 || amount > maxExchange)
                return;

            if (!canHold(rewardItem, amount))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Plain3_PleaseMake));
                return;
            }

            await gainItem(rewardItem, (short)amount);
            await gainItem(exchangeItem, (short)-(amount * required));
            await SayOK(this.GetTalkMessage(ScriptMessage.Plain3_Thanks));
        }

        // Npc: 1096003
        [ScriptName("n1096003")]
        public async Task CannonshooterDirection()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.Plain3_CooCoo));
            await showIntro("Effect/Direction4.img/cannonshooter/face00");
            await unlockUI();
        }

        // Npc: 1096005
        /// <summary>炮手新手引导：播放起飞特效后传送到 912060300。</summary>
        [ScriptName("n1096005")]
        public async Task CannonshooterLaunch()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.Plain3_AllRight));

            await removeNpcByOid(579711);
            await environmentChange("fire", 0);
            await playSound("cannonshooter/fire");
            await ShowDirectionEffect("Effect/Direction4.img/effect/cannonshooter/flying/0");
            await ShowDirectionEffect("Effect/Direction4.img/effect/cannonshooter/flying1/0");

            await warp(912060300, 0);

            await ShowDirectionEffect("Effect/Direction4.img/effect/cannonshooter/balloon/1");
            await ShowDirectionEffect("Effect/Direction4.img/effect/cannonshooter/balloon/2");
            await showIntro("Effect/Direction4.img/cannonshooter/face04");
            await showIntro("Effect/Direction4.img/cannonshooter/out01");
        }

        // Npc: 1104201
        /// <summary>只有在任务未推进、地图上没有对应怪物与 NPC 时，才召唤 1104002。</summary>
        [ScriptName("n1104201")]
        public async Task SummonEleanor()
        {
            const int questId = 20407;
            const int mobId = 9001010;
            const int npcId = 1104002;

            var questAdvanced = isQuestCompleted(questId)
                || (isQuestStarted(questId) && getQuestProgressInt(questId, mobId) != 0);

            var canSummon = !questAdvanced
                && getMap().countMonsters() == 0
                && getPlayer().getMap().getNPCById(npcId) == null;

            if (!canSummon)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Plain3_SorryCant));
                return;
            }

            await SayOK(this.GetTalkMessage(ScriptMessage.Plain3_Wounded, npcId));
            await spawnNpc(npcId, new System.Drawing.Point(850, 0), getMap());
        }

        // Npc: 9000019
        /// <summary>检查 ETC 栏是否有空位后打开猜拳 NPC 界面。</summary>
        [ScriptName("n9000019")]
        public async Task OpenRockPaperScissors()
        {
            if (getPlayer().getInventory(InventoryType.ETC).getNumFreeSlot() < 1)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Plain3_CheckAvailable));
                return;
            }

            await c.SendPacket(PacketCreator.openRPSNPC());
        }
    }
}
