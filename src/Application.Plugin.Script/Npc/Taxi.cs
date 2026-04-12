using System;
using System.Collections.Generic;
using System.Text;

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

    }
}
