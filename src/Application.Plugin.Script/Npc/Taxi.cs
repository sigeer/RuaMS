using Application.Core.scripting.Infrastructure;
using Application.Plugin.Script;
using Application.Resources.Messages;

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
            await SayNext(this.GetTalkMessage(ScriptMessage.Taxi_Cab_Intro));
            var selStr = "";
            if (getJobId() == 0)
            {
                selStr += this.GetTalkMessage(ScriptMessage.Taxi_Cab_BeginnerDiscount);
            }
            selStr += this.GetTalkMessage(ScriptMessage.Taxi_Cab_ChooseDest);
            var option = await AskMenu(selStr, options.Select(x => this.GetTalkMessage(ScriptMessage.Taxi_Cab_Option, x.map, DiscountForNovice(x.price))));
            var cost = DiscountForNovice(options[option].price);
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Taxi_Cab_Confirm, options[option].map, cost)))
            {
                if (getMeso() > cost)
                {
                    await gainMeso(-cost);
                    await warp(options[option].map, 0);
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Taxi_Cab_NoMeso));
                }
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Taxi_Cab_KeepExploring));
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
            await SayNext(this.GetTalkMessage(ScriptMessage.Taxi_CabVip));
            var cost = DiscountForNovice(1000);
            if (await AskYesNo(this.GetTalkMessage(getJobId() == 0 ? ScriptMessage.Taxi_Vip_ConfirmBeginner : ScriptMessage.Taxi_Vip_ConfirmNormal)))
            {
                if (getMeso() < cost)
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Taxi_Vip_NoMeso));
                }
                else
                {
                    await gainMeso(-cost);
                    await warp(105070001);
                }
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Taxi_Vip_Decline));
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
            await SayNext(this.GetTalkMessage(ScriptMessage.Taxi_Ossyria_Intro, v.Cost));

            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Taxi_Ossyria_Confirm, v.ToMap, v.Cost)))
            {
                if (getMeso() > v.Cost)
                {
                    await gainMeso(-v.Cost);
                    await warp(v.ToMap, v.ToPortal);
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Taxi_DontSeem));
                }
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Taxi_HmmPlease));
            }
        }


        /// <summary>
        /// 世界旅游
        /// </summary>
        /// <returns></returns>
        // Npc: 9000020 
        public async Task world_trip()
        {
            string travelAgency = this.GetTalkMessage(ScriptMessage.Taxi_MapleTravel);
            int fromMapID = -1;
            int currentMapID = getPlayer().getMapId();
            int talkIndex = 0;
            bool dynamicsFee = true;
            int FeeMin = 5000;
            int FeeMax = 50000;

            var travelConfig = new List<(string Area, string Name, int MapID, int Portal, int Fee, List<string> Desc, int? Npc)>();

            // 添加旅行地图
            AddTravelMap(travelConfig, this.GetTalkMessage(ScriptMessage.Taxi_China), this.GetTalkMessage(ScriptMessage.Taxi_BundShanghai), 701000000, 1, 10000, new List<string> {
                this.GetTalkMessage(ScriptMessage.Taxi_BundOne),
                this.GetTalkMessage(ScriptMessage.Taxi_AdmireFinancial),
                this.GetTalkMessage(ScriptMessage.Taxi_BesidesPlenty),
            }, null);

            AddTravelMap(travelConfig, this.GetTalkMessage(ScriptMessage.Taxi_China), this.GetTalkMessage(ScriptMessage.Taxi_SongshanTown), 702000000, 8, 10000, new List<string> {
                this.GetTalkMessage(ScriptMessage.Taxi_SongshanTownSmall),
                this.GetTalkMessage(ScriptMessage.Taxi_VisitThousand),
                this.GetTalkMessage(ScriptMessage.Taxi_BesidesMany),
            }, null);

            AddTravelMap(travelConfig, this.GetTalkMessage(ScriptMessage.Taxi_Singapore), this.GetTalkMessage(ScriptMessage.Taxi_BoatQuay), 541000000, 4, 10000, new List<string> {
                this.GetTalkMessage(ScriptMessage.Taxi_BoatQuayTown),
                this.GetTalkMessage(ScriptMessage.Taxi_StrollThrough),
            }, null);

            AddTravelMap(travelConfig, this.GetTalkMessage(ScriptMessage.Taxi_Malaysia), this.GetTalkMessage(ScriptMessage.Taxi_TrendZone), 550000000, 4, 10000, new List<string> {
                this.GetTalkMessage(ScriptMessage.Taxi_DesireFeel),
                this.GetTalkMessage(ScriptMessage.Taxi_OnceStrongly),
            }, 9201135);

            AddTravelMap(travelConfig, this.GetTalkMessage(ScriptMessage.Taxi_Japan), this.GetTalkMessage(ScriptMessage.Taxi_MushroomShrine), 800000000, 5, 10000, new List<string> {
                this.GetTalkMessage(ScriptMessage.Taxi_DesireFeelEssence),
                this.GetTalkMessage(ScriptMessage.Taxi_CheckOut),
            }, null);

            fromMapID = getPlayer().PeekSavedLocation(Shared.MapObjects.SavedLocationType.WORLDTOUR);

            if (travelConfig.Any(map => map.MapID == currentMapID))
            {
                // 当前在旅游地图
                var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Taxi_Tour_Menu));
                if (option == 0)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Taxi_Tour_GoingBack, fromMapID));
                    if (fromMapID != -1)
                    {
                        await warp(fromMapID);
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Taxi_HaveRecord));
                    }
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Taxi_Tour_Stay));
                }
            }
            else
            {
                // 显示旅游地点列表
                var maxAreaLength = travelConfig.Max(map => map.Area.Length);
                var maxNameLength = travelConfig.Max(map => map.Name.Length);
                var text = this.GetTalkMessage(ScriptMessage.Taxi_Tour_Intro, travelAgency);

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
                        await SayOK(this.GetTalkMessage(ScriptMessage.Taxi_SeemsDont));
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
                    await SayOK(this.GetTalkMessage(ScriptMessage.Taxi_WantTravel));
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
                newDesc.Add(this.GetTalkMessage(ScriptMessage.Taxi_Tour_GuideInfo, name, npc.Value));
            }
            newDesc.Add(this.GetTalkMessage(ScriptMessage.Taxi_Tour_GoNow, name));

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
                await SayOK(this.GetTalkMessage(ScriptMessage.Taxi_SeemsWere));
                return;
            }

            if (currentMap == 550000000)
            {
                var savedLoc = getPlayer().PeekSavedLocation(Shared.MapObjects.SavedLocationType.WORLDTOUR);
                toMap[1][1] = savedLoc != -1 ? savedLoc : 541000000;
            }

            var text = startedTravel
                ? this.GetTalkMessage(ScriptMessage.Taxi_Malay_IntroUnregistered)
                : this.GetTalkMessage(ScriptMessage.Taxi_ImTourGuide);

            var destinations = toMap[location];
            var costs = cost[location];
            var portals = toMapSp[location];

            var options = new Dictionary<int, string>();
            for (int i = 0; i < destinations.Length; i++)
            {
                var costStr = costs[i] > 0 ? this.GetTalkMessage(ScriptMessage.Taxi_Malay_Cost, costs[i]) : "";
                options.Add(i, $"#m{destinations[i]}# {costStr}");
            }

            var sel = await AskMenu(text, options);
            var travelCost = costs[sel];
            var travelMap = destinations[sel];
            var travelSp = portals[sel];

            if (travelCost > 0)
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Taxi_Malay_Confirm, travelMap, travelMap, numberWithCommas(travelCost))))
                {
                    if (getMeso() < travelCost)
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Taxi_SeemHave));
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
                    await SayNext(this.GetTalkMessage(ScriptMessage.Taxi_KnowCome));
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
