using Application.Shared.Constants.Inventory;
using tools;

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
                2101004 => "这不算什么#b宫殿#k，但在我们建造更好的宫殿之前，它就能用了！任何人都可以和国王和王后交谈，但不要指望他们对你友善。嗯……也许阿卜杜拉八世国王，如果你碰到他不懒惰、不注意力不集中的时候。",
                2101005 => "我觉得#r艾利安特#k很棒，但瓦尔文戴尔简直太棒了！那里有很多不同的景观和生命形式。就像城外那些奇怪的史莱姆一样……那些简直太奇怪了！",
                2101006 => "家里太无聊了……父母都不理我，让我受不了。自从我们从#rAriant#k搬走后，他们就一直在努力建造新的宫殿，这样他们就不必住在户外了。但我喜欢户外……",
                2101007 => "不行！阿卜杜拉，我说了17间卧室，23间浴室！打电话给建筑公司，改变它！",
                2101008 => "国王和王后最近变得非常专横和苛刻。我只有在每个星期天或他们来访的时候才能见到我的家人。但像我一样，他们很穷，需要黄金币……不知为何提古恩不允许穷人进入。",
                2101009 => "呵欠~!",
                2131004 => "呼呼……",
                2132001 => "叫我黑暗领主吧。我会让盗贼在社会中找到自己的位置……几年后你就会看到！",
                2132002 => "这片森林的魔力太神奇了……",
                _ => "年轻并不意味着我和那些家伙有什么不同。我会向他们展示！",
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
                2131001 => (4000439, "我的名字是#p2131001#，我是这个地方最强大的魔法师。#b\r\n#L0#嘿，拿着这些碎石。你可以用你的魔法对它们施展。#l"),
                2131003 => (4000437, "这么多伤员，这么少的药……#b\r\n#L0#嘿，拿着这些黑色孢子。你可以用它们做出更好的药。#l"),
                2131005 => (4000436, "我希望我有东西可以装这些水……#b\r\n#L0#嘿，拿这些蜗牛壳。你可以用这些来装水。#l"),
                2131006 => (4000440, "怪物们在前进……我无法战斗。我被原始野猪重伤了……#b\r\n#L0#嘿，拿着这些野猪皮。你可以从中恢复。#l"),
                _ => (4000438, "这些怪物太简单了！用我的剑一击就能杀死它们……最好先弄把剑。#b\r\n#L0#嘿，拿着这些树干。你可以用它们来打造一把更好的剑。#l"),
            };

            await AskMenu(greeting);

            const int required = 100;
            if (!haveItem(exchangeItem, required))
            {
                await SayNext("你没有足够的……我至少需要100个。");
                return;
            }

            var maxExchange = Math.Min(300, getItemQuantity(exchangeItem) / required);
            var amount = await AskNumber(
                $"嘿，好主意！你每给我 100 个#i{exchangeItem}##t{exchangeItem}#，我就能给你一个#i{rewardItem}#完美音感。你想要多少？（当前持有：{getItemQuantity(exchangeItem)}）",
                maxExchange, 1, maxExchange);

            if (amount < 1 || amount > maxExchange)
                return;

            if (!canHold(rewardItem, amount))
            {
                await SayOK("请在杂项标签页中腾出一些空间。");
                return;
            }

            await gainItem(rewardItem, (short)amount);
            await gainItem(exchangeItem, (short)-(amount * required));
            await SayOK("谢谢！");
        }

        // Npc: 1096003
        [ScriptName("n1096003")]
        public async Task CannonshooterDirection()
        {
            await SayNext("咕咕！咕咕！");
            await showIntro("Effect/Direction4.img/cannonshooter/face00");
            await unlockUI();
        }

        // Npc: 1096005
        /// <summary>炮手新手引导：播放起飞特效后传送到 912060300。</summary>
        [ScriptName("n1096005")]
        public async Task CannonshooterLaunch()
        {
            await SayNext("好的！让我们走吧！");

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
                await SayOK("抱歉，我无法完成你的要求。");
                return;
            }

            await SayOK($"……嗯…… #b#h0##k，是你吗……？#r#p{npcId}##k……她已经到了…… #b#h0##k，我真的很抱歉，我现在无法在这种状态下帮助你，就在一个更大的威胁出现时，我对我的人民束手无策…… 请，我请求你，请打败她，#b#h0##k！！……");
            await spawnNpc(npcId, new System.Drawing.Point(850, 0), getMap());
        }

        // Npc: 9000019
        /// <summary>检查 ETC 栏是否有空位后打开猜拳 NPC 界面。</summary>
        [ScriptName("n9000019")]
        public async Task OpenRockPaperScissors()
        {
            if (getPlayer().getInventory(InventoryType.ETC).getNumFreeSlot() < 1)
            {
                await SayNext("检查你的杂项物品栏是否有可用的空位。");
                return;
            }

            await c.SendPacket(PacketCreator.openRPSNPC());
        }
    }
}
