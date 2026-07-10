using Application.Templates.Exceptions;
using Application.Templates.Item;
using Application.Templates.Item.Cash;
using Application.Templates.Item.Consume;
using Application.Templates.Item.Etc;
using Application.Templates.Item.Install;
using Application.Templates.Item.Pet;
using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Duey.Abstractions;
using Duey.Provider.WZ.Files;
using Microsoft.Extensions.Logging;

namespace Application.Templates.Reader.Img.Provider
{
    public sealed class ItemProvider : AbstractGroupProvider<AbstractItemTemplate>, IItemProvider
    {
        public override ProviderType Type => ProviderType.Item;

        public ItemProvider(IWzPathResolver resolver) : base(resolver)
        {
        }

        protected override IEnumerable<AbstractItemTemplate> GetDataFromImg(string? path)
        {
            if (path == null) return [];

            if (path.Contains("Cash")) return IterateCashBundleItem(path);
            else if (path.Contains("Consume")) return LoadConsume(path);
            else if (path.Contains("Pet")) return LoadPets(path);
            else if (path.Contains("Install")) return LoadInstall(path);
            else if (path.Contains("Etc")) return LoadEtc(path);
            return [];
        }

        public List<MonsterCardItemTemplate> GetAllMonsterCard()
            => LoadConsume(_resolver.ResolveGroup(Type).FirstOrDefault(x => x.EndsWith("0238.img"))!).OfType<MonsterCardItemTemplate>().ToList();

        public List<MasteryItemTemplate> GetAllSkillBook()
            => LoadConsume(_resolver.ResolveGroup(Type).FirstOrDefault(x => x.EndsWith("0228.img") || x.EndsWith("0229.img"))!).OfType<MasteryItemTemplate>().ToList();

        public List<ConsumeItemTemplate> GetAllConsume()
            => _resolver.ResolveGroup(Type).Where(x => x.Contains("Consume")).SelectMany(x => LoadConsume(x)).OfType<ConsumeItemTemplate>().ToList();

        private IDataNode LoadRootNode(string imgPath)
        {
            var fullPath = _resolver.ResolveFullPath(imgPath);
            return new WZImage(fullPath);
        }

        private IEnumerable<AbstractItemTemplate> LoadPets(string imgPath)
        {
            try
            {
                var rootNode = LoadRootNode(imgPath);
                if (!int.TryParse(rootNode.Name.AsSpan(0, 7), out var petItemId))
                    throw new TemplateFormatException("Item.wz", imgPath);

                var pEntry = new PetItemTemplate(petItemId);
                PetItemTemplateGenerated_Duey.ApplyProperties(pEntry, rootNode);
                pEntry.Adjust();
                InsertItem(pEntry);
                return [pEntry];
            }
            catch (Exception ex)
            {
                LibLog.Logger.LogError(ex.ToString());
                return [];
            }
        }

        private IEnumerable<AbstractItemTemplate> LoadInstall(string imgPath)
        {
            try
            {
                var rootNode = LoadRootNode(imgPath);
                List<AbstractItemTemplate> all = [];
                foreach (var itemNode in rootNode.Children)
                {
                    if (int.TryParse(itemNode.Name, out var installId))
                    {
                        var template = new InstallItemTemplate(installId);
                        InstallItemTemplateGenerated_Duey.ApplyProperties(template, itemNode);
                        InsertItem(template);
                        all.Add(template);
                    }
                }
                return all;
            }
            catch (Exception ex)
            {
                LibLog.Logger.LogError(ex.ToString());
                return [];
            }
        }

        private IEnumerable<AbstractItemTemplate> LoadEtc(string imgPath)
        {
            try
            {
                var rootNode = LoadRootNode(imgPath);
                List<AbstractItemTemplate> all = [];
                if (!int.TryParse(rootNode.Name.AsSpan(0, 4), out var groupId))
                    throw new TemplateFormatException("Item.wz", imgPath);

                foreach (var itemNode in rootNode.Children)
                {
                    if (int.TryParse(itemNode.Name, out var itemId))
                    {
                        AbstractItemTemplate template;
                        if (groupId == 422)
                        {
                            var inc = new IncubatorItemTemplate(itemId);
                            IncubatorItemTemplateGenerated_Duey.ApplyProperties(inc, itemNode);
                            template = inc;
                        }
                        else
                        {
                            var etc = new EtcItemTemplate(itemId);
                            EtcItemTemplateGenerated_Duey.ApplyProperties(etc, itemNode);
                            template = etc;
                        }
                        InsertItem(template);
                        all.Add(template);
                    }
                }
                return all;
            }
            catch (Exception ex)
            {
                LibLog.Logger.LogError(ex.ToString());
                return [];
            }
        }

        private IEnumerable<AbstractItemTemplate> LoadConsume(string imgPath)
        {
            try
            {
                var rootNode = LoadRootNode(imgPath);
                List<AbstractItemTemplate> all = [];
                if (!int.TryParse(rootNode.Name.AsSpan(0, 4), out var groupId))
                    throw new TemplateFormatException("Item.wz", imgPath);

                foreach (var rootItem in rootNode.Children)
                {
                    if (int.TryParse(rootItem.Name, out var itemId))
                    {
                        var template = ProcessConsumeItem(groupId, itemId, rootItem);
                        InsertItem(template);
                        all.Add(template);
                    }
                }
                return all;
            }
            catch (Exception ex)
            {
                LibLog.Logger.LogError(ex.ToString());
                return [];
            }
        }

        public ConsumeItemTemplate ProcessConsumeItem(int groupId, int itemId, IDataNode itemNode)
        {
            return groupId switch
            {
                203 => CreateItem<TownScrollItemTemplate>(itemId, itemNode, TownScrollItemTemplateGenerated_Duey.ApplyProperties),
                204 => CreateItem<ScrollItemTemplate>(itemId, itemNode, ScrollItemTemplateGenerated_Duey.ApplyProperties),
                206 or 207 or 233 => CreateItem<BulletItemTemplate>(itemId, itemNode, BulletItemTemplateGenerated_Duey.ApplyProperties),
                210 => CreateItem<SummonMobItemTemplate>(itemId, itemNode, SummonMobItemTemplateGenerated_Duey.ApplyProperties),
                212 => CreateItem<PetFoodItemTemplate>(itemId, itemNode, PetFoodItemTemplateGenerated_Duey.ApplyProperties),
                227 => CreateItem<CatchMobItemTemplate>(itemId, itemNode, CatchMobItemTemplateGenerated_Duey.ApplyProperties),
                228 or 229 => CreateItem<MasteryItemTemplate>(itemId, itemNode, MasteryItemTemplateGenerated_Duey.ApplyProperties),
                237 => CreateItem<SolomenItemTemplate>(itemId, itemNode, SolomenItemTemplateGenerated_Duey.ApplyProperties),
                238 => CreateItem<MonsterCardItemTemplate>(itemId, itemNode, MonsterCardItemTemplateGenerated_Duey.ApplyProperties),
                243 => CreateItem<ScriptItemTemplate>(itemId, itemNode, ScriptItemTemplateGenerated_Duey.ApplyProperties),
                221 => CreateItem<MorphItemTemplate>(itemId, itemNode, MorphItemTemplateGenerated_Duey.ApplyProperties),
                236 => CreateItem<GhostItemTemplate>(itemId, itemNode, GhostItemTemplateGenerated_Duey.ApplyProperties),
                200 or 201 or 202 or 205 => CreateItem<PotionItemTemplate>(itemId, itemNode, PotionItemTemplateGenerated_Duey.ApplyProperties),
                226 or 245 => CreateItem<OtherConsumeItemTemplate>(itemId, itemNode, OtherConsumeItemTemplateGenerated_Duey.ApplyProperties),
                _ => CreateItem<ConsumeItemTemplate>(itemId, itemNode, ConsumeItemTemplateGenerated_Duey.ApplyProperties),
            };
        }

        private static T CreateItem<T>(int itemId, IDataNode itemNode, Action<T, IDataNode> apply) where T : AbstractItemTemplate
        {
            var template = (T)Activator.CreateInstance(typeof(T), itemId)!;
            apply(template, itemNode);
            return template;
        }

        private AbstractItemTemplate ProcessCashItemByGroup(int groupId, int itemId, IDataNode itemNode)
        {
            return groupId switch
            {
                503 => CreateItem<HiredMerchantItemTemplate>(itemId, itemNode, HiredMerchantItemTemplateGenerated_Duey.ApplyProperties),
                512 => CreateItem<MapBuffItemTemplate>(itemId, itemNode, MapBuffItemTemplateGenerated_Duey.ApplyProperties),
                513 => CreateItem<SafetyCharmItemTemplate>(itemId, itemNode, SafetyCharmItemTemplateGenerated_Duey.ApplyProperties),
                518 => CreateItem<WaterOfLifeItemTemplate>(itemId, itemNode, WaterOfLifeItemTemplateGenerated_Duey.ApplyProperties),
                520 => CreateItem<MesoBagItemTemplate>(itemId, itemNode, MesoBagItemTemplateGenerated_Duey.ApplyProperties),
                521 or 536 => CreateItem<CouponItemTemplate>(itemId, itemNode, CouponItemTemplateGenerated_Duey.ApplyProperties),
                524 => CreateItem<CashPetFoodItemTemplate>(itemId, itemNode, CashPetFoodItemTemplateGenerated_Duey.ApplyProperties),
                528 => CreateItem<AreaEffectItemTemplate>(itemId, itemNode, AreaEffectItemTemplateGenerated_Duey.ApplyProperties),
                530 => CreateItem<MorphItemTemplate>(itemId, itemNode, MorphItemTemplateGenerated_Duey.ApplyProperties),
                550 => CreateItem<ExtendItemTimeItemTemplate>(itemId, itemNode, ExtendItemTimeItemTemplateGenerated_Duey.ApplyProperties),
                553 => CreateItem<CashPackagedItemTemplate>(itemId, itemNode, CashPackagedItemTemplateGenerated_Duey.ApplyProperties),
                _ => CreateItem<CashItemTemplate>(itemId, itemNode, CashItemTemplateGenerated_Duey.ApplyProperties),
            };
        }

        private IEnumerable<AbstractItemTemplate> IterateCashBundleItem(string imgPath)
        {
            try
            {
                var rootNode = LoadRootNode(imgPath);
                if (!int.TryParse(rootNode.Name.AsSpan(0, 4), out var groupId))
                    throw new TemplateFormatException("Item.wz", imgPath);

                List<AbstractItemTemplate> all = [];
                foreach (var itemNode in rootNode.Children)
                {
                    if (int.TryParse(itemNode.Name, out var itemId))
                    {
                        var template = ProcessCashItemByGroup(groupId, itemId, itemNode);
                        InsertItem(template);
                        all.Add(template);
                    }
                }
                return all;
            }
            catch (Exception ex)
            {
                LibLog.Logger.LogError(ex.ToString());
                return [];
            }
        }
    }
}
