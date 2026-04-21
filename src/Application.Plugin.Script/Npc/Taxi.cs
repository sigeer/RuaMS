using Application.Core.scripting.Infrastructure;

namespace Application.Plugin.Script
{
    internal partial class NpcScript
    {
        int DiscountForNovice(int price)
        {
            return getJobId() == 0 ? price / 10 : price;
        }
        private async Task NormalTaxi((int map, int price)[] options)
        {
            await SayNext("你好，我开的是普通出租车。如果你想安全快速地从一个城镇到另一个城镇，那就乘坐我们的出租车吧。我们很乐意以实惠的价格带你到达目的地。");
            var selStr = "";
            if (getJobId() == 0)
            {
                selStr += "我们对新手有特别九折优惠。";
            }
            selStr += "选择您的目的地，因为费用将因地点而异。#b";
            var option = await SayOption(selStr, options.Select(x => $"#m{x.map}# ({DiscountForNovice(x.price)} 金币)"));
            var cost = DiscountForNovice(options[option].price);
            if (await SayYesNo($"你在这里没有其他事情要做了，是吗？你真的想去#b#m{options[option].map}##k吗？这将花费你#b{cost}金币#k。"))
            {
                if (getMeso() > cost)
                {
                    gainMeso(-cost);
                    warp(options[option].map, 0);
                }
                else
                {
                    await SayNext("你没有足够的金币。很抱歉要这么说，但没有金币的话，你将无法搭乘出租车。");
                }
            }
            else
            {
                await SayNext("这个城镇有很多值得一看的地方。当你需要去另一个城镇的时候，回来找我们。");
            }

        }

        // Npc: 1022001 
        public Task taxi1()
        {
            // (int map, int price)[] all = [(104000000, 1000), (100000000, 1000), (102000000, 1000), (101000000, 800), (103000000, 1000), (120000000, 800)];
            return NormalTaxi([(104000000, 1000), (100000000, 1000), (101000000, 800), (103000000, 1000), (120000000, 800)]);
        }

        // Npc: 1012000 
        public Task taxi2()
        {
            // TODO
            return NormalTaxi([(104000000, 1000), (102000000, 1000), (101000000, 800), (103000000, 1000), (120000000, 800)]);
        }
        // Npc: 1052016 
        public Task taxi3()
        {
            // TODO
            return NormalTaxi([(104000000, 1000), (102000000, 1000), (100000000, 1000), (101000000, 800), (120000000, 800)]);
        }

        // Npc: 1032000 
        public Task taxi4()
        {
            // TODO
            return NormalTaxi([(104000000, 1000), (102000000, 1000), (100000000, 1000), (103000000, 1000), (120000000, 800)]);
        }
        // Npc: 1092014 
        public Task taxi5()
        {
            // TODO
            return NormalTaxi([(104000000, 1000), (102000000, 1000), (100000000, 1000), (101000000, 800), (103000000, 1000)]);
        }

        // Npc: 1002007 
        public Task taxi6()
        {
            // TODO
            return NormalTaxi([(100000000, 1000), (102000000, 1000), (101000000, 800), (103000000, 1000), (120000000, 800)]);
        }

        // Npc: 1002004, 1032005 
        public async Task mTaxi()
        {
            await SayNext("你好！这辆出租车只对VIP客户开放。与普通出租车只能带你去不同的城镇不同，我们提供更好的服务，值得VIP级别的待遇。价格有点高，但是……只需10,000金币，我们就会安全地带你去#b蚁穴#k。");
            var cost = DiscountForNovice(1000);
            if (await SayYesNo(getJobId() == 0 ? $"我们为新手提供 90% 的特别折扣。 蚂蚁广场位于维多利亚大陆中心的地穴深处, 那里是24小时移动商店的所在地。 你想去那里并花费 #b1,000 金币#k 吗?" : "正常费用适用于所有非初学者。 蚂蚁广场位于维多利亚大陆中心的地穴深处, 那里是24小时移动商店的所在地。 你想去那里并花费 #b10,000 金币#k 吗?"))
            {
                if (getMeso() < cost)
                {
                    await SayNext("看来你没有足够的金币. 抱歉，没有它你将无法使用它。");
                }
                else
                {
                    gainMeso(-cost);
                    warp(105070001);
                }
            }
            else
            {
                await SayOK("这个城镇也有很多值得一看的地方。如果你觉得有必要去蚂蚁广场，就来找我们吧。");
            }
        }

        // Npc: 2023000 
        public async Task ossyria_taxi()
        {
            Dictionary<int, (int ToMap, int ToPortal, int Cost)> dict = new()
            {
                {211000000, (211040200,0,10000) },
                {220000000, (220050300,1,25000) },
                {221000000, (220000000,0,25000) },
                {240000000, (240030000,0,65000) },
            };
            var v = dict.GetValueOrDefault(getMapId());
            if (v.ToMap == 0)
            {
                throw new ConversationDiffMapException();
            }
            await SayNext($"你好，这辆出租车能带你去神秘岛的危险地带！费用是#b{v.Cost} 枚金币#k。我知道有点贵，但能避开所有危险区域，绝对物有所值！");

            if (await SayYesNo($"你真的想去#b#m{v.ToMap}##k吗？这将花费你#b{v.Cost}金币#k。"))
            {
                if (getMeso() > v.Cost)
                {
                    gainMeso(-v.Cost);
                    warp(v.ToMap, v.ToPortal);
                }
                else
                {
                    await SayNext("你似乎没有足够的金币。非常抱歉，除非你付款，否则我无法帮助你。多打怪赚更多金币，等你有足够的金币再回来吧。");
                }
            }
            else
            {
                await SayNext("嗯，请仔细考虑一下。这不便宜，但您绝对不会对我们的顶级服务感到失望！");
            }
        }


        /// <summary>
        /// 世界旅游
        /// </summary>
        /// <returns></returns>
        // Npc: 9000020 
        public Task world_trip()
        {
            // TODO
            return Task.CompletedTask;
        }
    }
}
