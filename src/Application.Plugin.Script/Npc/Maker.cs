using Application.Core.Channel.DataProviders;
using Application.Shared.Constants.Item;
using Application.Shared.Models;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {
        private class RecipeCategory
        {
            public string Name { get; init; } = string.Empty;
            public string SelectionPrompt { get; init; } = string.Empty;
            public List<RefineFormula> Formulas { get; init; } = [];
            public bool AskQuantity { get; init; } = true;
            public Func<int, int, int> GetResultQuantity { get; init; } = (itemId, count) => count;
        }

        private async Task PerformRefine(string greeting, List<RecipeCategory> categories, float discount = 1, int stimulatorId = 0)
        {
            var categoryIdx = await AskMenu(greeting, categories.Select(x => x.Name));
            if (categoryIdx < 0 || categoryIdx >= categories.Count) return;

            var category = categories[categoryIdx];
            var selectItemIdx = await AskMenu(category.SelectionPrompt,
                category.Formulas.Select(x => ItemConstants.isEquipment(x.TargetItemId)
                ? this.GetTalkMessage(ScriptMessage.Maker_Refine_LevelRequirement, x.TargetItemId, ItemInformationProvider.getInstance().getEquipLevelReq(x.TargetItemId))
                : $"#t{x.TargetItemId}#"));

            if (selectItemIdx < 0 || selectItemIdx >= category.Formulas.Count) return;

            var selected = category.Formulas[selectItemIdx];
            int count = 1;
            if (category.AskQuantity)
            {
                count = await AskNumber(this.GetTalkMessage(ScriptMessage.Maker_Refine_QuantityPrompt, selected.TargetItemId), 1, 1, 100);
            }

            var costPromt = string.Join("\r\n", selected.Items.Select(x => $"#i{x.ItemId}# x {x.Quantity * count} #t{x.ItemId}#"));
            if (stimulatorId > 0)
            {
                costPromt += $"\r\n#i{stimulatorId}# x {count} #t{stimulatorId}#";
            }

            var cost = (int)(discount * selected.Cost);
            if (cost > 0)
            {
                costPromt += this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_Meso, cost * count);
            }

            if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.Maker_CountPrompt, count, selected.TargetItemId)))
            {
                return;
            }

            if (getMeso() < (cost * count))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Result_NoMeso));
                return;
            }

            var resultQty = category.GetResultQuantity(selected.TargetItemId, count);
            if (!canHold(selected.TargetItemId, resultQty))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_CheckInventory));
                return;
            }

            bool hasMaterials = selected.Items.All(x => haveItem(x.ItemId, x.Quantity * count));
            if (stimulatorId > 0 && !haveItem(stimulatorId, count))
            {
                hasMaterials = false;
            }

            if (hasMaterials)
            {
                foreach (var item in selected.Items)
                {
                    await gainItem(item.ItemId, -item.Quantity * count);
                }

                if (stimulatorId > 0)
                {
                    await gainItem(stimulatorId, -count);
                }

                await gainMeso(-(cost * count));

                if (stimulatorId > 0 && Random.Shared.Next(10) == 0)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Result_TooManyAdditives));
                }
                else
                {
                    await gainItem(selected.TargetItemId, (short)resultQty, stimulatorId > 0, true);
                    await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Result_Success));
                }
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Result_MissingItems));
            }
        }

        // Npc: 1012002 
        public async Task refine_henesys()
        {
            var categories = new List<RecipeCategory>
            {
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_CreateBow),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_HaveSniper),
                    Formulas = [
                        new(1452002, [new(4003001, 5), new(4000000, 30)], 800),
                        new(1452003, [new(4011001, 1), new(4003000, 3)], 2000),
                        new(1452001, [new(4003001, 30), new(4000016, 50)], 3000),
                        new(1452000, [new(4011001, 2), new(4021006, 2), new(4003000, 8)], 5000),
                        new(1452005, [new(4011001, 5), new(4011006, 5), new(4021003, 3), new(4021006, 3), new(4003000, 30)], 30000),
                        new(1452006, [new(4011004, 7), new(4021000, 6), new(4021004, 3), new(4003000, 35)], 40000),
                        new(1452007, [new(4021008, 1), new(4011001, 10), new(4011006, 3), new(4003000, 40), new(4000014, 50)], 80000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_CreateCrossbow),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_SniperCrossbows),
                    Formulas = [
                        new(1462001, [new(4003001, 7), new(4003000, 2)], 1000),
                        new(1462002, [new(4011001, 1), new(4003001, 20), new(4003000, 5)], 2000),
                        new(1462003, [new(4011001, 1), new(4003001, 50), new(4003000, 8)], 3000),
                        new(1462000, [new(4011001, 2), new(4021006, 1), new(4021002, 1), new(4003000, 10)], 10000),
                        new(1462004, [new(4011001, 5), new(4011005, 5), new(4021006, 3), new(4003001, 50), new(4003000, 15)], 30000),
                        new(1462005, [new(4021008, 1), new(4011001, 8), new(4011006, 4), new(4021006, 2), new(4003000, 30)], 50000),
                        new(1462006, [new(4021008, 2), new(4011004, 6), new(4003001, 30), new(4003000, 30)], 80000),
                        new(1462007, [new(4021008, 2), new(4011006, 5), new(4021006, 3), new(4003001, 40), new(4003000, 40)], 200000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_MakeGlove),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_GloveWant),
                    Formulas = [
                        new(1082012, [new(4000021, 15), new(4000009, 20)], 5000),
                        new(1082013, [new(4000021, 20), new(4000009, 20), new(4011001, 2)], 10000),
                        new(1082016, [new(4000021, 40), new(4000009, 50), new(4011006, 2)], 15000),
                        new(1082048, [new(4000021, 50), new(4011006, 2), new(4021001, 1)], 20000),
                        new(1082068, [new(4011000, 1), new(4011001, 3), new(4000021, 60), new(4003000, 15)], 30000),
                        new(1082071, [new(4011001, 3), new(4021000, 1), new(4021002, 3), new(4000021, 80), new(4003000, 25)], 40000),
                        new(1082084, [new(4011004, 3), new(4011006, 1), new(4021002, 2), new(4000030, 40), new(4003000, 35)], 50000),
                        new(1082089, [new(4011006, 2), new(4011007, 1), new(4021006, 8), new(4000030, 50), new(4003000, 50)], 70000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_UpgradeGlove),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_UpgradeGloveShouldnt),
                    Formulas = [
                        new(1082015, [new(1082013, 1), new(4021003, 2)], 7000),
                        new(1082014, [new(1082013, 1), new(4021000, 1)], 7000),
                        new(1082017, [new(1082016, 1), new(4021000, 3)], 10000),
                        new(1082018, [new(1082016, 1), new(4021008, 1)], 12000),
                        new(1082049, [new(1082048, 1), new(4021003, 3)], 15000),
                        new(1082050, [new(1082048, 1), new(4021008, 1)], 20000),
                        new(1082069, [new(1082068, 1), new(4011002, 4)], 22000),
                        new(1082070, [new(1082068, 1), new(4011006, 2)], 25000),
                        new(1082072, [new(1082071, 1), new(4011006, 4)], 30000),
                        new(1082073, [new(1082071, 1), new(4021008, 2)], 40000),
                        new(1082085, [new(1082084, 1), new(4011000, 1), new(4021000, 5)], 55000),
                        new(1082083, [new(1082084, 1), new(4011006, 2), new(4021008, 2)], 60000),
                        new(1082090, [new(1082089, 1), new(4021000, 5), new(4021007, 1)], 70000),
                        new(1082091, [new(1082089, 1), new(4021007, 2), new(4021008, 2)], 80000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_CreateMaterials),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1022004_MaterialsKnow),
                    Formulas = [
                        new(4003001, [new(4000003, 10)], 0),
                        new(4003001, [new(4000018, 5)], 0),
                        new(4003000, [new(4011000, 1), new(4011001, 1)], 0)
                    ],
                    GetResultQuantity = (itemId, count) => itemId == 4003000 ? count * 15 : count
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_CreateArrows),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_ArrowsProblem),
                    Formulas = [
                        new(2060000, [new(4003001, 1), new(4003004, 1)], 0),
                        new(2061000, [new(4003001, 1), new(4003004, 1)], 0),
                        new(2060001, [new(4011000, 1), new(4003001, 3), new(4003004, 10)], 0),
                        new(2061001, [new(4011000, 1), new(4003001, 3), new(4003004, 10)], 0),
                        new(2060002, [new(4011001, 1), new(4003001, 5), new(4003005, 15)], 0),
                        new(2061002, [new(4011001, 1), new(4003001, 5), new(4003005, 15)], 0)
                    ],
                    GetResultQuantity = (itemId, count) => {
                        if (itemId >= 2060000 && itemId <= 2060002) return (1000 - (itemId - 2060000) * 100) * count;
                        if (itemId >= 2061000 && itemId <= 2061002) return (1000 - (itemId - 2061000) * 100) * count;
                        return count;
                    }
                }
            };

            await PerformRefine(this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_ViciousRetired, npc), categories);
        }

        // Npc: 1022003 
        public async Task refine_perion()
        {
            var categories = new List<RecipeCategory>
            {
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1022003_RefineMineral),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1022003_KindMineral),
                    Formulas = [
                        new(4011000, [new(4010000, 10)], 300),
                        new(4011001, [new(4010001, 10)], 300),
                        new(4011002, [new(4010002, 10)], 300),
                        new(4011003, [new(4010003, 10)], 500),
                        new(4011004, [new(4010004, 10)], 500),
                        new(4011005, [new(4010005, 10)], 500),
                        new(4011006, [new(4010006, 10)], 800)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1022003_RefineJewel),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1022003_KindJewel),
                    Formulas = [
                        new(4021000, [new(4020000, 10)], 500),
                        new(4021001, [new(4020001, 10)], 500),
                        new(4021002, [new(4020002, 10)], 500),
                        new(4021003, [new(4020003, 10)], 500),
                        new(4021004, [new(4020004, 10)], 500),
                        new(4021005, [new(4020005, 10)], 500),
                        new(4021006, [new(4020006, 10)], 500),
                        new(4021007, [new(4020007, 10)], 1000),
                        new(4021008, [new(4020008, 10)], 3000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1022003_UpgradeHelmet),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1022003_AhWish),
                    AskQuantity = false,
                    Formulas = [
                        new(1002042, [new(1002001, 1), new(4011002, 1)], 500),
                        new(1002041, [new(1002001, 1), new(4021006, 1)], 300),
                        new(1002002, [new(1002043, 1), new(4011001, 1)], 500),
                        new(1002044, [new(1002043, 1), new(4011002, 1)], 800),
                        new(1002003, [new(1002039, 1), new(4011001, 1)], 500),
                        new(1002040, [new(1002039, 1), new(4011002, 1)], 800),
                        new(1002007, [new(1002051, 1), new(4011001, 2)], 1000),
                        new(1002052, [new(1002051, 1), new(4011002, 2)], 1500),
                        new(1002011, [new(1002059, 1), new(4011001, 3)], 1500),
                        new(1002058, [new(1002059, 1), new(4011002, 3)], 2000),
                        new(1002009, [new(1002055, 1), new(4011001, 3)], 1500),
                        new(1002056, [new(1002055, 1), new(4011002, 3)], 2000),
                        new(1002087, [new(1002027, 1), new(4011002, 4)], 2000),
                        new(1002088, [new(1002027, 1), new(4011006, 4)], 4000),
                        new(1002050, [new(1002005, 1), new(4011005, 5)], 4000),
                        new(1002049, [new(1002005, 1), new(4011006, 5)], 5000),
                        new(1002047, [new(1002004, 1), new(4021000, 3)], 8000),
                        new(1002048, [new(1002004, 1), new(4021005, 3)], 10000),
                        new(1002099, [new(1002021, 1), new(4011002, 5)], 12000),
                        new(1002098, [new(1002021, 1), new(4011006, 6)], 15000),
                        new(1002085, [new(1002086, 1), new(4011002, 5)], 20000),
                        new(1002028, [new(1002086, 1), new(4011004, 4)], 25000),
                        new(1002022, [new(1002100, 1), new(4011007, 1), new(4011001, 7)], 30000),
                        new(1002101, [new(1002100, 1), new(4011007, 1), new(4011002, 7)], 30000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1022003_UpgradeShield),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1022003_AhWishUpgrade),
                    AskQuantity = false,
                    Formulas = [
                        new(1092014, [new(1092012, 1), new(4011003, 10)], 100000),
                        new(1092013, [new(1092012, 1), new(4011002, 10)], 100000),
                        new(1092010, [new(1092009, 1), new(4011007, 1), new(4011004, 15)], 120000),
                        new(1092011, [new(1092009, 1), new(4011007, 1), new(4011003, 15)], 120000)
                    ]
                }
            };

            await PerformRefine(this.GetTalkMessage(ScriptMessage.Maker_Npc1022003_HmYouve), categories);
        }


        // Npc: 1022004 
        public async Task refine_perion2()
        {
            var categories = new List<RecipeCategory>
            {
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_MakeGlove),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_GloveWant),
                    AskQuantity = false,
                    Formulas = [
                        new(1082003, [new(4000021, 15), new(4011001, 1)], 1000),
                        new(1082000, [new(4011001, 2)], 2000),
                        new(1082004, [new(4000021, 40), new(4011000, 2)], 5000),
                        new(1082001, [new(4011001, 2)], 10000),
                        new(1082007, [new(4011000, 3), new(4011001, 2), new(4003000, 15)], 20000),
                        new(1082008, [new(4000021, 30), new(4011001, 4), new(4003000, 15)], 30000),
                        new(1082023, [new(4000021, 50), new(4011001, 5), new(4003000, 40)], 40000),
                        new(1082009, [new(4011001, 3), new(4021007, 2), new(4000030, 30), new(4003000, 45)], 50000),
                        new(1082059, [new(4011007, 1), new(4011000, 8), new(4011006, 2), new(4000030, 50), new(4003000, 50)], 70000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_UpgradeGlove),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_UpgradeGloveShouldnt),
                    AskQuantity = false,
                    Formulas = [
                        new(1082005, [new(1082007, 1), new(4011001, 1)], 20000),
                        new(1082006, [new(1082007, 1), new(4011005, 2)], 25000),
                        new(1082035, [new(1082008, 1), new(4021006, 3)], 30000),
                        new(1082036, [new(1082008, 1), new(4021008, 1)], 40000),
                        new(1082024, [new(1082023, 1), new(4011003, 4)], 45000),
                        new(1082025, [new(1082023, 1), new(4021008, 2)], 50000),
                        new(1082010, [new(1082009, 1), new(4011002, 5)], 55000),
                        new(1082011, [new(1082009, 1), new(4011006, 4)], 60000),
                        new(1082060, [new(1082059, 1), new(4011002, 3), new(4021005, 5)], 70000),
                        new(1082061, [new(1082059, 1), new(4021007, 2), new(4021008, 2)], 80000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_CreateMaterials),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1022004_MaterialsKnow),
                    Formulas = [
                        new(4003001, [new(4000003, 10)], 0),
                        new(4003001, [new(4000018, 5)], 0),
                        new(4003000, [new(4011000, 1), new(4011001, 1)], 0)
                    ],
                    GetResultQuantity = (itemId, count) => itemId == 4003000 ? count * 15 : count
                }
            };

            await PerformRefine(this.GetTalkMessage(ScriptMessage.Maker_Npc1022004_UmIm), categories);
        }

        // Npc: 1032002 
        public async Task refine_ellinia()
        {
            var categories = new List<RecipeCategory>
            {
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_MakeGlove),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1032002_KindGlove),
                    AskQuantity = false,
                    Formulas = [
                        new(1082019, [new(4000021, 15)], 7000),
                        new(1082020, [new(4000021, 30), new(4011001, 1)], 15000),
                        new(1082026, [new(4000021, 50), new(4011006, 2)], 20000),
                        new(1082051, [new(4000021, 60), new(4021006, 1), new(4021000, 2)], 25000),
                        new(1082054, [new(4000021, 70), new(4011006, 1), new(4011001, 3), new(4021000, 2)], 30000),
                        new(1082062, [new(4000021, 80), new(4021000, 3), new(4021006, 3), new(4003000, 30)], 40000),
                        new(1082081, [new(4021000, 3), new(4011006, 2), new(4000030, 35), new(4003000, 40)], 50000),
                        new(1082086, [new(4011007, 1), new(4011001, 8), new(4021007, 1), new(4000030, 50), new(4003000, 50)], 70000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_UpgradeGlove),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1032002_KindGloveLooking),
                    AskQuantity = false,
                    Formulas = [
                        new(1082021, [new(1082020, 1), new(4011001, 1)], 20000),
                        new(1082022, [new(1082020, 1), new(4021001, 2)], 25000),
                        new(1082027, [new(1082026, 1), new(4021000, 3)], 30000),
                        new(1082028, [new(1082026, 1), new(4021008, 1)], 40000),
                        new(1082052, [new(1082051, 1), new(4021005, 3)], 35000),
                        new(1082053, [new(1082051, 1), new(4021008, 1)], 40000),
                        new(1082055, [new(1082054, 1), new(4021005, 3)], 40000),
                        new(1082056, [new(1082054, 1), new(4021008, 1)], 45000),
                        new(1082063, [new(1082062, 1), new(4021002, 4)], 45000),
                        new(1082064, [new(1082062, 1), new(4021008, 2)], 50000),
                        new(1082082, [new(1082081, 1), new(4021002, 5)], 55000),
                        new(1082080, [new(1082081, 1), new(4021008, 3)], 60000),
                        new(1082087, [new(1082086, 1), new(4011004, 3), new(4011006, 5)], 70000),
                        new(1082088, [new(1082086, 1), new(4021008, 2), new(4011006, 3)], 80000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1032002_UpgradeHat),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1032002_HatOne),
                    AskQuantity = false,
                    Formulas = [
                        new(1002065, [new(1002064, 1), new(4011001, 3)], 40000),
                        new(1002013, [new(1002064, 1), new(4011006, 3)], 50000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1032002_MakeWand),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1032002_WandHuh),
                    AskQuantity = false,
                    Formulas = [
                        new(1372005, [new(4003001, 5)], 1000),
                        new(1372006, [new(4003001, 10), new(4000001, 50)], 3000),
                        new(1372002, [new(4011001, 1), new(4000009, 30), new(4003000, 5)], 5000),
                        new(1372004, [new(4011002, 2), new(4003002, 1), new(4003000, 10)], 12000),
                        new(1372003, [new(4011002, 3), new(4021002, 1), new(4003000, 10)], 30000),
                        new(1372001, [new(4021006, 5), new(4011002, 3), new(4011001, 1), new(4003000, 15)], 60000),
                        new(1372000, [new(4021006, 5), new(4021005, 5), new(4021007, 1), new(4003003, 1), new(4003000, 20)], 120000),
                        new(1372007, [new(4011006, 4), new(4021003, 3), new(4021007, 2), new(4021002, 1), new(4003002, 1), new(4003000, 30)], 200000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1032002_MakeStaff),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1032002_AhStaff),
                    AskQuantity = false,
                    Formulas = [
                        new(1382000, [new(4003001, 5)], 2000),
                        new(1382003, [new(4021005, 1), new(4011001, 1), new(4003000, 5)], 2000),
                        new(1382005, [new(4021003, 1), new(4011001, 1), new(4003000, 5)], 2000),
                        new(1382004, [new(4003001, 50), new(4011001, 1), new(4003000, 10)], 5000),
                        new(1382002, [new(4021006, 2), new(4021001, 1), new(4011001, 1), new(4003000, 15)], 12000),
                        new(1382001, [new(4011001, 8), new(4021006, 5), new(4021001, 5), new(4021005, 5), new(4003000, 30), new(4000010, 50), new(4003003, 1)], 180000)
                    ]
                }
            };

            await PerformRefine(this.GetTalkMessage(ScriptMessage.Maker_Npc1032002_WelcomeEco), categories);
        }

        // Npc: 1052002 
        public async Task refine_kerning()
        {
            var categories = new List<RecipeCategory>
            {
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_MakeGlove),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1032002_KindGlove),
                    AskQuantity = false,
                    Formulas = [
                        new(1082002, [new(4000021, 15)], 1000),
                        new(1082029, [new(4000021, 30), new(4000018, 20)], 7000),
                        new(1082030, [new(4000021, 30), new(4000015, 20)], 7000),
                        new(1082031, [new(4000021, 30), new(4000020, 20)], 7000),
                        new(1082032, [new(4011000, 2), new(4000021, 40)], 10000),
                        new(1082037, [new(4011000, 2), new(4011001, 1), new(4000021, 10)], 15000),
                        new(1082042, [new(4011001, 2), new(4000021, 50), new(4003000, 10)], 25000),
                        new(1082046, [new(4011001, 3), new(4011000, 1), new(4000021, 60), new(4003000, 15)], 30000),
                        new(1082075, [new(4021000, 3), new(4000014, 200), new(4000021, 80), new(4003000, 30)], 40000),
                        new(1082065, [new(4021005, 3), new(4021008, 1), new(4000030, 40), new(4003000, 30)], 50000),
                        new(1082092, [new(4011007, 1), new(4011000, 8), new(4021007, 1), new(4000030, 50), new(4003000, 50)], 70000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_UpgradeGlove),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1052002_UpgradedGlove),
                    AskQuantity = false,
                    Formulas = [
                        new(1082033, [new(1082032, 1), new(4011002, 1)], 5000),
                        new(1082034, [new(1082032, 1), new(4021004, 1)], 7000),
                        new(1082038, [new(1082037, 1), new(4011002, 2)], 10000),
                        new(1082039, [new(1082037, 1), new(4021004, 2)], 12000),
                        new(1082043, [new(1082042, 1), new(4011004, 2)], 15000),
                        new(1082044, [new(1082042, 1), new(4011006, 1)], 20000),
                        new(1082047, [new(1082046, 1), new(4011005, 3)], 22000),
                        new(1082045, [new(1082046, 1), new(4011006, 2)], 25000),
                        new(1082076, [new(1082075, 1), new(4011006, 4)], 40000),
                        new(1082074, [new(1082075, 1), new(4021008, 2)], 50000),
                        new(1082067, [new(1082065, 1), new(4021000, 5)], 55000),
                        new(1082066, [new(1082065, 1), new(4011006, 2), new(4021008, 1)], 60000),
                        new(1082093, [new(1082092, 1), new(4011001, 7), new(4000014, 200)], 70000),
                        new(1082094, [new(1082092, 1), new(4011006, 7), new(4000027, 150)], 80000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1052002_CreateClaw),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1052002_KindClaw),
                    AskQuantity = false,
                    Formulas = [
                        new(1472001, [new(4011001, 1), new(4000021, 20), new(4003000, 5)], 2000),
                        new(1472004, [new(4011000, 2), new(4011001, 1), new(4000021, 30), new(4003000, 10)], 3000),
                        new(1472007, [new(1472000, 1), new(4011001, 3), new(4000021, 20), new(4003001, 30)], 5000),
                        new(1472008, [new(4011000, 3), new(4011001, 2), new(4000021, 50), new(4003000, 20)], 15000),
                        new(1472011, [new(4011000, 4), new(4011001, 2), new(4000021, 80), new(4003000, 25)], 30000),
                        new(1472014, [new(4011000, 3), new(4011001, 2), new(4000021, 100), new(4003000, 30)], 40000),
                        new(1472018, [new(4011000, 4), new(4011001, 2), new(4000030, 40), new(4003000, 35)], 50000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1052002_UpgradeClaw),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1052002_UpgradedClaw),
                    AskQuantity = false,
                    Formulas = [
                        new(1472002, [new(1472001, 1), new(4011002, 1)], 1000),
                        new(1472003, [new(1472001, 1), new(4011006, 1)], 2000),
                        new(1472005, [new(1472004, 1), new(4011001, 2)], 3000),
                        new(1472006, [new(1472004, 1), new(4011003, 2)], 5000),
                        new(1472009, [new(1472008, 1), new(4011002, 3)], 10000),
                        new(1472010, [new(1472008, 1), new(4011003, 3)], 15000),
                        new(1472012, [new(1472011, 1), new(4011004, 4)], 20000),
                        new(1472013, [new(1472011, 1), new(4021008, 1)], 25000),
                        new(1472015, [new(1472014, 1), new(4021000, 5)], 30000),
                        new(1472016, [new(1472014, 1), new(4011003, 5)], 30000),
                        new(1472017, [new(1472014, 1), new(4021008, 2)], 35000),
                        new(1472019, [new(1472018, 1), new(4021000, 6)], 40000),
                        new(1472020, [new(1472018, 1), new(4021005, 6)], 40000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_CreateMaterials),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1022004_MaterialsKnow),
                    Formulas = [
                        new(4003001, [new(4000003, 10)], 0),
                        new(4003001, [new(4000018, 5)], 0),
                        new(4003000, [new(4011000, 1), new(4011001, 1)], 0)
                    ],
                    GetResultQuantity = (itemId, count) => itemId == 4003000 ? count * 15 : count
                }
            };

            await PerformRefine(this.GetTalkMessage(ScriptMessage.Maker_Npc1052002_PstHave), categories);
        }


        // Npc: 1052003 
        public async Task refine_kerning2()
        {
            var categories = new List<RecipeCategory>
            {
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1022003_RefineMineral),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1022003_KindMineral),
                    Formulas = [
                        new(4011000, [new(4010000, 10)], 300),
                        new(4011001, [new(4010001, 10)], 300),
                        new(4011002, [new(4010002, 10)], 300),
                        new(4011003, [new(4010003, 10)], 500),
                        new(4011004, [new(4010004, 10)], 500),
                        new(4011005, [new(4010005, 10)], 500),
                        new(4011006, [new(4010006, 10)], 800)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1022003_RefineJewel),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1022003_KindJewel),
                    Formulas = [
                        new(4021000, [new(4020000, 10)], 500),
                        new(4021001, [new(4020001, 10)], 500),
                        new(4021002, [new(4020002, 10)], 500),
                        new(4021003, [new(4020003, 10)], 500),
                        new(4021004, [new(4020004, 10)], 500),
                        new(4021005, [new(4020005, 10)], 500),
                        new(4021006, [new(4020006, 10)], 500),
                        new(4021007, [new(4020007, 10)], 1000),
                        new(4021008, [new(4020008, 10)], 3000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1052003_HaveIron),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1052003_KnowAbout),
                    Formulas = [
                        new(4011001, [new(4000039, 100)], 1000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1052002_UpgradeClaw),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1052003_AhWish),
                    AskQuantity = false,
                    Formulas = [
                        new(1472023, [new(1472022, 1), new(4011007, 1), new(4021000, 8), new(2012000, 10)], 80000),
                        new(1472024, [new(1472022, 1), new(4011007, 1), new(4021005, 8), new(2012002, 10)], 80000),
                        new(1472025, [new(1472022, 1), new(4011007, 1), new(4021008, 3), new(4000046, 5)], 100000)
                    ]
                }
            };

            await PerformRefine(this.GetTalkMessage(ScriptMessage.Maker_Npc1052003_OwnForge), categories);
        }

        // Npc: 1091003 
        public async Task refine_nautillus()
        {
            var categories = new List<RecipeCategory>
            {
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1091003_MakeKnuckler),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1091003_LongBring),
                    AskQuantity = false,
                    Formulas = [
                        new(1482001, [new(4000021, 20)], 1000),
                        new(1482002, [new(4011001, 1), new(4011000, 1), new(4000021, 10), new(4003000, 5)], 2000),
                        new(1482003, [new(4011000, 2), new(4011001, 1), new(4003000, 10)], 5000),
                        new(1482004, [new(4011000, 1), new(4011001, 1), new(4000021, 30), new(4003000, 10)], 15000),
                        new(1482005, [new(4011000, 2), new(4011001, 2), new(4000021, 30), new(4003000, 20)], 30000),
                        new(1482006, [new(4011000, 1), new(4011001, 1), new(4021000, 2), new(4000021, 50), new(4003000, 20)], 50000),
                        new(1482007, [new(4000039, 150), new(4011000, 1), new(4011001, 2), new(4000030, 20), new(4000021, 20), new(4003000, 20)], 100000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1091003_MakeGun),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1091003_LongBringMaterials),
                    AskQuantity = false,
                    Formulas = [
                        new(1492001, [new(4011000, 1), new(4003000, 5), new(4003001, 1)], 1000),
                        new(1492002, [new(4011000, 1), new(4003000, 10), new(4003001, 5), new(4000021, 10)], 2000),
                        new(1492003, [new(4011000, 2), new(4003000, 10)], 5000),
                        new(1492004, [new(4011001, 2), new(4000021, 10), new(4003000, 10)], 15000),
                        new(1492005, [new(4011006, 10), new(4011001, 2), new(4000021, 5), new(4003000, 10)], 30000),
                        new(1492006, [new(4011004, 1), new(4011001, 2), new(4000021, 10), new(4003000, 20)], 50000),
                        new(1492007, [new(4011006, 1), new(4011004, 2), new(4011001, 4), new(4000030, 30), new(4003000, 30)], 100000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_MakeGlove),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1091003_LongBringMaterialsRequired),
                    AskQuantity = false,
                    Formulas = [
                        new(1082180, [new(4000021, 15), new(4021003, 1)], 1000),
                        new(1082183, [new(4000021, 35)], 8000),
                        new(1082186, [new(4011000, 2), new(4000021, 20)], 15000),
                        new(1082189, [new(4021006, 2), new(4000021, 50), new(4003000, 10)], 25000),
                        new(1082192, [new(4011000, 3), new(4000021, 60), new(4003000, 15)], 30000),
                        new(1082195, [new(4000021, 80), new(4011000, 3), new(4011001, 3), new(4003000, 25)], 40000),
                        new(1082198, [new(4011000, 3), new(4000021, 20), new(4000030, 40), new(4003000, 30)], 50000),
                        new(1082201, [new(4011007, 1), new(4021008, 1), new(4021007, 1), new(4000030, 50), new(4003000, 50)], 70000)
                    ]
                }
            };

            await PerformRefine(this.GetTalkMessage(ScriptMessage.Maker_Npc1091003_WantMake), categories);
        }

        // Npc: 1061000 
        public async Task refine_sleepy()
        {
            var categories = new List<RecipeCategory>
            {
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1061000_CreateWarrior),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1061000_WarriorShoes),
                    AskQuantity = false,
                    Formulas = [
                        new(1072051, [new(4011004, 2), new(4011001, 1), new(4000021, 15), new(4003000, 10)], 10000),
                        new(1072053, [new(4011006, 2), new(4011001, 1), new(4000021, 15), new(4003000, 10)], 10000),
                        new(1072052, [new(4021008, 1), new(4011001, 2), new(4000021, 20), new(4003000, 10)], 12000),
                        new(1072003, [new(4021003, 4), new(4011001, 2), new(4000021, 45), new(4003000, 15)], 20000),
                        new(1072039, [new(4011002, 4), new(4011001, 2), new(4000021, 45), new(4003000, 15)], 20000),
                        new(1072040, [new(4011004, 4), new(4011001, 2), new(4000021, 45), new(4003000, 15)], 20000),
                        new(1072041, [new(4021000, 4), new(4011001, 2), new(4000021, 45), new(4003000, 15)], 20000),
                        new(1072002, [new(4011001, 3), new(4021004, 1), new(4000021, 30), new(4000030, 20), new(4003000, 25)], 22000),
                        new(1072112, [new(4011002, 3), new(4021004, 1), new(4000021, 30), new(4000030, 20), new(4003000, 25)], 22000),
                        new(1072113, [new(4021008, 2), new(4021004, 1), new(4000021, 30), new(4000030, 20), new(4003000, 25)], 25000),
                        new(1072000, [new(4011003, 4), new(4000021, 40), new(4000030, 30), new(4003000, 100), new(4000033, 100)], 38000),
                        new(1072126, [new(4011005, 4), new(4021007, 1), new(4000030, 40), new(4003000, 30), new(4000042, 250)], 38000),
                        new(1072127, [new(4011002, 4), new(4021007, 1), new(4000030, 40), new(4003000, 30), new(4000041, 120)], 38000),
                        new(1072132, [new(4021008, 1), new(4011001, 3), new(4021003, 6), new(4000030, 65), new(4003000, 45)], 50000),
                        new(1072133, [new(4021008, 1), new(4011001, 3), new(4011002, 6), new(4000030, 65), new(4003000, 45)], 50000),
                        new(1072134, [new(4021008, 1), new(4011001, 3), new(4011005, 6), new(4000030, 65), new(4003000, 45)], 50000),
                        new(1072135, [new(4021008, 1), new(4011001, 3), new(4011006, 6), new(4000030, 65), new(4003000, 45)], 50000),
                        new(1072147, [new(4021008, 1), new(4011007, 1), new(4021005, 8), new(4000030, 80), new(4003000, 55)], 60000),
                        new(1072148, [new(4021008, 1), new(4011007, 1), new(4011005, 8), new(4000030, 80), new(4003000, 55)], 60000),
                        new(1072149, [new(4021008, 1), new(4011007, 1), new(4021000, 8), new(4000030, 80), new(4003000, 55)], 60000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1061000_CreateBowman),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1061000_BowmanShoes),
                    AskQuantity = false,
                    Formulas = [
                        new(1072027, [new(4000021, 35), new(4011000, 3), new(4003000, 10)], 9000),
                        new(1072034, [new(4000021, 35), new(4021003, 1), new(4003000, 10)], 9000),
                        new(1072069, [new(4000021, 35), new(4021000, 1), new(4003000, 10)], 9000),
                        new(1072079, [new(4000021, 50), new(4021000, 2), new(4003000, 15)], 19000),
                        new(1072080, [new(4000021, 50), new(4021005, 2), new(4003000, 15)], 19000),
                        new(1072081, [new(4000021, 50), new(4021003, 2), new(4003000, 15)], 19000),
                        new(1072082, [new(4000021, 50), new(4021004, 2), new(4003000, 15)], 19000),
                        new(1072083, [new(4000021, 50), new(4021006, 2), new(4003000, 15)], 19000),
                        new(1072101, [new(4021002, 3), new(4021006, 1), new(4000030, 15), new(4000021, 30), new(4003000, 20)], 19000),
                        new(1072102, [new(4021003, 3), new(4021006, 1), new(4000030, 15), new(4000021, 30), new(4003000, 20)], 20000),
                        new(1072103, [new(4021000, 3), new(4021006, 1), new(4000030, 15), new(4000021, 30), new(4003000, 20)], 20000),
                        new(1072118, [new(4021000, 4), new(4003000, 30), new(4000030, 45), new(4000024, 20)], 20000),
                        new(1072119, [new(4021006, 4), new(4003000, 30), new(4000030, 45), new(4000027, 20)], 32000),
                        new(1072120, [new(4011003, 5), new(4003000, 30), new(4000030, 45), new(4000044, 40)], 32000),
                        new(1072121, [new(4021002, 5), new(4003000, 30), new(4000030, 45), new(4000009, 120)], 40000),
                        new(1072122, [new(4011001, 3), new(4021006, 3), new(4021008, 1), new(4000030, 60), new(4003000, 35), new(4000033, 80)], 40000),
                        new(1072123, [new(4011001, 3), new(4021006, 3), new(4021008, 1), new(4000030, 60), new(4003000, 35), new(4000032, 150)], 50000),
                        new(1072124, [new(4011001, 3), new(4021006, 3), new(4021008, 1), new(4000030, 60), new(4003000, 35), new(4000041, 100)], 50000),
                        new(1072125, [new(4011001, 3), new(4021006, 3), new(4021008, 1), new(4000030, 60), new(4003000, 35), new(4000042, 250)], 50000),
                        new(1072144, [new(4011006, 5), new(4021000, 8), new(4021007, 1), new(4000030, 75), new(4003000, 50)], 50000),
                        new(1072145, [new(4011006, 5), new(4021005, 8), new(4021007, 1), new(4000030, 75), new(4003000, 50)], 60000),
                        new(1072146, [new(4011006, 5), new(4021003, 8), new(4021007, 1), new(4000030, 75), new(4003000, 50)], 60000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1061000_CreateMagician),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1061000_MagicianShoes),
                    AskQuantity = false,
                    Formulas = [
                        new(1072019, [new(4021005, 1), new(4000021, 30), new(4003000, 5)], 3000),
                        new(1072020, [new(4021001, 1), new(4000021, 30), new(4003000, 5)], 3000),
                        new(1072021, [new(4021000, 1), new(4000021, 30), new(4003000, 5)], 3000),
                        new(1072072, [new(4011004, 1), new(4000021, 35), new(4003000, 10)], 8000),
                        new(1072073, [new(4021006, 1), new(4000021, 35), new(4003000, 10)], 8000),
                        new(1072074, [new(4021004, 1), new(4000021, 35), new(4003000, 10)], 8000),
                        new(1072075, [new(4021000, 2), new(4000021, 50), new(4003000, 15)], 18000),
                        new(1072076, [new(4021002, 2), new(4000021, 50), new(4003000, 15)], 18000),
                        new(1072077, [new(4011004, 2), new(4000021, 50), new(4003000, 15)], 18000),
                        new(1072078, [new(4021008, 1), new(4000021, 50), new(4003000, 15)], 18000),
                        new(1072089, [new(4021001, 3), new(4021006, 1), new(4000021, 30), new(4000030, 15), new(4003000, 20)], 20000),
                        new(1072090, [new(4021000, 3), new(4021006, 1), new(4000021, 30), new(4000030, 15), new(4003000, 20)], 20000),
                        new(1072091, [new(4021008, 2), new(4021006, 1), new(4000021, 40), new(4000030, 25), new(4003000, 20)], 22000),
                        new(1072114, [new(4021000, 4), new(4000030, 40), new(4000043, 35), new(4003000, 25)], 30000),
                        new(1072115, [new(4021005, 4), new(4000030, 40), new(4000037, 70), new(4003000, 25)], 30000),
                        new(1072116, [new(4011006, 2), new(4021007, 1), new(4000030, 40), new(4000027, 20), new(4003000, 25)], 35000),
                        new(1072117, [new(4021008, 2), new(4021007, 1), new(4000030, 40), new(4000014, 30), new(4003000, 30)], 40000),
                        new(1072140, [new(4021009, 1), new(4011006, 3), new(4021000, 3), new(4000030, 60), new(4003000, 40)], 50000),
                        new(1072141, [new(4021009, 1), new(4011006, 3), new(4021005, 3), new(4000030, 60), new(4003000, 40)], 50000),
                        new(1072142, [new(4021009, 1), new(4011006, 3), new(4021001, 3), new(4000030, 60), new(4003000, 40)], 50000),
                        new(1072143, [new(4021009, 1), new(4011006, 3), new(4021003, 3), new(4000030, 60), new(4003000, 40)], 50000),
                        new(1072136, [new(4021009, 1), new(4011006, 4), new(4011005, 5), new(4000030, 70), new(4003000, 50)], 60000),
                        new(1072137, [new(4021009, 1), new(4011006, 4), new(4021003, 5), new(4000030, 70), new(4003000, 50)], 60000),
                        new(1072138, [new(4021009, 1), new(4011006, 4), new(4011003, 5), new(4000030, 70), new(4003000, 50)], 60000),
                        new(1072139, [new(4021009, 1), new(4011006, 4), new(4021002, 5), new(4000030, 70), new(4003000, 50)], 60000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1061000_CreateThief),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1061000_ThiefShoes),
                    AskQuantity = false,
                    Formulas = [
                        new(1072084, [new(4021005, 1), new(4000021, 35), new(4003000, 10)], 9000),
                        new(1072085, [new(4021000, 1), new(4000021, 35), new(4003000, 10)], 9000),
                        new(1072086, [new(4021003, 1), new(4000021, 35), new(4003000, 10)], 9000),
                        new(1072087, [new(4021004, 1), new(4000021, 35), new(4003000, 10)], 9000),
                        new(1072032, [new(4011000, 3), new(4000021, 50), new(4003000, 15)], 19000),
                        new(1072033, [new(4011001, 3), new(4000021, 50), new(4003000, 15)], 19000),
                        new(1072035, [new(4011004, 2), new(4000021, 50), new(4003000, 15)], 19000),
                        new(1072036, [new(4011006, 2), new(4000021, 50), new(4003000, 15)], 21000),
                        new(1072104, [new(4021000, 3), new(4021004, 1), new(4000021, 30), new(4000030, 15), new(4003000, 20)], 20000),
                        new(1072105, [new(4021003, 3), new(4021004, 1), new(4000021, 30), new(4000030, 15), new(4003000, 20)], 20000),
                        new(1072106, [new(4021002, 3), new(4021004, 1), new(4000021, 30), new(4000030, 15), new(4003000, 20)], 20000),
                        new(1072107, [new(4021000, 5), new(4000030, 45), new(4000033, 50), new(4003000, 30)], 40000),
                        new(1072108, [new(4021003, 4), new(4000030, 45), new(4000032, 30), new(4003000, 30)], 32000),
                        new(1072109, [new(4021006, 4), new(4000030, 45), new(4000040, 3), new(4003000, 30)], 35000),
                        new(1072110, [new(4021005, 4), new(4000030, 45), new(4000037, 70), new(4003000, 30)], 35000),
                        new(1072128, [new(4011007, 2), new(4021005, 3), new(4000030, 50), new(4000037, 200), new(4003000, 35)], 50000),
                        new(1072130, [new(4011007, 2), new(4021000, 3), new(4000030, 50), new(4000043, 150), new(4003000, 35)], 50000),
                        new(1072129, [new(4011007, 2), new(4021003, 3), new(4000030, 50), new(4000045, 80), new(4003000, 35)], 50000),
                        new(1072131, [new(4011007, 2), new(4021001, 3), new(4000030, 50), new(4000036, 80), new(4003000, 35)], 50000),
                        new(1072150, [new(4021008, 1), new(4011007, 1), new(4021005, 8), new(4000030, 75), new(4003000, 50)], 60000),
                        new(1072151, [new(4021008, 1), new(4011007, 1), new(4011005, 5), new(4000030, 75), new(4003000, 50)], 60000),
                        new(1072152, [new(4021008, 1), new(4011007, 1), new(4021000, 1), new(4000030, 75), new(4003000, 50)], 60000)
                    ]
                }
            };

            await PerformRefine(this.GetTalkMessage(ScriptMessage.Maker_Npc1061000_LiveDont), categories);
        }

        // Npc: 2010000 
        public async Task carlie()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.Maker_TradeIntro));
            if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.Maker_TradeExplain)))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Maker_ComeBackLater));
                return;
            }

            int[] eQuestChoices = [4000073, 4000059, 4000060, 4000061, 4000058,
                4000062, 4000048, 4000049, 4000055, 4000056,
                4000051, 4000052, 4000050, 4000057, 4000053,
                4000054, 4000076, 4000078, 4000081, 4000070,
                4000071, 4000072, 4000069, 4000079, 4000080];

            int[][][] eQuestPrizes = new int[25][][];
            eQuestPrizes[0] = [[2000001, 20], [2010004, 10], [2000003, 15], [4003001, 15], [2020001, 15], [2030000, 15]];
            eQuestPrizes[1] = [[2000003, 20], [2000001, 30], [2010001, 40], [4003001, 20], [2040002, 1]];
            eQuestPrizes[2] = [[2000002, 25], [2000006, 10], [2022000, 5], [4000030, 15], [2040902, 1]];
            eQuestPrizes[3] = [[2000002, 30], [2000006, 15], [2020000, 20], [4003000, 5], [2041016, 1]];
            eQuestPrizes[4] = [[2000002, 15], [2010004, 15], [2000003, 25], [4003001, 30], [2040302, 1]];
            eQuestPrizes[5] = [[2000002, 30], [2000006, 15], [2020000, 20], [4003000, 5], [2040402, 1]];
            eQuestPrizes[6] = [[2000002, 30], [2020000, 20], [2000006, 15], [4003000, 5], [2040402, 1]];
            eQuestPrizes[7] = [[2000006, 25], [2020000, 20], [4020000, 7], [4020001, 7], [4020002, 3], [4020007, 2], [2040708, 1]];
            eQuestPrizes[8] = [[2020005, 30], [2020006, 15], [2022001, 30], [4003003, 1], [2040505, 1]];
            eQuestPrizes[9] = [[2000006, 25], [4020005, 7], [4020003, 7], [4020004, 7], [4020008, 2], [2040802, 1]];
            eQuestPrizes[10] = [[2002004, 15], [2002005, 15], [2002003, 15], [4001005, 1], [2040502, 1]];
            eQuestPrizes[11] = [[2000006, 20], [4010004, 7], [4010003, 7], [4010005, 7], [4003002, 1], [2040602, 1]];
            eQuestPrizes[12] = [[2000006, 20], [4010002, 7], [4010001, 7], [4010000, 7], [4010006, 2], [4003000, 5], [2040702, 1]];
            eQuestPrizes[13] = [[2000006, 20], [4010004, 7], [4010005, 7], [4010006, 3], [4020007, 2], [4020008, 2], [2040705, 1]];
            eQuestPrizes[14] = [[2000006, 30], [4020006, 7], [4020008, 2], [4020007, 2], [2070010, 1], [2040805, 1]];
            eQuestPrizes[15] = [[2000006, 30], [4020006, 7], [4020008, 2], [4020007, 2], [2041020, 1]];
            eQuestPrizes[16] = [[2000001, 30], [2000003, 20], [4003001, 20], [2010001, 40], [2040002, 1]];
            eQuestPrizes[17] = [[2000002, 15], [2000003, 25], [2010004, 15], [2050004, 15], [4003001, 30], [2040302, 1]];
            eQuestPrizes[18] = [[2000006, 25], [2020006, 25], [4010004, 8], [4010005, 8], [4010006, 3], [4020007, 2], [4020008, 2], [2040705, 1]];
            eQuestPrizes[19] = [[2000002, 30], [2020000, 20], [2000006, 15], [4003000, 5], [2041005, 1]];
            eQuestPrizes[20] = [[2000002, 30], [2020000, 20], [2000006, 15], [4003000, 5], [2041005, 1]];
            eQuestPrizes[21] = [[2000002, 30], [2020000, 20], [2000006, 15], [4003000, 5], [2041005, 1]];
            eQuestPrizes[22] = [[2000006, 20], [2020005, 30], [2020006, 15], [2050004, 20], [4003003, 1], [2041002, 1]];
            eQuestPrizes[23] = [[2000006, 25], [2050004, 50], [2022001, 35], [4020000, 8], [4020001, 8], [4020002, 8], [4020007, 2], [2041023, 1]];
            eQuestPrizes[24] = [[2000006, 35], [4020006, 9], [4010008, 4], [4020007, 4], [2041008, 1]];

            var sel = await AskMenu(this.GetTalkMessage(ScriptMessage.Maker_ChooseItem),
                eQuestChoices.Select(id => $"#v{id}#  #t{id}#"));

            if (sel < 0 || sel >= eQuestChoices.Length) return;

            var requiredItem = eQuestChoices[sel];
            if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.Maker_ConfirmTrade, requiredItem)))
                return;

            if (!haveItem(requiredItem, 100))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Maker_CheckItems, requiredItem));
                return;
            }

            var reward = eQuestPrizes[sel];
            var prize = reward[Random.Shared.Next(reward.Length)];
            var prizeItem = prize[0];
            var prizeQuantity = prize[1];

            if (!canHold(prizeItem, prizeQuantity))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Maker_InventoryFull));
                return;
            }

            await gainItem(requiredItem, -100);
            await gainExp(500);
            await gainItem(prizeItem, prizeQuantity);
            await SayOK(this.GetTalkMessage(ScriptMessage.Maker_HereIsPrize, requiredItem, prizeQuantity, prizeItem));
        }


        // Npc: 2010003 
        public async Task make_orbis()
        {
            var categories = new List<RecipeCategory>
            {
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc2010003_CreateUpgrade),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc2010003_WarriorGlove),
                    AskQuantity = false,
                    Formulas = [
                        new(1082103, [new(4005000, 2), new(4011000, 8), new(4011006, 3), new(4000030, 70), new(4003000, 55)], 90000),
                        new(1082104, [new(1082103, 1), new(4011002, 6), new(4021006, 4)], 90000),
                        new(1082105, [new(1082103, 1), new(4021006, 8), new(4021008, 3)], 100000),
                        new(1082114, [new(4005000, 2), new(4005002, 1), new(4021005, 8), new(4000030, 90), new(4003000, 60)], 100000),
                        new(1082115, [new(1082114, 1), new(4005000, 1), new(4005002, 1), new(4021003, 7)], 110000),
                        new(1082116, [new(1082114, 1), new(4005002, 3), new(4021000, 8)], 110000),
                        new(1082117, [new(1082114, 1), new(4005000, 2), new(4005002, 1), new(4021008, 4)], 120000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc2010003_CreateUpgradeBowman),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc2010003_BowmanGlove),
                    AskQuantity = false,
                    Formulas = [
                        new(1082106, [new(4005002, 2), new(4021005, 8), new(4011004, 3), new(4000030, 70), new(4003000, 55)], 90000),
                        new(1082107, [new(1082106, 1), new(4021006, 5), new(4011006, 3)], 90000),
                        new(1082108, [new(1082106, 1), new(4021007, 2), new(4021008, 3)], 100000),
                        new(1082109, [new(4005002, 2), new(4005000, 1), new(4021000, 8), new(4000030, 90), new(4003000, 60)], 100000),
                        new(1082110, [new(1082109, 1), new(4005002, 1), new(4005000, 1), new(4021005, 7)], 110000),
                        new(1082111, [new(1082109, 1), new(4005002, 1), new(4005000, 1), new(4021003, 7)], 110000),
                        new(1082112, [new(1082109, 1), new(4005002, 2), new(4005000, 1), new(4021008, 4)], 120000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc2010003_CreateUpgradeMagician),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc2010003_MagicianGlove),
                    AskQuantity = false,
                    Formulas = [
                        new(1082098, [new(4005001, 2), new(4011000, 6), new(4011004, 6), new(4000030, 70), new(4003000, 55)], 90000),
                        new(1082099, [new(1082098, 1), new(4021002, 6), new(4021007, 2)], 90000),
                        new(1082100, [new(1082098, 1), new(4021008, 3), new(4011006, 3)], 100000),
                        new(1082121, [new(4005001, 2), new(4005003, 1), new(4021003, 8), new(4000030, 90), new(4003000, 60)], 100000),
                        new(1082122, [new(1082121, 1), new(4005001, 1), new(4005003, 1), new(4021005, 7)], 110000),
                        new(1082123, [new(1082121, 1), new(4005001, 1), new(4005003, 1), new(4021008, 4)], 120000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc2010003_CreateUpgradeThief),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc2010003_ThiefGlove),
                    AskQuantity = false,
                    Formulas = [
                        new(1082095, [new(4005003, 2), new(4011000, 6), new(4011003, 6), new(4000030, 70), new(4003000, 55)], 90000),
                        new(1082096, [new(1082095, 1), new(4011004, 6), new(4021007, 2)], 90000),
                        new(1082097, [new(1082095, 1), new(4021007, 3), new(4011006, 3)], 100000),
                        new(1082118, [new(4005003, 2), new(4005002, 1), new(4011002, 8), new(4000030, 90), new(4003000, 60)], 100000),
                        new(1082119, [new(1082118, 1), new(4005003, 1), new(4005002, 1), new(4021001, 7)], 110000),
                        new(1082120, [new(1082118, 1), new(4005003, 1), new(4005002, 1), new(4021000, 8)], 120000)
                    ]
                }
            };

            await PerformRefine(this.GetTalkMessage(ScriptMessage.Maker_Npc2010003_ImOrbis), categories);
        }

        // Npc: 2020000 
        public async Task refine_elnath()
        {
            var categories = new List<RecipeCategory>
            {
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1022003_RefineMineral),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1022003_KindMineral),
                    Formulas = [
                        new(4011000, [new(4010000, 10)], 300),
                        new(4011001, [new(4010001, 10)], 300),
                        new(4011002, [new(4010002, 10)], 300),
                        new(4011003, [new(4010003, 10)], 500),
                        new(4011004, [new(4010004, 10)], 500),
                        new(4011005, [new(4010005, 10)], 500),
                        new(4011006, [new(4010006, 10)], 800)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1022003_RefineJewel),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1022003_KindJewel),
                    Formulas = [
                        new(4021000, [new(4020000, 10)], 500),
                        new(4021001, [new(4020001, 10)], 500),
                        new(4021002, [new(4020002, 10)], 500),
                        new(4021003, [new(4020003, 10)], 500),
                        new(4021004, [new(4020004, 10)], 500),
                        new(4021005, [new(4020005, 10)], 500),
                        new(4021006, [new(4020006, 10)], 500),
                        new(4021007, [new(4020007, 10)], 1000),
                        new(4021008, [new(4020008, 10)], 3000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc2020000_RefineRare),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc2020000_RareJewel),
                    Formulas = [
                        new(4011007, [new(4011000, 1), new(4011001, 1), new(4011002, 1), new(4011003, 1), new(4011004, 1), new(4011005, 1), new(4011006, 1)], 10000),
                        new(4021009, [new(4021000, 1), new(4021001, 1), new(4021002, 1), new(4021003, 1), new(4021004, 1), new(4021005, 1), new(4021006, 1), new(4021007, 1), new(4021008, 1)], 15000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc2020000_RefineCrystal),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc2020000_CrystalOre),
                    Formulas = [
                        new(4005000, [new(4004000, 10)], 5000),
                        new(4005001, [new(4004001, 10)], 5000),
                        new(4005002, [new(4004002, 10)], 5000),
                        new(4005003, [new(4004003, 10)], 5000),
                        new(4005004, [new(4004004, 10)], 1000000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_CreateMaterials),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1022004_MaterialsKnow),
                    Formulas = [
                        new(4003001, [new(4000003, 10)], 0),
                        new(4003001, [new(4000018, 5)], 0),
                        new(4003000, [new(4011000, 1), new(4011001, 1)], 0)
                    ],
                    GetResultQuantity = (itemId, count) => itemId == 4003000 ? count * 15 : count
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_CreateArrows),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_ArrowsProblem),
                    Formulas = [
                        new(2060000, [new(4003001, 1), new(4003004, 1)], 0),
                        new(2061000, [new(4003001, 1), new(4003004, 1)], 0),
                        new(2060001, [new(4011000, 1), new(4003001, 3), new(4003004, 10)], 0),
                        new(2061001, [new(4011000, 1), new(4003001, 3), new(4003004, 10)], 0),
                        new(2060002, [new(4011001, 1), new(4003001, 5), new(4003005, 15)], 0),
                        new(2061002, [new(4011001, 1), new(4003001, 5), new(4003005, 15)], 0)
                    ],
                    GetResultQuantity = (itemId, count) => {
                        if (itemId >= 2060000 && itemId <= 2060002) return (1000 - (itemId - 2060000) * 100) * count;
                        if (itemId >= 2061000 && itemId <= 2061002) return (1000 - (itemId - 2061000) * 100) * count;
                        return count;
                    }
                }
            };

            await PerformRefine(this.GetTalkMessage(ScriptMessage.Maker_Npc1022003_HmYouve), categories);
        }


        // Npc: 2020002 
        public async Task make_elnath()
        {
            var categories = new List<RecipeCategory>
            {
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1061000_CreateWarrior),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc2020002_WarriorShoes),
                    AskQuantity = false,
                    Formulas = [
                        new(1072147, [new(4021008, 1), new(4011007, 1), new(4021005, 8), new(4000030, 80), new(4003000, 55)], 60000),
                        new(1072148, [new(4021008, 1), new(4011007, 1), new(4011005, 8), new(4000030, 80), new(4003000, 55)], 60000),
                        new(1072149, [new(4021008, 1), new(4011007, 1), new(4021000, 8), new(4000030, 80), new(4003000, 55)], 60000),
                        new(1072154, [new(4005000, 1), new(4005002, 3), new(4011002, 5), new(4000048, 100), new(4003000, 55)], 70000),
                        new(1072155, [new(4005000, 2), new(4005002, 2), new(4011005, 5), new(4000048, 100), new(4003000, 55)], 70000),
                        new(1072156, [new(4005000, 3), new(4005002, 1), new(4021008, 1), new(4000048, 100), new(4003000, 55)], 70000),
                        new(1072210, [new(4005000, 2), new(4005002, 3), new(4021000, 7), new(4000030, 90), new(4003000, 65)], 80000),
                        new(1072211, [new(4005000, 3), new(4005002, 2), new(4021002, 7), new(4000030, 90), new(4003000, 65)], 80000),
                        new(1072212, [new(4005000, 4), new(4005002, 1), new(4021008, 2), new(4000030, 90), new(4003000, 65)], 80000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1061000_CreateBowman),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc2020002_BowmanShoes),
                    AskQuantity = false,
                    Formulas = [
                        new(1072144, [new(4011006, 5), new(4021000, 8), new(4021007, 1), new(4000030, 75), new(4003000, 50)], 60000),
                        new(1072145, [new(4011006, 5), new(4021005, 8), new(4021007, 1), new(4000030, 75), new(4003000, 50)], 60000),
                        new(1072146, [new(4011006, 5), new(4021003, 8), new(4021007, 1), new(4000030, 75), new(4003000, 50)], 60000),
                        new(1072164, [new(4005002, 1), new(4005000, 3), new(4021005, 5), new(4000055, 100), new(4003000, 55)], 70000),
                        new(1072165, [new(4005002, 2), new(4005000, 2), new(4021004, 5), new(4000055, 100), new(4003000, 55)], 70000),
                        new(1072166, [new(4005002, 2), new(4005000, 2), new(4021003, 5), new(4000055, 100), new(4003000, 55)], 70000),
                        new(1072167, [new(4005002, 3), new(4005000, 1), new(4021008, 1), new(4000055, 100), new(4003000, 55)], 70000),
                        new(1072182, [new(4005002, 2), new(4005000, 3), new(4021002, 7), new(4000030, 90), new(4003000, 60)], 80000),
                        new(1072183, [new(4005002, 3), new(4005000, 2), new(4021000, 7), new(4000030, 90), new(4003000, 60)], 80000),
                        new(1072184, [new(4005002, 4), new(4005000, 1), new(4021003, 7), new(4000030, 90), new(4003000, 60)], 80000),
                        new(1072185, [new(4005002, 5), new(4021008, 2), new(4000030, 90), new(4003000, 60)], 80000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1061000_CreateMagician),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc2020002_MagicianShoes),
                    AskQuantity = false,
                    Formulas = [
                        new(1072136, [new(4021009, 1), new(4011006, 4), new(4011005, 5), new(4000030, 70), new(4003000, 50)], 60000),
                        new(1072137, [new(4021009, 1), new(4011006, 4), new(4021003, 5), new(4000030, 70), new(4003000, 50)], 60000),
                        new(1072138, [new(4021009, 1), new(4011006, 4), new(4011003, 5), new(4000030, 70), new(4003000, 50)], 60000),
                        new(1072139, [new(4021009, 1), new(4011006, 4), new(4021002, 5), new(4000030, 70), new(4003000, 50)], 60000),
                        new(1072157, [new(4005001, 1), new(4005003, 3), new(4021002, 5), new(4000051, 100), new(4003000, 55)], 70000),
                        new(1072158, [new(4005001, 2), new(4005003, 2), new(4021000, 5), new(4000051, 100), new(4003000, 55)], 70000),
                        new(1072159, [new(4005001, 2), new(4005003, 2), new(4011003, 5), new(4000051, 100), new(4003000, 55)], 70000),
                        new(1072160, [new(4005001, 3), new(4005003, 1), new(4011006, 3), new(4000051, 100), new(4003000, 55)], 70000),
                        new(1072177, [new(4005001, 2), new(4005003, 3), new(4021003, 7), new(4000030, 85), new(4003000, 60)], 80000),
                        new(1072178, [new(4005001, 3), new(4005003, 2), new(4021001, 7), new(4000030, 85), new(4003000, 60)], 80000),
                        new(1072179, [new(4005001, 4), new(4005003, 1), new(4021008, 2), new(4000030, 85), new(4003000, 60)], 80000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1061000_CreateThief),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc2020002_ThiefShoes),
                    AskQuantity = false,
                    Formulas = [
                        new(1072150, [new(4021007, 1), new(4011007, 1), new(4021000, 8), new(4000030, 75), new(4003000, 50)], 60000),
                        new(1072151, [new(4021007, 1), new(4011007, 1), new(4011006, 5), new(4000030, 75), new(4003000, 50)], 60000),
                        new(1072152, [new(4021007, 1), new(4011007, 1), new(4021008, 1), new(4000030, 75), new(4003000, 50)], 60000),
                        new(1072161, [new(4005003, 1), new(4005000, 3), new(4021001, 5), new(4000051, 100), new(4003000, 55)], 70000),
                        new(1072162, [new(4005003, 1), new(4005002, 3), new(4021005, 5), new(4000051, 100), new(4003000, 55)], 70000),
                        new(1072163, [new(4005002, 2), new(4005003, 2), new(4021000, 5), new(4000051, 100), new(4003000, 55)], 70000),
                        new(1072172, [new(4005000, 3), new(4005003, 2), new(4021003, 7), new(4000030, 90), new(4003000, 60)], 80000),
                        new(1072173, [new(4005002, 3), new(4005003, 2), new(4021000, 7), new(4000030, 90), new(4003000, 60)], 80000),
                        new(1072174, [new(4005003, 3), new(4005002, 2), new(4021008, 2), new(4000030, 90), new(4003000, 60)], 80000)
                    ]
                }
            };

            await PerformRefine(this.GetTalkMessage(ScriptMessage.Maker_Npc2020002_ElNath), categories);
        }

        // Npc: 2040016 
        public async Task make_ludi1()
        {
            var categories = new List<RecipeCategory>
            {
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1022003_RefineMineral),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1022003_KindMineral),
                    Formulas = [
                        new(4011000, [new(4010000, 10)], 270),
                        new(4011001, [new(4010001, 10)], 270),
                        new(4011002, [new(4010002, 10)], 270),
                        new(4011003, [new(4010003, 10)], 450),
                        new(4011004, [new(4010004, 10)], 450),
                        new(4011005, [new(4010005, 10)], 450),
                        new(4011006, [new(4010006, 10)], 720)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1022003_RefineJewel),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1022003_KindJewel),
                    Formulas = [
                        new(4021000, [new(4020000, 10)], 450),
                        new(4021001, [new(4020001, 10)], 450),
                        new(4021002, [new(4020002, 10)], 450),
                        new(4021003, [new(4020003, 10)], 450),
                        new(4021004, [new(4020004, 10)], 450),
                        new(4021005, [new(4020005, 10)], 450),
                        new(4021006, [new(4020006, 10)], 450),
                        new(4021007, [new(4020007, 10)], 900),
                        new(4021008, [new(4020008, 10)], 2700)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc2020000_RefineRare),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc2020000_RareJewel),
                    Formulas = [
                        new(4011007, [new(4011000, 1), new(4011001, 1), new(4011002, 1), new(4011003, 1), new(4011004, 1), new(4011005, 1), new(4011006, 1)], 9000),
                        new(4021009, [new(4021000, 1), new(4021001, 1), new(4021002, 1), new(4021003, 1), new(4021004, 1), new(4021005, 1), new(4021006, 1), new(4021007, 1), new(4021008, 1)], 13500)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc2020000_RefineCrystal),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc2040016_CrystalOre),
                    Formulas = [
                        new(4005000, [new(4004000, 10)], 4500),
                        new(4005001, [new(4004001, 10)], 4500),
                        new(4005002, [new(4004002, 10)], 4500),
                        new(4005003, [new(4004003, 10)], 4500)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_CreateMaterials),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1022004_MaterialsKnow),
                    Formulas = [
                        new(4003001, [new(4000003, 10)], 0),
                        new(4003001, [new(4000018, 5)], 0),
                        new(4003000, [new(4011000, 1), new(4011001, 1)], 0)
                    ],
                    GetResultQuantity = (itemId, count) => itemId == 4003000 ? count * 15 : count
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_CreateArrows),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc1012002_ArrowsProblem),
                    Formulas = [
                        new(2060000, [new(4003001, 1), new(4003004, 1)], 0),
                        new(2061000, [new(4003001, 1), new(4003004, 1)], 0),
                        new(2060001, [new(4011000, 1), new(4003001, 3), new(4003004, 10)], 0),
                        new(2061001, [new(4011000, 1), new(4003001, 3), new(4003004, 10)], 0),
                        new(2060002, [new(4011001, 1), new(4003001, 5), new(4003005, 15)], 0),
                        new(2061002, [new(4011001, 1), new(4003001, 5), new(4003005, 15)], 0)
                    ],
                    GetResultQuantity = (itemId, count) => {
                        if (itemId >= 2060000 && itemId <= 2060002) return (1000 - (itemId - 2060000) * 100) * count;
                        if (itemId >= 2061000 && itemId <= 2061002) return (1000 - (itemId - 2061000) * 100) * count;
                        return count;
                    }
                }
            };

            await PerformRefine(this.GetTalkMessage(ScriptMessage.Maker_Npc1022003_HmYouve), categories);
        }

        // Npc: 2040020 
        public async Task make_ludi2()
        {
            string greeting = this.GetTalkMessage(ScriptMessage.Maker_Npc2040020_WelcomeLudibrium);
            string[] mainOptions = [
                this.GetTalkMessage(ScriptMessage.Maker_Npc2040020_WhatsStimulator), 
                this.GetTalkMessage(ScriptMessage.Maker_Npc2040020_CreateWarrior), 
                this.GetTalkMessage(ScriptMessage.Maker_Npc2040020_CreateBowman), 
                this.GetTalkMessage(ScriptMessage.Maker_Npc2040020_CreateMagician), 
                this.GetTalkMessage(ScriptMessage.Maker_Npc2040020_CreateThiefGlove),
                this.GetTalkMessage(ScriptMessage.Maker_Npc2040020_CreateWarriorGlove), 
                this.GetTalkMessage(ScriptMessage.Maker_Npc2040020_CreateBowmanGlove), 
                this.GetTalkMessage(ScriptMessage.Maker_Npc2040020_CreateMagicianGlove),
                this.GetTalkMessage(ScriptMessage.Maker_Npc2040020_CreateThief)];

            var mainSel = await AskMenu(greeting, mainOptions);
            if (mainSel < 0) return;

            if (mainSel == 0)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Maker_Npc2040020_StimulatorSpecial));
                return;
            }

            bool stimulator = mainSel > 4;
            int categoryIdx = stimulator ? mainSel - 5 : mainSel - 1;
            int stimId = 4130000;

            var warriorGloveCat = new RecipeCategory
            {
                Name = this.GetTalkMessage(ScriptMessage.Maker_Npc2040020_WarriorGlove),
                SelectionPrompt = stimulator ? this.GetTalkMessage(ScriptMessage.Maker_Npc2040020_WarriorGloveStimulator) : this.GetTalkMessage(ScriptMessage.Maker_Npc2040020_WarriorGloveSure),
                AskQuantity = false,
                Formulas = stimulator ? [
                    new(1082005, [new(1082007, 1), new(4011001, 1)], 18000),
                    new(1082006, [new(1082007, 1), new(4011005, 2)], 22500),
                    new(1082035, [new(1082008, 1), new(4021006, 3)], 27000),
                    new(1082036, [new(1082008, 1), new(4021008, 1)], 36000),
                    new(1082024, [new(1082023, 1), new(4011003, 4)], 40500),
                    new(1082025, [new(1082023, 1), new(4021008, 2)], 45000),
                    new(1082010, [new(1082009, 1), new(4011002, 5)], 49500),
                    new(1082011, [new(1082009, 1), new(4011006, 4)], 54000)
                ] : [
                    new(1082007, [new(4011000, 3), new(4011001, 2), new(4003000, 15)], 18000),
                    new(1082008, [new(4000021, 30), new(4011001, 4), new(4003000, 15)], 27000),
                    new(1082023, [new(4000021, 50), new(4011001, 5), new(4003000, 40)], 36000),
                    new(1082009, [new(4011001, 3), new(4021007, 2), new(4000030, 30), new(4003000, 45)], 45000)
                ]
            };

            var bowmanGloveCat = new RecipeCategory
            {
                Name = this.GetTalkMessage(ScriptMessage.Maker_Npc2040020_BowmanGlove),
                SelectionPrompt = stimulator ? this.GetTalkMessage(ScriptMessage.Maker_Npc2040020_BowmanGloveStimulator) : this.GetTalkMessage(ScriptMessage.Maker_Npc2040020_BowmanGloveSure),
                AskQuantity = false,
                Formulas = stimulator ? [
                    new(1082049, [new(1082048, 1), new(4021003, 3)], 13500),
                    new(1082050, [new(1082048, 1), new(4021008, 1)], 18000),
                    new(1082069, [new(1082068, 1), new(4011002, 4)], 19800),
                    new(1082070, [new(1082068, 1), new(4011006, 2)], 22500),
                    new(1082072, [new(1082071, 1), new(4011006, 4)], 27000),
                    new(1082073, [new(1082071, 1), new(4021008, 2)], 36000),
                    new(1082085, [new(1082084, 1), new(4011000, 1), new(4021000, 5)], 49500),
                    new(1082083, [new(1082084, 1), new(4011006, 2), new(4021008, 2)], 54000)
                ] : [
                    new(1082048, [new(4000021, 50), new(4011006, 2), new(4021001, 1)], 18000),
                    new(1082068, [new(4011000, 1), new(4011001, 3), new(4000021, 60), new(4003000, 15)], 27000),
                    new(1082071, [new(4011001, 3), new(4021000, 1), new(4021002, 3), new(4000021, 80), new(4003000, 25)], 36000),
                    new(1082084, [new(4011004, 3), new(4011006, 1), new(4021002, 2), new(4000030, 40), new(4003000, 35)], 45000)
                ]
            };

            var mageGloveCat = new RecipeCategory
            {
                Name = this.GetTalkMessage(ScriptMessage.Maker_Npc2040020_MagicianGlove),
                SelectionPrompt = stimulator ? this.GetTalkMessage(ScriptMessage.Maker_Npc2040020_MagicianGloveStimulator) : this.GetTalkMessage(ScriptMessage.Maker_Npc2040020_MagicianGloveSure),
                AskQuantity = false,
                Formulas = stimulator ? [
                    new(1082052, [new(1082051, 1), new(4021005, 3)], 31500),
                    new(1082053, [new(1082051, 1), new(4021008, 1)], 36000),
                    new(1082055, [new(1082054, 1), new(4021005, 3)], 36000),
                    new(1082056, [new(1082054, 1), new(4021008, 1)], 40500),
                    new(1082063, [new(1082062, 1), new(4021002, 4)], 40500),
                    new(1082064, [new(1082062, 1), new(4021008, 2)], 45000),
                    new(1082082, [new(1082081, 1), new(4021002, 5)], 49500),
                    new(1082080, [new(1082081, 1), new(4021008, 3)], 54000)
                ] : [
                    new(1082051, [new(4000021, 60), new(4021006, 1), new(4021000, 2)], 22500),
                    new(1082054, [new(4000021, 70), new(4011006, 1), new(4011001, 3), new(4021000, 2)], 27000),
                    new(1082062, [new(4000021, 80), new(4021000, 3), new(4021006, 3), new(4003000, 30)], 36000),
                    new(1082081, [new(4021000, 3), new(4011006, 2), new(4000030, 35), new(4003000, 40)], 45000)
                ]
            };

            var thiefGloveCat = new RecipeCategory
            {
                Name = this.GetTalkMessage(ScriptMessage.Maker_Npc2040020_ThiefGlove),
                SelectionPrompt = stimulator ? this.GetTalkMessage(ScriptMessage.Maker_Npc2040020_ThiefGloveStimulator) : this.GetTalkMessage(ScriptMessage.Maker_Npc2040020_ThiefGloveSure),
                AskQuantity = false,
                Formulas = stimulator ? [
                    new(1082043, [new(1082042, 1), new(4011004, 2)], 13500),
                    new(1082044, [new(1082042, 1), new(4011006, 1)], 18000),
                    new(1082047, [new(1082046, 1), new(4011005, 3)], 19800),
                    new(1082045, [new(1082046, 1), new(4011006, 2)], 22500),
                    new(1082076, [new(1082075, 1), new(4011006, 4)], 36000),
                    new(1082074, [new(1082075, 1), new(4021008, 2)], 45000),
                    new(1082067, [new(1082065, 1), new(4021000, 5)], 49500),
                    new(1082066, [new(1082065, 1), new(4011006, 2), new(4021008, 1)], 54000)
                ] : [
                    new(1082042, [new(4011001, 2), new(4000021, 50), new(4003000, 10)], 22500),
                    new(1082046, [new(4011001, 3), new(4011000, 1), new(4000021, 60), new(4003000, 15)], 27000),
                    new(1082075, [new(4021000, 3), new(4000101, 100), new(4000021, 80), new(4003000, 30)], 36000),
                    new(1082065, [new(4021005, 3), new(4021008, 1), new(4000030, 40), new(4003000, 30)], 45000)
                ]
            };

            var categoriesList = new List<RecipeCategory> { warriorGloveCat, bowmanGloveCat, mageGloveCat, thiefGloveCat };
            await PerformRefine(greeting, new List<RecipeCategory> { categoriesList[categoryIdx] }, stimulatorId: stimulator ? stimId : 0);
        }


        // Npc: 2040021 
        public async Task make_ludi3()
        {
            string greeting = this.GetTalkMessage(ScriptMessage.Maker_Npc2040021_WelcomeLudibrium);
            string[] mainOptions = [
                this.GetTalkMessage(ScriptMessage.Maker_Npc2040020_WhatsStimulator),
                this.GetTalkMessage(ScriptMessage.Maker_Npc1061000_CreateWarrior),
                this.GetTalkMessage(ScriptMessage.Maker_Npc1061000_CreateBowman),
                this.GetTalkMessage(ScriptMessage.Maker_Npc1061000_CreateMagician), 
                this.GetTalkMessage(ScriptMessage.Maker_Npc1061000_CreateThief),
                this.GetTalkMessage(ScriptMessage.Maker_Npc2040021_CreateWarrior), 
                this.GetTalkMessage(ScriptMessage.Maker_Npc2040021_CreateBowman), 
                this.GetTalkMessage(ScriptMessage.Maker_Npc2040021_CreateMagician), 
                this.GetTalkMessage(ScriptMessage.Maker_Npc2040021_CreateThief)];

            var mainSel = await AskMenu(greeting, mainOptions);
            if (mainSel < 0) return;

            if (mainSel == 0)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Maker_Npc2040020_StimulatorSpecial));
                return;
            }

            bool stimulator = mainSel > 4;
            int categoryIdx = stimulator ? mainSel - 5 : mainSel - 1;
            int stimId = 4130001;

            var warriorShoeCat = new RecipeCategory
            {
                Name = this.GetTalkMessage(ScriptMessage.Maker_Npc2040021_WarriorShoes),
                SelectionPrompt = stimulator ? this.GetTalkMessage(ScriptMessage.Maker_Npc2040021_WarriorShoesStimulator) : this.GetTalkMessage(ScriptMessage.Maker_Npc2040021_WarriorShoesSure),
                AskQuantity = false,
                Formulas = [
                    new(1072003, [new(4021003, 4), new(4011001, 2), new(4000021, 45), new(4003000, 15)], 20000),
                    new(1072039, [new(4011002, 4), new(4011001, 2), new(4000021, 45), new(4003000, 15)], 20000),
                    new(1072040, [new(4011004, 4), new(4011001, 2), new(4000021, 45), new(4003000, 15)], 20000),
                    new(1072041, [new(4021000, 4), new(4011001, 2), new(4000021, 45), new(4003000, 15)], 20000),
                    new(1072002, [new(4011001, 3), new(4021004, 1), new(4000021, 30), new(4000030, 20), new(4003000, 25)], 22000),
                    new(1072112, [new(4011002, 3), new(4021004, 1), new(4000021, 30), new(4000030, 20), new(4003000, 25)], 22000),
                    new(1072113, [new(4021008, 2), new(4021004, 1), new(4000021, 30), new(4000030, 20), new(4003000, 25)], 25000),
                    new(1072000, [new(4011003, 4), new(4000021, 40), new(4000030, 30), new(4003000, 100), new(4000103, 100)], 38000),
                    new(1072126, [new(4011005, 4), new(4021007, 1), new(4000030, 40), new(4003000, 30), new(4000104, 100)], 38000),
                    new(1072127, [new(4011002, 4), new(4021007, 1), new(4000030, 40), new(4003000, 30), new(4000105, 100)], 38000),
                    new(1072132, [new(4021008, 1), new(4011001, 3), new(4021003, 6), new(4000030, 65), new(4003000, 45)], 50000),
                    new(1072133, [new(4021008, 1), new(4011001, 3), new(4011002, 6), new(4000030, 65), new(4003000, 45)], 50000),
                    new(1072134, [new(4021008, 1), new(4011001, 3), new(4011005, 6), new(4000030, 65), new(4003000, 45)], 50000),
                    new(1072135, [new(4021008, 1), new(4011001, 3), new(4011006, 6), new(4000030, 65), new(4003000, 45)], 50000)
                ]
            };

            var bowmanShoeCat = new RecipeCategory
            {
                Name = this.GetTalkMessage(ScriptMessage.Maker_Npc2040021_BowmanShoes),
                SelectionPrompt = stimulator ? this.GetTalkMessage(ScriptMessage.Maker_Npc2040021_BowmanShoesStimulator) : this.GetTalkMessage(ScriptMessage.Maker_Npc2040021_BowmanShoesSure),
                AskQuantity = false,
                Formulas = [
                    new(1072079, [new(4000021, 50), new(4021000, 2), new(4003000, 15)], 19000),
                    new(1072080, [new(4000021, 50), new(4021005, 2), new(4003000, 15)], 19000),
                    new(1072081, [new(4000021, 50), new(4021003, 2), new(4003000, 15)], 19000),
                    new(1072082, [new(4000021, 50), new(4021004, 2), new(4003000, 15)], 19000),
                    new(1072083, [new(4000021, 50), new(4021006, 2), new(4003000, 15)], 19000),
                    new(1072101, [new(4021002, 3), new(4021006, 1), new(4000030, 15), new(4000021, 30), new(4003000, 20)], 19000),
                    new(1072102, [new(4021003, 3), new(4021006, 1), new(4000030, 15), new(4000021, 30), new(4003000, 20)], 20000),
                    new(1072103, [new(4021000, 3), new(4021006, 1), new(4000030, 15), new(4000021, 30), new(4003000, 20)], 20000),
                    new(1072118, [new(4021000, 4), new(4003000, 30), new(4000030, 45), new(4000106, 100)], 20000),
                    new(1072119, [new(4021006, 4), new(4003000, 30), new(4000030, 45), new(4000107, 100)], 32000),
                    new(1072120, [new(4011003, 5), new(4003000, 30), new(4000030, 45), new(4000108, 100)], 32000),
                    new(1072121, [new(4021002, 5), new(4003000, 30), new(4000030, 45), new(4000099, 100)], 40000),
                    new(1072122, [new(4011001, 3), new(4021006, 3), new(4021008, 1), new(4000030, 60), new(4003000, 35), new(4000033, 80)], 40000),
                    new(1072123, [new(4011001, 3), new(4021006, 3), new(4021008, 1), new(4000030, 60), new(4003000, 35), new(4000032, 150)], 50000),
                    new(1072124, [new(4011001, 3), new(4021006, 3), new(4021008, 1), new(4000030, 60), new(4003000, 35), new(4000041, 100)], 50000),
                    new(1072125, [new(4011001, 3), new(4021006, 3), new(4021008, 1), new(4000030, 60), new(4003000, 35), new(4000042, 250)], 50000)
                ]
            };

            var mageShoeCat = new RecipeCategory
            {
                Name = this.GetTalkMessage(ScriptMessage.Maker_Npc2040021_MagicianShoes),
                SelectionPrompt = stimulator ? this.GetTalkMessage(ScriptMessage.Maker_Npc2040021_MagicianShoesStimulator) : this.GetTalkMessage(ScriptMessage.Maker_Npc2040021_MagicianShoesSure),
                AskQuantity = false,
                Formulas = [
                    new(1072075, [new(4021000, 2), new(4000021, 50), new(4003000, 15)], 18000),
                    new(1072076, [new(4021002, 2), new(4000021, 50), new(4003000, 15)], 18000),
                    new(1072077, [new(4011004, 2), new(4000021, 50), new(4003000, 15)], 18000),
                    new(1072078, [new(4021008, 1), new(4000021, 50), new(4003000, 15)], 18000),
                    new(1072089, [new(4021001, 3), new(4021006, 1), new(4000021, 30), new(4000030, 15), new(4003000, 20)], 20000),
                    new(1072090, [new(4021000, 3), new(4021006, 1), new(4000021, 30), new(4000030, 15), new(4003000, 20)], 20000),
                    new(1072091, [new(4021008, 2), new(4021006, 1), new(4000021, 40), new(4000030, 25), new(4003000, 20)], 22000),
                    new(1072114, [new(4021000, 4), new(4000030, 40), new(4000110, 100), new(4003000, 25)], 30000),
                    new(1072115, [new(4021005, 4), new(4000030, 40), new(4000111, 100), new(4003000, 25)], 30000),
                    new(1072116, [new(4011006, 2), new(4021007, 1), new(4000030, 40), new(4000100, 100), new(4003000, 25)], 35000),
                    new(1072117, [new(4021008, 2), new(4021007, 1), new(4000030, 40), new(4000112, 100), new(4003000, 30)], 40000),
                    new(1072140, [new(4021009, 1), new(4011006, 3), new(4021000, 3), new(4000030, 60), new(4003000, 40)], 50000),
                    new(1072141, [new(4021009, 1), new(4011006, 3), new(4021005, 3), new(4000030, 60), new(4003000, 40)], 50000),
                    new(1072142, [new(4021009, 1), new(4011006, 3), new(4021001, 3), new(4000030, 60), new(4003000, 40)], 50000),
                    new(1072143, [new(4021009, 1), new(4011006, 3), new(4021003, 3), new(4000030, 60), new(4003000, 40)], 50000)
                ]
            };

            var thiefShoeCat = new RecipeCategory
            {
                Name = this.GetTalkMessage(ScriptMessage.Maker_Npc2040021_ThiefShoes),
                SelectionPrompt = stimulator ? this.GetTalkMessage(ScriptMessage.Maker_Npc2040021_ThiefShoesStimulator) : this.GetTalkMessage(ScriptMessage.Maker_Npc2040021_ThiefShoesSure),
                AskQuantity = false,
                Formulas = [
                    new(1072032, [new(4011000, 3), new(4000021, 50), new(4003000, 15)], 19000),
                    new(1072033, [new(4011001, 3), new(4000021, 50), new(4003000, 15)], 19000),
                    new(1072035, [new(4011004, 2), new(4000021, 50), new(4003000, 15)], 19000),
                    new(1072036, [new(4011006, 2), new(4000021, 50), new(4003000, 15)], 21000),
                    new(1072104, [new(4021000, 3), new(4021004, 1), new(4000021, 30), new(4000030, 15), new(4003000, 20)], 20000),
                    new(1072105, [new(4021003, 3), new(4021004, 1), new(4000021, 30), new(4000030, 15), new(4003000, 20)], 20000),
                    new(1072106, [new(4021002, 3), new(4021004, 1), new(4000021, 30), new(4000030, 15), new(4003000, 20)], 20000),
                    new(1072107, [new(4021000, 5), new(4000030, 45), new(4000113, 100), new(4003000, 30)], 40000),
                    new(1072108, [new(4021003, 4), new(4000030, 45), new(4000095, 100), new(4003000, 30)], 32000),
                    new(1072109, [new(4021006, 4), new(4000030, 45), new(4000096, 100), new(4003000, 30)], 35000),
                    new(1072110, [new(4021005, 4), new(4000030, 45), new(4000097, 100), new(4003000, 30)], 35000),
                    new(1072128, [new(4011007, 2), new(4021005, 3), new(4000030, 50), new(4000114, 100), new(4003000, 35)], 50000),
                    new(1072130, [new(4011007, 2), new(4021000, 3), new(4000030, 50), new(4000115, 100), new(4003000, 35)], 50000),
                    new(1072129, [new(4011007, 2), new(4021003, 3), new(4000030, 50), new(4000109, 100), new(4003000, 35)], 50000),
                    new(1072131, [new(4011007, 2), new(4021001, 3), new(4000030, 50), new(4000036, 80), new(4003000, 35)], 50000)
                ]
            };

            var categoriesList = new List<RecipeCategory> { warriorShoeCat, bowmanShoeCat, mageShoeCat, thiefShoeCat };
            await PerformRefine(greeting, new List<RecipeCategory> { categoriesList[categoryIdx] }, 0.9f, stimulator ? stimId : 0);
        }


        // Npc: 2040022 
        public async Task make_ludi4()
        {
            string greeting = this.GetTalkMessage(ScriptMessage.Maker_Npc2040022_AhYouve);
            string[] mainOptions = [
                this.GetTalkMessage(ScriptMessage.Maker_Npc2040020_WhatsStimulator), 
                this.GetTalkMessage(ScriptMessage.Maker_Npc2040022_CreateWarrior),
                this.GetTalkMessage(ScriptMessage.Maker_Npc2040022_CreateBowman), 
                this.GetTalkMessage(ScriptMessage.Maker_Npc2040022_CreateMagician), 
                this.GetTalkMessage(ScriptMessage.Maker_Npc2040022_CreateThiefWeapon),
                this.GetTalkMessage(ScriptMessage.Maker_Npc2040022_CreateWarriorWeapon),
                this.GetTalkMessage(ScriptMessage.Maker_Npc2040022_CreateBowmanWeapon), 
                this.GetTalkMessage(ScriptMessage.Maker_Npc2040022_CreateMagicianWeapon), 
                this.GetTalkMessage(ScriptMessage.Maker_Npc2040022_CreateThief)];

            var mainSel = await AskMenu(greeting, mainOptions);
            if (mainSel < 0) return;

            if (mainSel == 0)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Maker_Npc2040020_StimulatorSpecial));
                return;
            }

            bool stimulator = mainSel > 4;
            int categoryIdx = stimulator ? mainSel - 5 : mainSel - 1;

            var warriorWeaponCat = new RecipeCategory
            {
                Name = this.GetTalkMessage(ScriptMessage.Maker_Npc2040022_WarriorWeapon),
                SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc2040022_VeryWarrior),
                AskQuantity = false,
                Formulas = [
                    new(1302008, [new(4131000, 1), new(4011001, 2), new(4011004, 2), new(4003000, 30)], 18000),
                    new(1302004, [new(4131000, 1), new(4011006, 1), new(4011001, 5), new(4021006, 3), new(4003000, 35)], 35000),
                    new(1302009, [new(4131000, 1), new(4011006, 3), new(4011001, 5), new(4021000, 5), new(4003000, 40)], 70000),
                    new(1302010, [new(4131000, 1), new(4005000, 1), new(4021008, 2), new(4011006, 4), new(4021003, 10), new(4003000, 50)], 200000),
                    new(1312005, [new(4131001, 1), new(4011001, 2), new(4021000, 2), new(4003000, 30)], 18000),
                    new(1312006, [new(4131001, 1), new(4011001, 5), new(4021000, 5), new(4011004, 3), new(4003000, 35)], 35000),
                    new(1312007, [new(4131001, 1), new(4021005, 7), new(4011001, 5), new(4021001, 5), new(4003000, 40)], 70000),
                    new(1312008, [new(4131001, 1), new(4005000, 1), new(4021008, 2), new(4011004, 8), new(4011001, 10), new(4003000, 50)], 200000),
                    new(1322014, [new(4131002, 1), new(4011001, 2), new(4011000, 2), new(4003000, 30)], 18000),
                    new(1322015, [new(4131002, 1), new(4011001, 5), new(4011000, 5), new(4011003, 3), new(4003000, 35)], 35000),
                    new(1322016, [new(4131002, 1), new(4011003, 7), new(4011001, 5), new(4011004, 5), new(4003000, 40)], 70000),
                    new(1322017, [new(4131002, 1), new(4005000, 1), new(4021008, 2), new(4011006, 4), new(4011001, 10), new(4003000, 50)], 200000),
                    new(1402002, [new(4131003, 1), new(4011001, 2), new(4021000, 1), new(4021004, 2), new(4003000, 35)], 20000),
                    new(1402006, [new(4131003, 1), new(4011006, 1), new(4011001, 5), new(4021004, 5), new(4003000, 40)], 37000),
                    new(1402007, [new(4131003, 1), new(4021003, 7), new(4011000, 5), new(4011001, 5), new(4003000, 45)], 72000),
                    new(1402003, [new(4131003, 1), new(4005000, 1), new(4021007, 2), new(4011006, 4), new(4011001, 10), new(4003000, 55)], 220000),
                    new(1412006, [new(4131004, 1), new(4021005, 2), new(4011001, 2), new(4003001, 5), new(4003000, 35)], 20000),
                    new(1412004, [new(4131004, 1), new(4011004, 5), new(4011000, 5), new(4021003, 3), new(4003000, 40)], 37000),
                    new(1412005, [new(4131004, 1), new(4011006, 3), new(4011004, 5), new(4011001, 5), new(4003000, 45)], 72000),
                    new(1412003, [new(4131004, 1), new(4005000, 1), new(4021007, 2), new(4011006, 5), new(4021006, 7), new(4003000, 55)], 220000),
                    new(1422001, [new(4131005, 1), new(4011001, 2), new(4011004, 3), new(4003000, 35)], 20000),
                    new(1422008, [new(4131005, 1), new(4011001, 5), new(4011000, 5), new(4003001, 10), new(4003000, 40)], 37000),
                    new(1422007, [new(4131005, 1), new(4011001, 5), new(4011004, 5), new(4011006, 3), new(4003000, 45)], 72000),
                    new(1422005, [new(4131005, 1), new(4005000, 1), new(4021008, 2), new(4021006, 7), new(4011006, 5), new(4003000, 55)], 220000),
                    new(1432002, [new(4131006, 1), new(4011000, 2), new(4011004, 3), new(4003000, 40)], 22000),
                    new(1432003, [new(4131006, 1), new(4011001, 5), new(4011002, 5), new(4021000, 3), new(4003000, 45)], 39000),
                    new(1432005, [new(4131006, 1), new(4011004, 3), new(4011001, 5), new(4011000, 5), new(4003000, 50)], 74000),
                    new(1432004, [new(4131006, 1), new(4005000, 1), new(4021008, 2), new(4011000, 7), new(4021000, 5), new(4003000, 60)], 240000),
                    new(1442001, [new(4131007, 1), new(4011000, 2), new(4011002, 3), new(4003000, 40)], 22000),
                    new(1442003, [new(4131007, 1), new(4011001, 5), new(4011002, 5), new(4003000, 40)], 39000),
                    new(1442009, [new(4131007, 1), new(4011006, 3), new(4011002, 5), new(4011001, 5), new(4003000, 50)], 74000),
                    new(1442005, [new(4131007, 1), new(4005000, 1), new(4021007, 2), new(4011001, 7), new(4011002, 5), new(4003000, 60)], 240000)
                ]
            };

            var bowmanWeaponCat = new RecipeCategory
            {
                Name = this.GetTalkMessage(ScriptMessage.Maker_Npc2040022_BowmanWeapon),
                SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc2040022_VeryBowman),
                AskQuantity = false,
                Formulas = [
                    new(1452005, [new(4131010, 1), new(4011001, 5), new(4011006, 5), new(4021003, 3), new(4021006, 3), new(4003000, 30)], 15000),
                    new(1452006, [new(4131010, 1), new(4011004, 7), new(4021000, 6), new(4021004, 3), new(4003000, 35)], 20000),
                    new(1452007, [new(4131010, 1), new(4021008, 1), new(4011001, 10), new(4011006, 3), new(4003000, 40), new(4000112, 100)], 40000),
                    new(1452008, [new(4131010, 1), new(4005002, 1), new(4021008, 2), new(4011001, 10), new(4021005, 6), new(4003000, 50)], 100000),
                    new(1462004, [new(4131011, 1), new(4011001, 5), new(4011005, 5), new(4021006, 3), new(4003001, 50), new(4003000, 15)], 15000),
                    new(1462005, [new(4131011, 1), new(4021008, 1), new(4011001, 8), new(4011006, 4), new(4021006, 2), new(4003000, 30)], 25000),
                    new(1462006, [new(4131011, 1), new(4021008, 2), new(4011004, 6), new(4003001, 30), new(4003000, 30)], 41000),
                    new(1462007, [new(4131011, 1), new(4021008, 2), new(4011006, 5), new(4021006, 3), new(4003001, 40), new(4003000, 40)], 100000)
                ]
            };

            var mageWeaponCat = new RecipeCategory
            {
                Name = this.GetTalkMessage(ScriptMessage.Maker_Npc2040022_MagicianWeapon),
                SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc2040022_VeryMagician),
                AskQuantity = false,
                Formulas = [
                    new(1372003, [new(4131008, 1), new(4011002, 3), new(4021002, 1), new(4003000, 10)], 15000),
                    new(1372001, [new(4131008, 1), new(4021006, 5), new(4011002, 3), new(4011001, 1), new(4003000, 15)], 30000),
                    new(1372000, [new(4131008, 1), new(4021006, 5), new(4021005, 5), new(4021007, 1), new(4003003, 1), new(4003000, 20)], 60000),
                    new(1372007, [new(4131008, 1), new(4011006, 4), new(4021003, 3), new(4021007, 2), new(4021002, 1), new(4003000, 30)], 100000),
                    new(1382002, [new(4131009, 1), new(4021006, 2), new(4021001, 1), new(4011001, 1), new(4003000, 15)], 10000),
                    new(1382001, [new(4131009, 1), new(4011001, 8), new(4021006, 5), new(4021001, 5), new(4021005, 5), new(4003000, 30)], 80000),
                    new(1382006, [new(4131009, 1), new(4005001, 2), new(4021008, 2), new(4011006, 5), new(4011004, 10), new(4003000, 40)], 200000)
                ]
            };

            var thiefWeaponCat = new RecipeCategory
            {
                Name = this.GetTalkMessage(ScriptMessage.Maker_Npc2040022_ThiefWeapon),
                SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc2040022_VeryThief),
                AskQuantity = false,
                Formulas = [
                    new(1332012, [new(4131012, 1), new(4011002, 2), new(4011001, 3), new(4003000, 30)], 20000),
                    new(1332009, [new(4131012, 1), new(4021005, 2), new(4011001, 3), new(4003000, 30)], 20000),
                    new(1332014, [new(4131012, 1), new(4021005, 1), new(4011001, 5), new(4011002, 3), new(4003000, 35)], 33000),
                    new(1332011, [new(4131012, 1), new(4011001, 7), new(4011006, 3), new(4021006, 6), new(4003000, 40)], 73000),
                    new(1332016, [new(4131012, 1), new(4005003, 1), new(4021008, 2), new(4011004, 7), new(4011001, 10), new(4003000, 50)], 230000),
                    new(1332003, [new(4131012, 1), new(4005003, 1), new(4021007, 2), new(4011006, 5), new(4011001, 10), new(4003000, 50)], 230000),
                    new(1472008, [new(4131013, 1), new(4011000, 3), new(4011001, 2), new(4000021, 50), new(4003000, 20)], 15000),
                    new(1472011, [new(4131013, 1), new(4011000, 4), new(4011001, 2), new(4000021, 80), new(4003000, 25)], 30000),
                    new(1472014, [new(4131013, 1), new(4011000, 3), new(4011001, 2), new(4000021, 100), new(4003000, 30)], 40000),
                    new(1472018, [new(4131013, 1), new(4011000, 4), new(4011001, 2), new(4000030, 40), new(4003000, 35)], 50000)
                ]
            };

            var categoriesList = new List<RecipeCategory> { warriorWeaponCat, bowmanWeaponCat, mageWeaponCat, thiefWeaponCat };
            var selectedCategory = categoriesList[categoryIdx];
            var selectItemIdx = await AskMenu(selectedCategory.SelectionPrompt,
                selectedCategory.Formulas.Select(x => this.GetTalkMessage(ScriptMessage.Maker_Npc2040022_RequiredLevel, x.TargetItemId, ItemInformationProvider.getInstance().getEquipLevelReq(x.TargetItemId))));

            if (selectItemIdx < 0) return;

            var formula = selectedCategory.Formulas[selectItemIdx];
            int stimId = stimulator ? formula.Items[0].ItemId - 998 : 0; // manual id - 998 = stimulator id

            await PerformRefine(greeting, new List<RecipeCategory> {
                new() {
                    Name = selectedCategory.Name,
                    SelectionPrompt = selectedCategory.SelectionPrompt,
                    Formulas = [formula],
                    AskQuantity = false
                }
            }, stimulator ? stimId : 0);
        }

        // Npc: 2040050 
        public async Task make_ston()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.Maker_Npc2040050_AlrightMix));

            var categories = new List<RecipeCategory>
            {
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc2040050_MakeMagic),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc2040050_HahaMystical),
                    AskQuantity = false,
                    GetResultQuantity = (id, count) => 5,
                    Formulas = [
                        new(4006000, [new(4000046, 20), new(4000027, 20), new(4021001, 1)], 4000),
                        new(4006000, [new(4000025, 20), new(4000049, 20), new(4021006, 1)], 4000),
                        new(4006000, [new(4000129, 15), new(4000130, 15), new(4021002, 1)], 4000),
                        new(4006000, [new(4000074, 15), new(4000057, 15), new(4021005, 1)], 4000),
                        new(4006000, [new(4000054, 7), new(4000053, 7), new(4021003, 1)], 4000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc2040050_MakeSummoning),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc2040050_HahaMysticalRock),
                    AskQuantity = false,
                    GetResultQuantity = (id, count) => 5,
                    Formulas = [
                        new(4006001, [new(4000046, 20), new(4000027, 20), new(4011001, 1)], 4000),
                        new(4006001, [new(4000014, 20), new(4000049, 20), new(4011003, 1)], 4000),
                        new(4006001, [new(4000132, 15), new(4000128, 15), new(4011005, 1)], 4000),
                        new(4006001, [new(4000074, 15), new(4000069, 15), new(4011002, 1)], 4000),
                        new(4006001, [new(4000080, 7), new(4000079, 7), new (4011004, 1)], 4000)
                    ]
                }
            };

            await PerformRefine(this.GetTalkMessage(ScriptMessage.Maker_Npc2040050_SeeIm), categories);
        }


        // Npc: 9201052 
        public async Task refine_TCG1()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Npc9201052_PatrolNew));
        }


        // Npc: 9201095 
        public async Task Gear_Upgrade()
        {
            if (getQuestStatus(8225) != 2)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Npc9201095_StepAside));
                return;
            }

            var categories = new List<RecipeCategory>
            {
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc9201095_WeaponForging),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc9201095_KindWeapon),
                    AskQuantity = false,
                    Formulas = [
                        new(2070018, [new(4032015, 1), new(4032016, 1), new(4032017, 1), new(4021008, 100), new(4032005, 30)], 70000),
                        new(1382060, [new(4032016, 1), new(4032017, 1), new(4032004, 400), new(4032005, 10), new(4032012, 30), new(4005001, 4)], 70000),
                        new(1442068, [new(4032015, 1), new(4032017, 1), new(4032004, 500), new(4032005, 40), new(4032012, 20), new(4005000, 4)], 70000),
                        new(1452060, [new(4032015, 1), new(4032016, 1), new(4032004, 300), new(4032005, 75), new(4032012, 10), new(4005002, 4)], 70000)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc9201095_WeaponUpgrading),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc9201095_UpgradedWeapon),
                    AskQuantity = false,
                    Formulas = [
                        new(1472074, [new(4032017, 1), new(4005001, 10), new(4021008, 20)], 75000),
                        new(1472073, [new(4032015, 1), new(4005002, 10), new(4021008, 30)], 50000),
                        new(1472075, [new(4032016, 1), new(4005000, 5), new(4021008, 20)], 50000),
                        new(1332079, [new(4032017, 1), new(4005001, 10), new(4021008, 20)], 75000),
                        new(1332078, [new(4032015, 1), new(4005002, 10), new(4021008, 30)], 50000),
                        new(1332080, [new(4032016, 1), new(4005000, 5), new(4021008, 20)], 50000),
                        new(1462054, [new(4032017, 1), new(4005001, 10), new(4021008, 20)], 75000),
                        new(1462053, [new(4032015, 1), new(4005002, 10), new(4021008, 30)], 50000),
                        new(1462055, [new(4032016, 1), new(4005000, 5), new(4021008, 20)], 50000),
                        new(1402050, [new(4032017, 1), new(4005001, 10), new(4021008, 20)], 75000),
                        new(1402049, [new(4032015, 1), new(4005002, 10), new(4021008, 30)], 50000),
                        new(1402051, [new(4032016, 1), new(4005000, 5), new(4021008, 20)], 50000)
                    ]
                }
            };

            await PerformRefine(this.GetTalkMessage(ScriptMessage.Maker_Npc9201095_PartnerHave), categories);
        }


        // Npc: 9201096 
        [ScriptName("Jack_Additional ")]
        public async Task s_Jack_Additional()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.Maker_Npc9201096_AwareAbout));
            if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.Maker_Npc9201096_SaidMethinks)))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Npc9201096_VerySee));
                return;
            }

            var categories = new List<RecipeCategory>
            {
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc9201096_CraftPotions),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc9201096_IllCrafting),
                    Formulas = [
                        new(2022284, [new(4032010, 60), new(4032011, 60), new(4032012, 45)], 75000)
                    ]
                }
            };

            await PerformRefine(this.GetTalkMessage(ScriptMessage.Maker_Npc9201096_IllCrafting), categories);
        }

        // Npc: 9201097 
        public async Task Badge_Bounty()
        {
            if (getQuestStatus(8225) != 2)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Npc9201097_ImBandit));
                return;
            }

            await SayNext(this.GetTalkMessage(ScriptMessage.Maker_Npc9201097_GotLittle));
            if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.Maker_Npc9201097_DealSimple)))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Npc9201097_HmmmShouldnt));
                return;
            }

            int[] eQuestChoices = [4032007, 4032006, 4032009, 4032008, 4032007, 4032006, 4032009, 4032008];
            int[][][] eQuestPrizes = new int[8][][];
            eQuestPrizes[0] = [[1002801, 1], [1462052, 1], [1462006, 1], [1462009, 1], [1452012, 1], [1472031, 1], [2044701, 1], [2044501, 1], [3010041, 1], [0, 750000]];
            eQuestPrizes[1] = [[1332077, 1], [1322062, 1], [1302068, 1], [4032016, 1], [2043001, 1], [2043201, 1], [2044401, 1], [2044301, 1], [3010041, 1], [0, 1250000]];
            eQuestPrizes[2] = [[1472072, 1], [1332077, 1], [1402048, 1], [1302068, 1], [4032017, 1], [4032015, 1], [2043023, 1], [2043101, 1], [2043301, 1], [3010040, 1], [0, 2500000]];
            eQuestPrizes[3] = [[1002801, 1], [1382008, 1], [1382006, 1], [4032016, 1], [4032015, 1], [2043701, 1], [2043801, 1], [3010040, 1], [0, 1750000]];
            eQuestPrizes[4] = [[0, 3500000]];
            eQuestPrizes[5] = [[0, 3500000]];
            eQuestPrizes[6] = [[0, 3500000]];
            eQuestPrizes[7] = [[0, 3500000]];

            var sel = await AskMenu(this.GetTalkMessage(ScriptMessage.Maker_Npc9201097_FirstNeed),
                eQuestChoices.Select((id, i) => $"#v{id}#  #b#t{id}# #kx {(i < 4 ? 50 : 25)}"));

            if (sel < 0) return;

            int requiredItem = eQuestChoices[sel];
            int qnt = sel < 4 ? 50 : 25;

            if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.Maker_Npc9201097_LetsSee, qnt, requiredItem)))
                return;

            if (!haveItem(requiredItem, qnt))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Npc9201097_HmmmSure, qnt, requiredItem));
                return;
            }

            var reward = eQuestPrizes[sel];
            var prize = reward[Random.Shared.Next(reward.Length)];
            int prizeItem = prize[0];
            int prizeQuantity = prize[1];

            if (prizeItem == 0)
            {
                await gainItem(requiredItem, -qnt);
                await gainMeso(prizeQuantity);
                await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Npc9201097_HeresMesos, qnt, requiredItem, prizeQuantity));
            }
            else
            {
                if (!canHold(prizeItem, prizeQuantity))
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Npc9201097_UseEtc));
                    return;
                }
                await gainItem(requiredItem, -qnt);
                await gainItem(prizeItem, (short)prizeQuantity);
                await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Npc9201097_HeresThink, qnt, requiredItem, prizeQuantity, prizeItem));
            }
        }


        // Npc: 9201098 
        public async Task Brewing_Storm()
        {
            if (getQuestStatus(8223) == 2)
            {
                if (haveItem(3992041))
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Npc9201098_DefendersYore));
                }
                else
                {
                    if (!canHold(3992041, 1))
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Npc9201098_PleaseMake));
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Npc9201098_LostKey));
                        await gainItem(3992041, 1);
                    }
                }
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Npc9201098_OBrave));
            }
        }

        // Npc: 2100001 
        public async Task make_ariant1()
        {
            if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.Maker_Npc2100001_RefineOres)))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Maker_Npc2100001_ArentHurry));
                return;
            }

            var categories = new List<RecipeCategory>
            {
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc2100001_RefineMineral),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc2100001_LikeAttitude),
                    Formulas = [
                        new(4011000, [new(4010000, 10)], 270),
                        new(4011001, [new(4010001, 10)], 270),
                        new(4011002, [new(4010002, 10)], 270),
                        new(4011003, [new(4010003, 10)], 450),
                        new(4011004, [new(4010004, 10)], 450),
                        new(4011005, [new(4010005, 10)], 450),
                        new(4011006, [new(4010006, 10)], 720),
                        new(4011008, [new(4010007, 10)], 270)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc2100001_RefineJewel),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc2100001_LikeAttitudeLets),
                    Formulas = [
                        new(4021000, [new(4020000, 10)], 450),
                        new(4021001, [new(4020001, 10)], 450),
                        new(4021002, [new(4020002, 10)], 450),
                        new(4021003, [new(4020003, 10)], 450),
                        new(4021004, [new(4020004, 10)], 450),
                        new(4021005, [new(4020005, 10)], 450),
                        new(4021006, [new(4020006, 10)], 450),
                        new(4021007, [new(4020007, 10)], 900),
                        new(4021008, [new(4020008, 10)], 2700)
                    ]
                },
                new() {
                    Name = this.GetTalkMessage(ScriptMessage.Maker_Npc2100001_RefineCrystal),
                    SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc2100001_CrystalThats),
                    Formulas = [
                        new(4005000, [new(4004000, 10)], 4500),
                        new(4005001, [new(4004001, 10)], 4500),
                        new(4005002, [new(4004002, 10)], 4500),
                        new(4005003, [new(4004003, 10)], 4500)
                    ]
                }
            };

            await PerformRefine(this.GetTalkMessage(ScriptMessage.Maker_Npc2100001_LikeAttitudeLetsJust), categories);
        }

        // Npc: 2090004 
        public async Task make_murueng()
        {
            if (isQuestActive(3821) && !haveItem(4031554) && !haveItem(4161030) && isQuestCompleted(3830))
            {
                if (canHold(4031554, 1))
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Npc2090004_BoyWanted));
                    await gainItem(4031554, 1);
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Npc2090004_BoyWantedBring));
                }
                return;
            }

            var option = await AskMenu(
                this.GetTalkMessage(ScriptMessage.Maker_Npc2090004_ManMany), 
                [this.GetTalkMessage(ScriptMessage.Maker_Npc2090004_MakeMedicine), 
                this.GetTalkMessage(ScriptMessage.Maker_Npc2090004_MakeScroll), 
                this.GetTalkMessage(ScriptMessage.Maker_Npc2090004_DonateMedicine)]);
            if (option < 0) return;

            if (option == 0) // Make a medicine
            {
                if (!haveItem(4161030, 1))
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Maker_Npc2090004_WantMake));
                    return;
                }

                var categories = new List<RecipeCategory>
                {
                    new() {
                        Name = this.GetTalkMessage(ScriptMessage.Maker_Npc2090004_MakeMedicine),
                        SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc2090004_KindMedicine),
                        Formulas = [
                            new(2022145, [new(2022116, 3)], 0),
                            new(2022146, [new(2022116, 3)], 0),
                            new(2022147, [new(4000281, 10), new(4000293, 10)], 910),
                            new(2022148, [new(4000276, 20), new(2002005, 1)], 950),
                            new(2022149, [new(4000288, 20), new(4000292, 20)], 1940),
                            new(2022150, [new(4000295, 10)], 600),
                            new(2050004, [new(2022131, 1), new(2022132, 1)], 700),
                            new(4031554, [new(4000286, 20), new(4000287, 20), new(4000293, 20)], 1000)
                        ]
                    }
                };

                await PerformRefine(this.GetTalkMessage(ScriptMessage.Maker_Npc2090004_KindMedicine), categories);
            }
            else if (option == 1) // Make a scroll
            {
                var categories = new List<RecipeCategory>
                {
                    new() {
                        Name = this.GetTalkMessage(ScriptMessage.Maker_Npc2090004_MakeScroll),
                        SelectionPrompt = this.GetTalkMessage(ScriptMessage.Maker_Npc2090004_KindScrolls),
                        AskQuantity = false,
                        Formulas = [
                            new(2043000, [new(4001124, 100), new(4010001, 10)], 0),
                            new(2043100, [new(4001124, 100), new(4010001, 10)], 0),
                            new(2043200, [new(4001124, 100), new(4010001, 10)], 0),
                            new(2043300, [new(4001124, 100), new(4010001, 10)], 0),
                            new(2043700, [new(4001124, 100), new(4010001, 10)], 0),
                            new(2043800, [new(4001124, 100), new(4010001, 10)], 0),
                            new(2044000, [new(4001124, 100), new(4010001, 10)], 0),
                            new(2044100, [new(4001124, 100), new(4010001, 10)], 0),
                            new(2044200, [new(4001124, 100), new(4010001, 10)], 0),
                            new(2044300, [new(4001124, 100), new(4010001, 10)], 0),
                            new(2044400, [new(4001124, 100), new(4010001, 10)], 0),
                            new(2044500, [new(4001124, 100), new(4010001, 10)], 0),
                            new(2044600, [new(4001124, 100), new(4010001, 10)], 0),
                            new(2044700, [new(4001124, 100), new(4010001, 10)], 0),
                            new(2044800, [new(4001124, 100), new(4010001, 10)], 0),
                            new(2044900, [new(4001124, 100), new(4010001, 10)], 0)
                        ],
                        GetResultQuantity = (id, count) => {
                            // 10% chance for 60% scroll
                            if (Random.Shared.Next(10) == 0) return 1; // Actually in JS it does item += 1
                            return 1;
                        }
                    }
                };

                // Special handling for the scroll reward logic (10% chance for 60% scroll)
                var category = categories[0];
                var selectItemIdx = await AskMenu(category.SelectionPrompt,
                    ["Scroll for One-Handed Sword for ATT", "Scroll for One-Handed Axe for ATT", "Scroll for One-Handed BW for ATT",
                    "Scroll for Dagger for ATT", "Scroll for Wand for Magic Att.", "Scroll for Staff for Magic Att.",
                    "Scroll for Two-handed Sword for ATT.", "Scroll for Two-handed Axe for ATT", "Scroll for Two-handed BW for ATT",
                    "Scroll for Spear for ATT", "Scroll for Pole Arm for ATT", "Scroll for Bow for ATT", "Scroll for Crossbow for ATT ",
                    "Scroll for Claw for ATT", "Scroll for Knuckle for ATT", "Scroll for Gun for ATT"]);

                if (selectItemIdx < 0) return;
                var selected = category.Formulas[selectItemIdx];
                int targetItem = selected.TargetItemId;
                if (Random.Shared.Next(10) == 0) targetItem += 1; // 10% chance for 60% scroll

                // Use PerformRefine with the potentially changed target item
                await PerformRefine(category.SelectionPrompt, new List<RecipeCategory> {
                    new() {
                        Name = category.Name,
                        SelectionPrompt = category.SelectionPrompt,
                        Formulas = [new(targetItem, selected.Items, selected.Cost)],
                        AskQuantity = false
                    }
                });
            }
            else // Donate medicine ingredients
            {
                int[] items = [4000276, 4000277, 4000278, 4000279, 4000280, 4000291, 4000292, 4000286, 4000287, 4000293, 4000294, 4000298, 4000284, 4000288, 4000285, 4000282, 4000295, 4000289, 4000296, 4000297];
                int[][] rewdRange = [[7, 7], [7, 7], [7, 8], [10, 10], [11, 11], [8, 8], [7, 8], [7, 9], [7, 8], [9, 9], [10, 10], [10, 11], [11, 11], [11, 12], [13, 13], [13, 13], [14, 14], [15, 15], [15, 16], [17, 17]];

                var sel = await AskMenu(this.GetTalkMessage(ScriptMessage.Maker_Npc2090004_WishDonate),
                    items.Select(id => $"#v{id}# #t{id}#"));

                if (sel < 0) return;

                int itemToDonate = items[sel];
                if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.Maker_Npc2090004_SureWant, itemToDonate)))
                    return;

                if (!haveItem(itemToDonate, 100))
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Npc2090004_PleaseMake));
                    return;
                }

                int[] range = rewdRange[sel];
                int rewardQty = Random.Shared.Next(range[0], range[1] + 1);

                if (!canHold(4001124, (short)rewardQty))
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Npc2090004_PleaseMake));
                    return;
                }

                await gainItem(itemToDonate, -100);
                await gainItem(4001124, (short)rewardQty);
                await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Npc2090004_ThankMuch));
            }
        }

        // Npc: 2080000 — 龙之武器制作
        public async Task minar_weapon()
        {
            getPlayer().setCS(true);

            var options = new List<string>
            {
                this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_WhatsStimulator),
                this.GetTalkMessage(ScriptMessage.Maker_Npc2040022_CreateWarrior),
                this.GetTalkMessage(ScriptMessage.Maker_Npc2040022_CreateBowman),
                this.GetTalkMessage(ScriptMessage.Maker_Npc2040022_CreateMagician),
                this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_CreateThiefWeapon), 
                this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_CreatePirateWeapon),
                this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_CreateWarrior), 
                this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_CreateBowman),
                this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_CreateMagician),
                this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_CreateThief),
                this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_CreatePirate)
            };
            if (isQuestStarted(7301) || isQuestStarted(7303))
            {
                options.Add(this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_Make));
            }

            var selStr = this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_DragonsPower);
            for (var i = 0; i < options.Count; i++)
            {
                selStr += $"\r\n#L{i}# {options[i]}#l";
            }

            var selectedType = await AskMenu(selStr);
            if (selectedType < 0 || selectedType >= options.Count)
                return;

            var stimulator = selectedType > 5 && selectedType < 11;
            if (stimulator)
            {
                selectedType -= 5;
            }

            if (selectedType == 0)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_StimulatorSpecial));
                return;
            }

            // 各职业的武器列表
            string[] weaponNames = selectedType switch
            {
                1 => [
                    this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_DragonCarabella), 
                    this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_DragonAxe), 
                    this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_DragonMace), 
                    this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_DragonClaymore), 
                    this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_DragonBattle), 
                    this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_DragonFlame), 
                    this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_DragonFaltizan), 
                    this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_DragonChelbird)],
                2 => [this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_DragonShiner), this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_DragonShinerCross)],
                3 => [this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_DragonWand), this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_DragonStaff)],
                4 => [this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_DragonKanzir), this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_DragonKreda), this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_DragonGreen)],
                5 => [this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_DragonSlash), this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_DragonfireRevolver)],
                _ => [],
            };

            int[] itemSet;
            int[][] matSet;
            int[][] matQtySet;
            int cost;

            switch (selectedType)
            {
                case 1:
                    itemSet = [1302059, 1312031, 1322052, 1402036, 1412026, 1422028, 1432038, 1442045];
                    matSet = [
                        [1302056, 4000244, 4000245, 4005000], [1312030, 4000244, 4000245, 4005000],
                        [1322045, 4000244, 4000245, 4005000], [1402035, 4000244, 4000245, 4005000],
                        [1412021, 4000244, 4000245, 4005000], [1422027, 4000244, 4000245, 4005000],
                        [1432030, 4000244, 4000245, 4005000], [1442044, 4000244, 4000245, 4005000]];
                    matQtySet = [.. Enumerable.Repeat(new[] { 1, 20, 25, 8 }, 8)];
                    cost = 120000;
                    break;
                case 2:
                    itemSet = [1452044, 1462039];
                    matSet = [[1452019, 4000244, 4000245, 4005000, 4005002], [1462015, 4000244, 4000245, 4005000, 4005002]];
                    matQtySet = [[1, 20, 25, 3, 5], [1, 20, 25, 5, 3]];
                    cost = 120000;
                    break;
                case 3:
                    itemSet = [1372032, 1382036];
                    matSet = [[1372010, 4000244, 4000245, 4005001, 4005003], [1382035, 4000244, 4000245, 4005001, 4005003]];
                    matQtySet = [[1, 20, 25, 6, 2], [1, 20, 25, 6, 2]];
                    cost = 120000;
                    break;
                case 4:
                    itemSet = [1332049, 1332050, 1472051];
                    matSet = [[1332051, 4000244, 4000245, 4005000, 4005002], [1332052, 4000244, 4000245, 4005002, 4005003], [1472053, 4000244, 4000245, 4005002, 4005003]];
                    matQtySet = [[1, 20, 25, 5, 3], [1, 20, 25, 3, 5], [1, 20, 25, 2, 6]];
                    cost = 120000;
                    break;
                case 5:
                    itemSet = [1482013, 1492013];
                    matSet = [[1482012, 4000244, 4000245, 4005000, 4005002], [1492012, 4000244, 4000245, 4005000, 4005002]];
                    matQtySet = [[1, 20, 25, 5, 3], [1, 20, 25, 3, 5]];
                    cost = 120000;
                    break;
                case 11:
                    itemSet = [4001078];
                    matSet = [[4011001, 4011002, 4001079]];
                    matQtySet = [[1, 1, 1]];
                    cost = 25000;
                    break;
                default:
                    return;
            }

            var selectedItem = await AskMenu(this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_VeryWeapon)
                + string.Join("\r\n", weaponNames.Select((n, i) => $"#L{i}# {n}#l")));
            if (selectedItem < 0 || selectedItem >= itemSet.Length)
                return;

            var item = itemSet[selectedItem];
            var mats = matSet[selectedItem];
            var matQty = matQtySet[selectedItem];

            var stimID = stimulator ? GetStimulatorId(item) : 0;
            var prompt = this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_WantMake, item);
            if (stimulator)
            {
                prompt += $"\r\n#i{stimID}# 1 #t{stimID}#";
            }
            for (var i = 0; i < mats.Length; i++)
            {
                prompt += $"\r\n#i{mats[i]}# {matQty[i]} #t{mats[i]}#";
            }
            if (cost > 0)
            {
                prompt += this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_Meso, cost);
            }

            if (!await AskYesNo(prompt))
                return;

            if (!canHold(item))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_CheckInventory));
                return;
            }

            if (getMeso() < cost)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_FeeGood));
                return;
            }

            for (var i = 0; i < mats.Length; i++)
            {
                if (!haveItem(mats[i], matQty[i]))
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_ImAfraid));
                    return;
                }
            }

            if (stimulator && !haveItem(stimID))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_ImAfraid));
                return;
            }

            for (var i = 0; i < mats.Length; i++)
            {
                await gainItem(mats[i], -matQty[i]);
            }
            await gainMeso(-cost);

            if (stimulator)
            {
                await gainItem(stimID, -1);
                if (Random.Shared.Next(10) != 0)
                {
                    await gainItem(item, 1, true);
                    await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_ProcessComplete));
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_UnfortunatelyDragons));
                }
            }
            else
            {
                await gainItem(item, 1);
                await SayOK(this.GetTalkMessage(ScriptMessage.Maker_Npc2080000_ProcessComplete));
            }
        }

        private static int GetStimulatorId(int equipId)
        {
            return (equipId / 10000) switch
            {
                130 => 4130002,
                131 => 4130003,
                132 => 4130004,
                140 => 4130005,
                141 => 4130006,
                142 => 4130007,
                143 => 4130008,
                144 => 4130009,
                137 => 4130010,
                138 => 4130011,
                145 => 4130012,
                146 => 4130013,
                133 => 4130014,
                147 => 4130015,
                148 => 4130016,
                149 => 4130017,
                _ => 4130002,
            };
        }

    }
}
