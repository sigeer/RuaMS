using Application.Core.scripting.Infrastructure;
using Application.Shared.Constants.Map;

namespace Application.Plugin.Script.Npc
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
            var option = await AskMenu(selStr, options.Select(x => $"#m{x.map}# ({DiscountForNovice(x.price)} 金币)"));
            var cost = DiscountForNovice(options[option].price);
            if (await AskYesNo($"你在这里没有其他事情要做了，是吗？你真的想去#b#m{options[option].map}##k吗？这将花费你#b{cost}金币#k。"))
            {
                if (getMeso() > cost)
                {
                    await gainMeso(-cost);
                    await warp(options[option].map, 0);
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
            return NormalTaxi([(104000000, 1000), (102000000, 1000), (101000000, 800), (103000000, 1000), (120000000, 800)]);
        }
        // Npc: 1052016 
        public Task taxi3()
        {
            return NormalTaxi([(104000000, 1000), (102000000, 1000), (100000000, 1000), (101000000, 800), (120000000, 800)]);
        }

        // Npc: 1032000 
        public Task taxi4()
        {
            return NormalTaxi([(104000000, 1000), (102000000, 1000), (100000000, 1000), (103000000, 1000), (120000000, 800)]);
        }
        // Npc: 1092014 
        public Task taxi5()
        {
            return NormalTaxi([(104000000, 1000), (102000000, 1000), (100000000, 1000), (101000000, 800), (103000000, 1000)]);
        }

        // Npc: 1002007 
        public Task taxi6()
        {
            return NormalTaxi([(100000000, 1000), (102000000, 1000), (101000000, 800), (103000000, 1000), (120000000, 800)]);
        }

        // Npc: 1002004, 1032005 
        public async Task mTaxi()
        {
            await SayNext("你好！这辆出租车只对VIP客户开放。与普通出租车只能带你去不同的城镇不同，我们提供更好的服务，值得VIP级别的待遇。价格有点高，但是……只需10,000金币，我们就会安全地带你去#b蚁穴#k。");
            var cost = DiscountForNovice(1000);
            if (await AskYesNo(getJobId() == 0 ? $"我们为新手提供 90% 的特别折扣。 蚂蚁广场位于维多利亚大陆中心的地穴深处, 那里是24小时移动商店的所在地。 你想去那里并花费 #b1,000 金币#k 吗?" : "正常费用适用于所有非初学者。 蚂蚁广场位于维多利亚大陆中心的地穴深处, 那里是24小时移动商店的所在地。 你想去那里并花费 #b10,000 金币#k 吗?"))
            {
                if (getMeso() < cost)
                {
                    await SayNext("看来你没有足够的金币. 抱歉，没有它你将无法使用它。");
                }
                else
                {
                    await gainMeso(-cost);
                    await warp(105070001);
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

            if (await AskYesNo($"你真的想去#b#m{v.ToMap}##k吗？这将花费你#b{v.Cost}金币#k。"))
            {
                if (getMeso() > v.Cost)
                {
                    await gainMeso(-v.Cost);
                    await warp(v.ToMap, v.ToPortal);
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
        public async Task world_trip()
        {
            string travelAgency = "世界旅游";
            int fromMapID = -1;
            int currentMapID = getPlayer().getMapId();
            int talkIndex = 0;
            bool dynamicsFee = true;
            int FeeMin = 5000;
            int FeeMax = 50000;
            
            var travelConfig = new List<(string Area, string Name, int MapID, int Portal, int Fee, List<string> Desc, int? Npc)>();
            
            // 添加旅行地图
            AddTravelMap(travelConfig, "东方神州", "上海外滩", 701000000, 1, 10000, new List<string> {
                "上海外滩是上海市的标志性景点之一，以其壮丽的天际线而闻名。这里是黄浦江畔的一条长堤路，两旁矗立着众多历史建筑和现代化摩天大楼。",
                "你可以在这里欣赏到浦东陆家嘴的金融区、外白渡桥以及历史悠久的老建筑群。夜晚，这里的灯光璀璨夺目，非常适合散步或拍照留念。",
                "此外，外滩附近还有许多美食街和特色餐厅，你可以品尝到地道的上海小吃，如生煎包、小笼包等。"
            }, null);
            
            AddTravelMap(travelConfig, "东方神州", "嵩山镇", 702000000, 8, 10000, new List<string> {
                "嵩山镇是一个充满历史韵味的小城镇，位于中国的河南省登封市。这里有许多古老的寺庙和文化遗产，其中最著名的是少林寺，被誉为武术发源地之一。",
                "你可以参观少林寺的千年古刹，体验正宗的禅宗文化。在嵩山镇，你还可以欣赏到美丽的山水风光，感受大自然的魅力。",
                "此外，嵩山镇周边有许多传统村落和农家乐，你可以品尝到当地的农家菜，了解中原地区的风土人情。"
            }, null);
            
            AddTravelMap(travelConfig, "新加坡", "驳船码头城", 541000000, 4, 10000, new List<string> {
                "驳船码头城是新加坡的一个充满活力的历史区域，它将现代都市生活与殖民时期的建筑完美融合。这里曾经是繁忙的贸易港口，现在则是夜生活的中心，拥有各种酒吧、餐厅和娱乐场所。",
                "当您漫步在这个充满魅力的地方时，不妨停下来品尝一下当地的美食，如辣椒蟹和海南鸡饭。此外，还可以参加河畔游船之旅，欣赏美丽的天际线。。"
            }, null);
            
            AddTravelMap(travelConfig, "马来西亚", "吉隆大都市", 550000000, 4, 10000, new List<string> {
                "如果你想在乐观的环境中感受热带的炎热，马来西亚的居民非常欢迎你。此外，大都市本身就是当地经济的核心，众所周知，这个地方总是有事情可做或参观。",
                "一到那里，我强烈建议你安排一次去甘榜村的旅行。为什么？你肯定已经知道奇幻主题公园《幽灵世界》了吧？不它只是把最好的主题公园放在那里，值得一游！"
            }, 9201135);
            
            AddTravelMap(travelConfig, "日本", "蘑菇神社", 800000000, 5, 10000, new List<string> {
                "如果你想感受日本的精髓，没有什么比参观日本文化大熔炉更好的了。蘑菇神社是一个神话般的地方，供奉着古代无与伦比的蘑菇神。",
                "看看为蘑菇神服务的女巫，我强烈建议尝试日本街头出售的章鱼烧、烤肉和其他美味的食物。"
            }, null);
            
            fromMapID = getPlayer().PeekSavedLocation(Shared.MapObjects.SavedLocationType.WORLDTOUR);
            
            if (travelConfig.Any(map => map.MapID == currentMapID))
            {
                // 当前在旅游地图
                var option = await AskMenu("旅行怎么样？你喜欢吗？\r\n#b\r\n#L0#是的，我要回去。\r\n#L1#不，我想继续探索这个地方。");
                if (option == 0)
                {
                    await SayOK($"好的，我将送你回到#b#m{fromMapID}##k");
                    if (fromMapID != -1)
                    {
                        await warp(fromMapID);
                    }
                    else
                    {
                        await SayOK("你没有来时的记录！你这个愚蠢的非法偷渡者！");
                    }
                }
                else
                {
                    await SayOK("如果你想回去了可以随时找我，祝你旅途愉快！");
                }
            }
            else
            {
                // 显示旅游地点列表
                var maxAreaLength = travelConfig.Max(map => map.Area.Length);
                var maxNameLength = travelConfig.Max(map => map.Name.Length);
                var text = $"如果你觉得日常生活的节奏有些单调，不妨换个心情，去外面的世界探索一番吧？\r\n没有什么能比沉浸在不同的文化中，每分钟都有新发现更让人愉悦了！是时候计划一次旅行了。\r\n我们#b#e{travelAgency}#k#n强烈建议你来一场#b世界之旅#k！\r\n担心旅行费用吗？别担心！\r\n我们#b#e{travelAgency}#k#n已经为你精心制定了以下旅游计划，让你轻松享受旅程：\r\n\r\n";
                
                for (int i = 0; i < travelConfig.Count; i++)
                {
                    var map = travelConfig[i];
                    var areaPadded = map.Area.PadRight(maxAreaLength, '　');
                    var namePadded = map.Name.PadRight(maxNameLength, '　');
                    var feeFormatted = map.Fee.ToString("N0");
                    text += $"#L{i}##b【{areaPadded}】\t\t{namePadded}#k\t\t#fUI/Basic.img/BtCoin/normal/0#{feeFormatted}#l\r\n";
                }
                
                var selectedIndex = await AskMenu(text);
                if (selectedIndex >= 0 && selectedIndex < travelConfig.Count)
                {
                    var selectedMap = travelConfig[selectedIndex];
                    talkIndex = 0;

                    await SaySpeech(selectedMap.Desc.ToArray());

                    // 确认是否前往
                    if (getMeso() < selectedMap.Fee)
                    {
                        await SayOK("哎呀，看起来你的金币不足，无法进行这次世界之旅。\r\n不过别担心，当你有足够的金币时，随时欢迎来找我！\r\n祝你早日攒够金币，开启一段精彩的旅程！");
                    }
                    else
                    {
                        await gainMeso(-selectedMap.Fee);
                        getPlayer().SaveLocation(Shared.MapObjects.SavedLocationType.WORLDTOUR);
                        await warp(selectedMap.MapID, selectedMap.Portal);
                    }
                }
                else
                {
                    await SayOK("如果你想去旅游，请你不要尝试通过非法渠道旅游。");
                }
            }
        }
        
        private void AddTravelMap(List<(string Area, string Name, int MapID, int Portal, int Fee, List<string> Desc, int? Npc)> travelConfig, string area, string name, int mapID, int portal, int fee, List<string> desc, int? npc)
        {
            var dynamicsFee = true;
            int FeeMin = 5000;
            int FeeMax = 50000;
            int currentMapID = getPlayer().getMapId();
            
            int calculatedFee = fee;
            if (dynamicsFee)
            {
                calculatedFee = CalculateTravelFee(currentMapID, mapID, FeeMin, FeeMax);
            }
            
            var newDesc = new List<string>(desc);
            if (npc.HasValue)
            {
                newDesc.Add($"我们目前为您提供这个旅行地点：#b{name}#k。\r\n将由与我们合作的当地向导 #b#p{npc.Value}##k 在那里作为您的旅行向导为您服务。\r\n请放心，目的地数量将会随着时间的推移而增加。");
            }
            newDesc.Add($"现在让我们立刻前往 #b#e{name}#k#n 吧！出发！");
            
            travelConfig.Add((area, name, mapID, portal, calculatedFee, newDesc, npc));
        }
        
        private int CalculateTravelFee(int mapID, int targetMapID, int feeMin, int feeMax)
        {
            int baseFee = feeMin;
            int maxFee = feeMax;
            double distanceFactor = 0.00004;
            int distance = Math.Abs(targetMapID - mapID);
            double fee = baseFee + (distance * distanceFactor);
            return Math.Min((int)Math.Round(fee), maxFee);
        }


        // Npc: 9201135 
        public async Task Malay_Warp2()
        {
            var currentMap = getMapId();
            var startedTravel = currentMap == 540000000;

            var inMap = new[] { 540000000, 550000000, 551000000 };
            int[][] toMap = [[550000000], [551000000, 541000000], [550000000]];
            int[][] cost = [[42000], [10000, 0], [10000]];
            int[][] toMapSp = [[0], [2, 4], [4]];

            var location = Array.IndexOf(inMap, currentMap);
            if (location < 0)
            {
                await SayOK("我好像不是和你在一个合适的地点见面……如果你想体验马来西亚之旅，可以到马来西亚去找我。");
                return;
            }

            if (currentMap == 550000000)
            {
                var savedLoc = getPlayer().PeekSavedLocation(Shared.MapObjects.SavedLocationType.WORLDTOUR);
                toMap[1][1] = savedLoc != -1 ? savedLoc : 541000000;
            }

            var text = startedTravel
                ? "嘿，我是#p9201135#，是#r马来西亚#k的导游。因为你没有在我们合作伙伴#b枫叶旅行社#k注册我们的特别旅行套餐，所以乘车的价格会贵很多。那么，你现在想乘车吗？\n\n"
                : "嘿，我是#p9201135#，是你#r马来西亚#k的导游。你想去哪里旅行？\n\n";

            var destinations = toMap[location];
            var costs = cost[location];
            var portals = toMapSp[location];

            var options = new Dictionary<int, string>();
            for (int i = 0; i < destinations.Length; i++)
            {
                var costStr = costs[i] > 0 ? $"({costs[i]} 金币)" : "";
                options.Add(i, $"#m{destinations[i]}# {costStr}");
            }

            var sel = await AskMenu(text, options);
            var travelCost = costs[sel];
            var travelMap = destinations[sel];
            var travelSp = portals[sel];

            if (travelCost > 0)
            {
                if (await AskYesNo($"您想前往#b#m{travelMap}##k吗？前往#b#m{travelMap}##k需要花费#r{numberWithCommas(travelCost)}金币#k。您现在要前往吗？"))
                {
                    if (getMeso() < travelCost)
                    {
                        await SayNext("你似乎没有足够的金币。");
                    }
                    else
                    {
                        await gainMeso(-travelCost);
                        if (startedTravel)
                        {
                            getPlayer().SaveLocation(Shared.MapObjects.SavedLocationType.WORLDTOUR);
                        }
                        await warp(travelMap, travelSp);
                    }
                }
                else
                {
                    await SayNext("如果你需要搭车，你知道该来找我了！");
                }
            }
            else
            {
                var savedLoc = getPlayer().GetSavedLocation(Shared.MapObjects.SavedLocationType.WORLDTOUR);
                await warp(savedLoc != -1 ? savedLoc : toMap[1][1], travelSp);
            }
        }

    }
}
