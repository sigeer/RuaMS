using Application.Core.Channel.DataProviders;
using Application.Core.Client;
using Application.Core.tools.RandomUtils;
using Application.Shared.Constants.Item;
using Application.Shared.Items;
using Application.Templates.Etc;
using Application.Templates.Item.Etc;
using Application.Templates.Reader;
using Application.Utility.Configs;
using Application.Utility.Extensions;
using client.inventory.manipulator;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Serilog;
using server.life;

namespace Application.Core.Channel.Maker
{
    public class MakerManager
    {
        readonly ILogger<MakerManager> _logger;
        readonly ItemInformationProvider ii;
        readonly IMapper _mapper;
        readonly IProvider<ItemMakeTemplate> _provider;
        readonly IItemProvider _itemProvider;

        #region

        Dictionary<int, int> mobCrystalMakerCache = new();

        public MakerManager(ILogger<MakerManager> logger, ItemInformationProvider ii, IMapper mapper)
        {
            _logger = logger;
            this.ii = ii;
            _mapper = mapper;
            _provider = ProviderSource.Instance.GetProvider<IProvider<ItemMakeTemplate>>(ProviderType.EtcItemMake);
            _itemProvider = ProviderSource.Instance.GetProvider<IItemProvider>(ProviderType.Item);
        }

        private int getCrystalForLevel(int level)
        {
            int range = (level - 1) / 10;

            if (range < 5)
            {
                return ItemId.BASIC_MONSTER_CRYSTAL_1;
            }
            else if (range > 11)
            {
                return ItemId.ADVANCED_MONSTER_CRYSTAL_3;
            }
            else
            {
                return range switch
                {
                    5 => ItemId.BASIC_MONSTER_CRYSTAL_2,
                    6 => ItemId.BASIC_MONSTER_CRYSTAL_3,
                    7 => ItemId.INTERMEDIATE_MONSTER_CRYSTAL_1,
                    8 => ItemId.INTERMEDIATE_MONSTER_CRYSTAL_2,
                    9 => ItemId.INTERMEDIATE_MONSTER_CRYSTAL_3,
                    10 => ItemId.ADVANCED_MONSTER_CRYSTAL_1,
                    _ => ItemId.ADVANCED_MONSTER_CRYSTAL_2
                };
            }
        }

        /// <summary>
        /// 通过物品找到怪物，再根据怪物等级推断物品的怪物结晶等级
        /// </summary>
        /// <param name="leftoverId"></param>
        /// <returns></returns>
        public int getMakerCrystalFromLeftover(int leftoverId)
        {
            try
            {
                if (mobCrystalMakerCache.TryGetValue(leftoverId, out var itemid))
                    return itemid;

                itemid = -1;

                var droppers = MonsterInformationProvider.getInstance().FindDroppers(itemid);
                if (droppers.Count > 0)
                    itemid = getCrystalForLevel(LifeFactory.Instance.getMonsterLevel(droppers.First()));

                mobCrystalMakerCache.Add(leftoverId, itemid);
                return itemid;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.ToString());
            }

            return -1;
        }

        public MakerItemCreateEntry? getMakerItemEntry(int toCreate)
        {
            var table = _provider.GetItem(toCreate);
            if (table != null)
            {
                var makerEntry = new MakerItemCreateEntry(table.Meso, table.ReqLevel, table.ReqSkillLevel);
                foreach (var item in table.Recipes)
                {
                    makerEntry.addReqItem(item.Item, item.Count);
                }
                var reward = new LotteryMachine<ItemMakeReward, int>(table.Rewards, x => x.Prob).GetRandomItem();
                makerEntry.addGainItem(reward.Item, reward.ItemNum);
                return makerEntry;
            }
            return null;
        }

        public int getMakerCrystalFromEquip(int equipId)
        {
            try
            {
                return getCrystalForLevel(ii.getEquipLevelReq(equipId));
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.ToString());
            }

            return -1;
        }

        public int getMakerStimulantFromEquip(int equipId)
        {
            try
            {
                return getCrystalForLevel(ii.getEquipLevelReq(equipId));
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.ToString());
            }

            return -1;
        }

        public int getMakerDisassembledFee(int itemId)
        {
            int fee = -1;
            try
            {
                var table = getMakerItemEntry(itemId);
                if (table != null)
                {
                    // cost is 13.6363~ % of the original value, trim by 1000.
                    float val = (float)(table.getCost() * 0.13636363636364);
                    fee = (int)(val / 1000);
                    fee *= 1000;
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.ToString());
            }
            return fee;
        }

        public int getMakerStimulant(int itemId)
        {
            return _provider.GetItem(itemId)?.Catalyst ?? -1;
        }
        #endregion

        public MakerItemCreateEntry? getItemCreateEntry(int toCreate, int stimulantid, Dictionary<int, short> reagentids)
        {
            var makerEntry = getMakerItemEntry(toCreate);
            if (makerEntry == null || makerEntry.isInvalid())
            {
                return makerEntry;
            }

            // THEY DECIDED FOR SOME BIZARRE PATTERN ON THE FEE THING, ALMOST RANDOMIZED.
            if (stimulantid != -1)
            {
                makerEntry.addCost(getMakerStimulantFee(toCreate));
            }

            if (reagentids.Count > 0)
            {
                foreach (var r in reagentids)
                {
                    makerEntry.addCost((getMakerReagentFee(toCreate, ((r.Key % 10) + 1))) * r.Value);
                }
            }

            makerEntry.trimCost();  // "commit" the real cost of the recipe.
            return makerEntry;
        }

        public MakerItemCreateEntry generateLeftoverCrystalEntry(int fromLeftoverid, int crystalId)
        {
            MakerItemCreateEntry ret = new MakerItemCreateEntry(0, 0, 1);
            ret.addReqItem(fromLeftoverid, 100);
            ret.addGainItem(crystalId, 1);
            return ret;
        }

        public MakerItemCreateEntry generateDisassemblyCrystalEntry(int fromEquipid, int cost, List<ItemQuantity> gains)
        {
            // equipment at specific position already taken
            MakerItemCreateEntry ret = new MakerItemCreateEntry(cost, 0, 1);
            ret.addReqItem(fromEquipid, 1);
            foreach (var p in gains)
            {
                ret.addGainItem(p.ItemId, p.Quantity);
            }
            return ret;
        }

        private double getMakerStimulantFee(int itemid)
        {
            if (YamlConfig.config.server.USE_MAKER_FEE_HEURISTICS)
            {
                EquipType et = EquipTypeUtils.getEquipTypeById(itemid);
                int eqpLevel = ii.getEquipLevelReq(itemid);

                switch (et)
                {
                    case EquipType.CAP:
                        return 1145.736246 * Math.Exp(0.03336832546 * eqpLevel);

                    case EquipType.LONGCOAT:
                        return 2117.469118 * Math.Exp(0.03355349137 * eqpLevel);

                    case EquipType.SHOES:
                        return 1218.624674 * Math.Exp(0.0342266043 * eqpLevel);

                    case EquipType.GLOVES:
                        return 2129.531152 * Math.Exp(0.03421778102 * eqpLevel);

                    case EquipType.COAT:
                        return 1770.630579 * Math.Exp(0.03359768677 * eqpLevel);

                    case EquipType.PANTS:
                        return 1442.98837 * Math.Exp(0.03444783295 * eqpLevel);

                    case EquipType.SHIELD:
                        return 6312.40136 * Math.Exp(0.0237929527 * eqpLevel);

                    default:    // weapons
                        return 4313.581428 * Math.Exp(0.03147837094 * eqpLevel);
                }
            }
            else
            {
                return 14000;
            }
        }

        private double getMakerReagentFee(int itemid, int reagentLevel)
        {
            if (YamlConfig.config.server.USE_MAKER_FEE_HEURISTICS)
            {
                EquipType et = EquipTypeUtils.getEquipTypeById(itemid);
                int eqpLevel = ii.getEquipLevelReq(itemid);

                switch (et)
                {
                    case EquipType.CAP:
                        return 5592.01613 * Math.Exp(0.02914576018 * eqpLevel) * reagentLevel;

                    case EquipType.LONGCOAT:
                        return 3405.23441 * Math.Exp(0.03413001038 * eqpLevel) * reagentLevel;

                    case EquipType.SHOES:
                        return 2115.697484 * Math.Exp(0.0354881705 * eqpLevel) * reagentLevel;

                    case EquipType.GLOVES:
                        return 4684.040894 * Math.Exp(0.03166500585 * eqpLevel) * reagentLevel;

                    case EquipType.COAT:
                        return 2955.89017 * Math.Exp(0.0339948456 * eqpLevel) * reagentLevel;

                    case EquipType.PANTS:
                        return 1774.722181 * Math.Exp(0.03854321409 * eqpLevel) * reagentLevel;

                    case EquipType.SHIELD:
                        return 12014.11296 * Math.Exp(0.02185471162 * eqpLevel) * reagentLevel;

                    default:    // weapons
                        return 4538.650247 * Math.Exp(0.0371980303 * eqpLevel) * reagentLevel;
                }
            }
            else
            {
                return 8000 * reagentLevel;
            }
        }


        // checks and prevents hackers from PE'ing Maker operations with invalid operations
        public bool removeOddMakerReagents(int toCreate, Dictionary<int, short> reagentids)
        {
            Dictionary<int, int> reagentType = new();
            List<int> toRemove = new();

            bool isWeapon = ItemConstants.isWeapon(toCreate) || YamlConfig.config.server.USE_MAKER_PERMISSIVE_ATKUP;  // thanks Vcoc for finding a case where a weapon wouldn't be counted as such due to a bounding on isWeapon

            foreach (var r in reagentids)
            {
                int curRid = r.Key;
                int type = r.Key / 100;

                if (type < 42502 && !isWeapon)
                {     // only weapons should gain w.att/m.att from these.
                    return false;   //toRemove.Add(curRid);
                }
                else
                {
                    if (reagentType.TryGetValue(type, out var tableRid))
                    {
                        if (tableRid < curRid)
                        {
                            toRemove.Add(tableRid);
                            reagentType.AddOrUpdate(type, curRid);
                        }
                        else
                        {
                            toRemove.Add(curRid);
                        }
                    }
                    else
                    {
                        reagentType.AddOrUpdate(type, curRid);
                    }
                }
            }

            // removing less effective gems of repeated type
            foreach (int i in toRemove)
            {
                reagentids.Remove(i);
            }

            // the Maker skill will use only one of each gem
            foreach (int i in reagentids.Keys)
            {
                reagentids.AddOrUpdate(i, (short)1);
            }

            return true;
        }

        public int getMakerReagentSlots(int itemId)
        {
            try
            {
                int eqpLevel = ii.getEquipLevelReq(itemId);

                if (eqpLevel < 78)
                {
                    return 1;
                }
                else if (eqpLevel >= 78 && eqpLevel < 108)
                {
                    return 2;
                }
                else
                {
                    return 3;
                }
            }
            catch
            {
                return 0;
            }
        }

        public KeyValuePair<int, List<ItemQuantity>>? generateDisassemblyInfo(int itemId)
        {
            var table = getMakerItemEntry(itemId);
            if (table == null)
                return null;
            int fee = -1;

            // cost is 13.6363~ % of the original value, trim by 1000.
            float val = (float)(table.getCost() * 0.13636363636364);
            fee = (int)(val / 1000);
            fee *= 1000;

            if (fee > -1 && table.getReqItems().Count > 0)
            {
                return new(fee, table.getReqItems());
            }

            return null;
        }


        public async Task<short> getCreateStatus(IChannelClient c, MakerItemCreateEntry? recipe)
        {
            if (recipe == null || recipe.isInvalid())
            {
                return -1;
            }

            if (!hasItems(c, recipe))
            {
                return 1;
            }

            if (c.OnlinedCharacter.getMeso() < recipe.getCost())
            {
                return 2;
            }

            if (c.OnlinedCharacter.getLevel() < recipe.getReqLevel())
            {
                return 3;
            }

            if (c.OnlinedCharacter.GetMakerSkillLevel() < recipe.getReqSkillLevel())
            {
                return 4;
            }

            List<int> addItemids = new();
            List<int> addQuantity = new();
            List<int> rmvItemids = new();
            List<int> rmvQuantity = new();

            foreach (var p in recipe.getReqItems())
            {
                rmvItemids.Add(p.ItemId);
                rmvQuantity.Add(p.Quantity);
            }

            foreach (var p in recipe.getGainItems())
            {
                addItemids.Add(p.ItemId);
                addQuantity.Add(p.Quantity);
            }

            if (!await c.OnlinedCharacter.getAbstractPlayerInteraction().canHoldAllAfterRemoving(addItemids, addQuantity, rmvItemids, rmvQuantity))
            {
                return 5;
            }

            return 0;
        }

        private bool hasItems(IChannelClient c, MakerItemCreateEntry recipe)
        {
            foreach (var p in recipe.getReqItems())
            {
                int itemId = p.ItemId;
                if (c.OnlinedCharacter.countItem(itemId) < p.Quantity)
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> addBoostedMakerItem(IChannelClient c, int itemid, int stimulantid, Dictionary<int, short> reagentids)
        {
            if (stimulantid != -1 && !ItemInformationProvider.rollSuccessChance(90.0))
            {
                return false;
            }

            var eqp = ii.getEquipById(itemid);
            if (ItemConstants.isAccessory(eqp.getItemId()) && eqp.getUpgradeSlots() <= 0)
            {
                eqp.setUpgradeSlots(3);
            }

            if (YamlConfig.config.server.USE_ENHANCED_CRAFTING)
            {
                if (!(c.OnlinedCharacter.isGM() && YamlConfig.config.server.USE_PERFECT_GM_SCROLL))
                {
                    eqp.setUpgradeSlots(eqp.getUpgradeSlots() + 1);
                }
                eqp = ItemInformationProvider.getInstance().scrollEquipWithId(eqp, ItemId.CHAOS_SCROll_60, true, ItemId.CHAOS_SCROll_60, c.OnlinedCharacter.isGM());
            }

            if (reagentids.Count > 0)
            {
                Dictionary<string, int> stats = new();
                List<short> randOption = new();
                List<short> randStat = new();

                foreach (var r in reagentids)
                {
                    var reagent = _itemProvider.GetRequiredItem<EtcMakeItemTemplate>(r.Key);
                    if (reagent != null)
                    {
                        if (reagent.RandOption != 0 || reagent.RandStat != 0)
                        {
                            if (reagent.RandStat != 0)
                            {
                                randStat.Add((short)(reagent.RandStat * r.Value));
                            }
                            else
                            {
                                randOption.Add((short)(reagent.RandOption * r.Value));
                            }
                        }

                        ItemInformationProvider.improveEquipStats(eqp, reagent);
                    }
                }


                foreach (short sh in randStat)
                {
                    ii.scrollOptionEquipWithChaos(eqp, sh, false);
                }

                foreach (short sh in randOption)
                {
                    ii.scrollOptionEquipWithChaos(eqp, sh, true);
                }
            }

            if (stimulantid != -1)
            {
                eqp = ii.randomizeUpgradeStats(eqp);
            }

            await InventoryManipulator.addFromDrop(c, eqp, false);
            return true;
        }
    }

}
