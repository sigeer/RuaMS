namespace Application.Plugin.Script.Npc
{
    /// <summary>
    /// 网咖自动贩卖机（NPC 1052014）。
    /// 投入「橡皮擦」（4001009~4001014）或「羽毛」（4001158），按投入量换算积分并兑换对应层级的奖品。
    /// 积分规则：每张橡皮擦基础 6 分，第 2/4 号橡皮擦每多 1 个额外 +3 分；羽毛按 0.46 分/个向上取整。
    /// </summary>
    internal partial class NpcScript
    {
        /// <summary>橡皮擦道具起始 ID，共 6 种。</summary>
        const int ERASER_BASE_ID = 4001009;
        const int ERASER_KINDS = 6;
        /// <summary>羽毛，按 0.46 分/个换算。</summary>
        const int FEATHER_ITEM = 4001158;

        static readonly string[] PrizeTierNames = ["第1层", "第2层", "第3层", "第4层", "第5层", "第6层"];

        static readonly int[][] PrizeItems =
        [
            [1302021, 1302024, 1302033, 1082150, 1002419, 2022053, 2022054, 2020032, 2022057, 2022096, 2022097, 2022192, 2020030, 2010005, 2022041, 2030000, 2040100, 2040004, 2040207, 2048004, 4031203, 4000021, 4003005, 4003000, 4003001, 4010000, 4010001, 4010002, 4010005, 4020004],
            [1022073, 1012098, 1012101, 1012102, 1012103, 2022055, 2022056, 2022103, 2020029, 2020032, 2020031, 2022191, 2022016, 2043300, 2043110, 2043800, 2041001, 2040903, 4031203, 4000021, 4003005, 4003000, 4003001, 4010000, 4010001, 4010003, 4010004, 4020004, 3010004, 3010005],
            [1302058, 1372008, 1422030, 1422031, 1022082, 2022279, 2022120, 2001001, 2001002, 2022071, 2022189, 2040914, 2041001, 2041041, 2041308, 4031203, 4000030, 4003005, 4003000, 4010004, 4010006, 4020000, 4020006, 3010002, 3010003],
            [1332029, 1472027, 1462032, 1492019, 2022045, 2022048, 2022094, 2022123, 2022058, 2041304, 2041019, 2040826, 2040758, 4000030, 4003005, 4003000, 4010007, 4011003, 4021003, 3010016, 3010017],
            [1382015, 1382016, 1442044, 1382035, 2022310, 2022068, 2022069, 2022190, 2022047, 2040727, 2040924, 2040501, 4000030, 4003005, 4003000, 4011003, 4011006, 4021004, 3010099],
            [1442046, 1432018, 1102146, 1102145, 2022094, 2022544, 2022123, 2022310, 2040727, 2041058, 2040817, 4000030, 4003005, 4003000, 4011007, 4021009, 4011008, 3010098],
        ];

        static readonly int[][] PrizeQuantities =
        [
            [1, 1, 1, 1, 1, 20, 20, 20, 20, 20, 25, 25, 25, 50, 50, 12, 1, 1, 1, 1, 3, 4, 2, 2, 1, 2, 2, 2, 2, 2],
            [1, 1, 1, 1, 1, 40, 40, 40, 40, 60, 60, 60, 60, 1, 1, 1, 1, 1, 4, 6, 7, 5, 2, 4, 4, 3, 3, 4, 1, 1],
            [1, 1, 1, 1, 1, 65, 40, 40, 40, 25, 25, 1, 1, 1, 1, 10, 7, 10, 8, 5, 5, 5, 5, 1, 1],
            [1, 1, 1, 1, 45, 40, 25, 20, 60, 1, 1, 1, 1, 10, 10, 20, 5, 1, 1, 1, 1],
            [1, 1, 1, 1, 20, 40, 40, 30, 30, 1, 1, 1, 20, 20, 25, 3, 2, 3, 1],
            [1, 1, 1, 1, 35, 15, 20, 20, 1, 1, 1, 30, 30, 30, 1, 1, 3, 1],
        ];

        // Npc: 1052014
        [ScriptName("n1052014")]
        public async Task NpcCafeVendingMachine()
        {
            await SayNext($"这是互联网咖啡厅的自动贩卖机。放置你在任务中获得的橡皮擦或#t{FEATHER_ITEM}#来兑换奖品。你可以放置#b任意数量的橡皮擦#k，但请注意放置#r不同的橡皮擦#k和#r更多数量的任何一种#k会提高奖励的可能性！");

            // 每轮交互都重新读取背包，故状态只在本方法内维护
            int[] tickets = new int[ERASER_KINDS];
            var coins = 0;
            var hasFeather = haveItem(FEATHER_ITEM);

            while (true)
            {
                var tier = GetPrizeTier(tickets, coins);
                var header = tier >= 0
                    ? $"使用您当前放置的物品，您可以检索#r{PrizeTierNames[tier]}#k 奖品。放置橡皮擦:"
                    : "您还没有放置橡皮擦。放置橡皮擦:";

                var list = "";
                for (var i = 0; i < ERASER_KINDS; i++)
                {
                    list += $"#b#L{i}##t{ERASER_BASE_ID + i}##k";
                    if (tickets[i] > 0)
                    {
                        list += $" - {tickets[i]} 橡皮擦";
                    }
                    list += "#l\r\n";
                }

                if (hasFeather)
                {
                    list += $"#b#L{ERASER_KINDS}##t{FEATHER_ITEM}##k";
                    if (coins > 0)
                    {
                        list += $" - {coins} 羽毛";
                    }
                    list += "#l\r\n";
                }

                var claimIndex = hasFeather ? ERASER_KINDS + 1 : ERASER_KINDS;
                var sel = await AskMenu(header + "\r\n\r\n" + list + $"#r#L{claimIndex}#找回奖品！#l#k\r\n");

                if (sel == claimIndex)
                {
                    if (tier < 0)
                    {
                        await SayNext("您没有提交橡皮擦。提交至少一个橡皮擦即可领取奖品。");
                        continue;
                    }

                    await GiveVendingPrize(tier, tickets, hasFeather, coins);
                    return;
                }

                var itemId = sel < ERASER_KINDS ? ERASER_BASE_ID + sel : FEATHER_ITEM;
                var owned = getItemQuantity(itemId);
                if (owned <= 0)
                {
                    await SayNext($" #r你没有可用的 #k #b#t{itemId}##k 提交到机器上。 点击 '#r上项#k' 返回主界面。");
                    continue;
                }

                var input = await AskText($"要将提交多少个 #b#t{itemId}##k 提交到机器上? (剩余#r{owned}#k 可用)#k");
                if (!int.TryParse(input, out var placed) || placed < 0)
                {
                    await SayNext("你必须提交正确橡皮擦数量。 点击 '#r上项#k' 返回主界面。");
                    continue;
                }

                if (placed > owned)
                {
                    await SayNext($"你没有足够的橡皮可提交 (剩余#r{owned}#k 可用). 点击 '#r上项#k' 返回主界面。");
                    continue;
                }

                if (sel < ERASER_KINDS)
                {
                    tickets[sel] = placed;
                }
                else
                {
                    coins = placed;
                }

                await SayNext("操作成功。 点击 '#r上项#k' 返回主界面 #r找回奖品！#k");
            }
        }

        /// <summary>按投入量计算可兑换的奖品层级；0 表示第 1 层，-1 表示没有投入。</summary>
        static int GetPrizeTier(int[] tickets, int coins)
        {
            var points = 0;
            for (var i = 0; i < tickets.Length; i++)
            {
                if (tickets[i] <= 0)
                    continue;

                // 第 2/4 号橡皮擦（索引 1/3）倍数更高
                var multiplier = i == 1 || i == 3 ? 3 : 1;
                points += 6 + (tickets[i] - 1) * multiplier;
            }
            points += (int)Math.Ceiling(0.46 * coins);

            if (points <= 6)
            {
                return points <= 0 ? -1 : 0;
            }

            return points >= 46 ? 5 : (points - 6) / 8;
        }

        async Task GiveVendingPrize(int tier, int[] tickets, bool hasFeather, int coins)
        {
            var items = PrizeItems[tier];
            var quantities = PrizeQuantities[tier];

            if (!items.Zip(quantities).All(x => canHold(x.First, x.Second)))
            {
                await SayOK("检查一下你的背包是否有空间，然后再领取奖品。");
                return;
            }

            var rnd = Random.Shared.Next(items.Length);

            for (var i = 0; i < tickets.Length; i++)
            {
                if (tickets[i] > 0)
                {
                    await gainItem(ERASER_BASE_ID + i, (short)-tickets[i]);
                }
            }

            if (hasFeather && coins > 0)
            {
                await gainItem(FEATHER_ITEM, (short)-coins);
            }

            await gainItem(items[rnd], (short)quantities[rnd]);
        }
    }
}
