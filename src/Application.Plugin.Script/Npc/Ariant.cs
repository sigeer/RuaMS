using Application.Shared.GameProps;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {
        // Npc: 2103000
        public async Task ariant_oasis()
        {
            bool isTigunMorphed()
            {
                return getPlayer().getBuffSource(BuffStat.MORPH) == 2210005;
            }

            if (isQuestStarted(3900) && getQuestProgressInt(3900) != 5)
            {
                await SayOK("#b(你喝了绿洲中的水，感到精神焕发。)");
                setQuestProgress(3900, 5);
            }
            else if (isQuestCompleted(3938))
            {
                if (canHold(2210005))
                {
                    if (!haveItem(2210005) && !isTigunMorphed())
                    {
                        gainItem(2210005, 1);
                        await SayOK("你找到了一缕头发（可能是Tigun的）在水中漂浮着，你捉住了它。记得上次#bJano#k是怎么做的，你制作了一个新的#t2210005#。");
                    }
                }
                else
                {
                    await SayOK("你没有可用的使用槽位。");
                }
            }
            else if (isQuestStarted(3934) || (isQuestCompleted(3934) && !isQuestCompleted(3935)))
            {
                if (canHold(2210005))
                {
                    if (!haveItem(2210005) && !isTigunMorphed())
                    {
                        gainItem(2210005, 1);
                        await SayOK("你成功找到了一只在河流上漂浮的奇怪瓶子。它看起来像是一个模仿城堡卫兵之一的变身瓶，也许你可以用它自由地在城堡内漫游。");
                    }
                }
                else
                {
                    await SayOK("你发现了一只在河流上漂浮的奇怪烧瓶。但你决定忽略它，因为你没有可用的使用槽。");
                }
            }
        }

        // Npc: 2103001
        public async Task secret_wall()
        {
            if (isQuestStarted(3927))
            {
                await SayNext("如果我有一把铁锤和一把匕首，一张弓和一支箭……");
                setQuestProgress(3927, 1);
            }
        }


        // Npc: 2103002
        public async Task ariant_ring()
        {
            if (isQuestStarted(3923) && !haveItem(4031578, 1))
            {
                if (canHold(4031578, 1))
                {
                    await SayOK("你刚刚偷走了戒指。尽快清理这个区域！");
                    gainItem(4031578, 1);
                }
                else
                {
                    await SayOK("你没有可用的ETC槽。");
                }
            }
        }


        // Npc: 2103003
        public async Task ariant_house1()
        {
            await ProcessFoodDepot(0);
        }

        // Npc: 2103004
        public async Task ariant_house2()
        {
            await ProcessFoodDepot(2);
        }

        // Npc: 2103005
        public async Task ariant_house3()
        {
            await ProcessFoodDepot(1);
        }

        // Npc: 2103006
        public async Task ariant_house4()
        {
            await ProcessFoodDepot(3);
        }

        // Npc: 2103009
        public async Task ariant_gold1()
        {
            await ProcessJewelDepot(0);
        }

        // Npc: 2103010
        public async Task ariant_gold2()
        {
            await ProcessJewelDepot(2);
        }

        // Npc: 2103011
        public async Task ariant_gold3()
        {
            await ProcessJewelDepot(1);
        }

        // Npc: 2103012
        public async Task ariant_gold4()
        {
            await ProcessJewelDepot(3);
        }

        private async Task ProcessFoodDepot(int slot)
        {
            if (isQuestStarted(3929))
            {
                var progress = getQuestProgress(3929);
                var ch = progress[slot];

                if (ch == '2')
                {
                    var nextProgress = progress.Substring(0, slot) + '3' + progress.Substring(slot + 1);
                    gainItem(4031580, -1);
                    setQuestProgress(3929, nextProgress);
                }
            }
        }

        private async Task ProcessJewelDepot(int slot)
        {
            if (isQuestStarted(3926))
            {
                var progress = getQuestProgress(3926);
                var ch = progress[slot];

                if (ch == '2')
                {
                    var nextProgress = progress.Substring(0, slot) + '3' + progress.Substring(slot + 1);
                    gainItem(4031579, -1);
                    setQuestProgress(3926, nextProgress);
                }
            }
        }

    }
}
