using Application.Core.Channel.DataProviders;
using Application.Shared.Items;
using Application.Shared.Models;
using client.inventory;
using System.Numerics;

namespace Application.Plugin.Script
{
    internal partial class NpcScript
    {
        // Npc: 1012002 
        public async Task refine_henesys()
        {
            var option = await SayOption("你好. 我是比休斯，退休的射手，但我曾经是雅典娜皮尔斯顶尖的学生，我不再打猎了，但我可以帮你制作一些对你有帮助的物品...",
                ["制作一把弓", "制作一把弩", "制作一双手套", "升级一双手套", "材料制作", "制作箭矢"]);

            RefineFormula? selected = null;
            int count = 0;
            switch (option)
            {
                case 0:
                    // 通过文件加载配方
                    RefineFormula[] items = [];
                    var selectItemIdx = await SayOption("好眼光,弓的攻击速度快,也比弩灵敏许多,但是攻击比弩低一点点哦，但箭矢和弩没有太大区别。 总之, 你想做哪一种？",
                        items.Select(x => $"#t{x.TargetItemId}# - 需要等级 {ItemInformationProvider.getInstance().getEquipLevelReq(x.TargetItemId)}"));

                    selected = items[selectItemIdx];
                    count = await SayInputNumber("所以，你需要我帮你做一些 #t" + selected.TargetItemId + "#？那你想要我帮你做多少个呢？", 1, 1, 100);
                    var costPromt = string.Join("\r\n", selected.Items.Select(x => $"#i{x.ItemId}# x {x.Quantity * count} #t{x.ItemId}#"));
                    if (selected.Cost > 0)
                    {
                        costPromt += "\r\n#i4031138# " + (selected.Cost * count) + " 金币";
                    }
                    if (!await SayYesNo($"你需要我帮你做{count}个#t{selected.TargetItemId}#？ 如果是那样的话，我需要你提供一些特定的物品才能制作。另外，请确保你的背包中有足够的空间！\r\n\r\n"))
                    {
                        return;
                    }
                    break;
                default:
                    break;
            }

            if (selected != null && count > 0)
            {
                if (getMeso() < (selected.Cost * count))
                {
                    await SayOK("抱歉，但这是我谋生的方式。没有金币，就没有物品。");
                    return;
                }
                else if (!canHold(selected.TargetItemId, count))
                {
                    await SayOK("请确保你的背包有空间，然后再和我交谈。");
                    return;
                }
                else
                {
                    if (selected.Items.All(x => haveItem(x.ItemId, x.Quantity * count)))
                    {
                        foreach (var item in selected.Items)
                        {
                            gainItem(item.ItemId, -item.Quantity * count);
                        }

                        gainMeso(-(selected.Cost * count));
                        gainItem(selected.TargetItemId, count);

                        await SayOK("一如既往，物品完美无缺。如果你需要其他东西，就来找我吧。");
                    }
                }
            }
            // TODO
        }

        // Npc: 1022003 
        public Task refine_perion()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1022004 
        public Task refine_perion2()
        {
            // TODO
            return Task.CompletedTask;
        }

        // Npc: 1032002 
        public Task refine_ellinia()
        {
            // TODO
            return Task.CompletedTask;
        }

        // Npc: 1052002 
        public Task refine_kerning()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1052003 
        public Task refine_kerning2()
        {
            // TODO
            return Task.CompletedTask;
        }

        // Npc: 1061000 
        public Task refine_sleepy()
        {
            // TODO
            return Task.CompletedTask;
        }

        // Npc: 2010000 
        public Task carlie()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2010003 
        public Task make_orbis()
        {
            // TODO
            return Task.CompletedTask;
        }

        // Npc: 2020000 
        public Task refine_elnath()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2020002 
        public Task make_elnath()
        {
            // TODO
            return Task.CompletedTask;
        }

        // Npc: 2040016 
        public Task make_ludi1()
        {
            // TODO
            return Task.CompletedTask;
        }

        // Npc: 2040020 
        public Task make_ludi2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2040021 
        public Task make_ludi3()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2040022 
        public Task make_ludi4()
        {
            // TODO
            return Task.CompletedTask;
        }

        // Npc: 2040050 
        public Task make_ston()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201052 
        public Task refine_TCG1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201095 
        public Task Gear_Upgrade()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201096 
        [ScriptName("Jack_Additional ")]
        public Task s_Jack_Additional()
        {
            // TODO
            return Task.CompletedTask;
        }

        // Npc: 9201097 
        public Task Badge_Bounty()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201098 
        public Task Brewing_Storm()
        {
            // TODO
            return Task.CompletedTask;
        }

        // Npc: 2100001 
        public Task make_ariant1()
        {
            // TODO
            return Task.CompletedTask;
        }

        // Npc: 2090004 
        public Task make_murueng()
        {
            // TODO
            return Task.CompletedTask;
        }
    }
}
