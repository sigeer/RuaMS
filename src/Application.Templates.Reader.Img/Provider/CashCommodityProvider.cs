using Application.Templates.Etc;
using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Duey.Abstractions;
using Duey.Provider.WZ.Files;
using Microsoft.Extensions.Logging;

namespace Application.Templates.Reader.Img.Provider
{
    public sealed class CashCommodityProvider : AbstractAllProvider<CashCommodityTemplate>
    {
        public override ProviderType Type => ProviderType.EtcCashCommodity;

        public CashCommodityProvider(IWzPathResolver resolver, bool useCache = true)
            : base(resolver, useCache)
        {
        }

        protected override IEnumerable<CashCommodityTemplate> GetDataFromImg()
        {
            try
            {
                List<CashCommodityTemplate> all = [];
                foreach (var file in _resolver.ResolveGroup(Type))
                {
                    var fullPath = _resolver.ResolveFullPath(file);
                    var root = new WZImage(fullPath);
                    foreach (var item in root.Children)
                    {
                        if (int.TryParse(item.Name, out var idx))
                        {
                            var pEntry = new CashCommodityTemplate(idx)
                            {
                                TemplateId = item.GetIntValue("SN"),
                                CashItemSN = item.GetIntValue("SN"),
                                ItemID = item.GetIntValue("ItemId"),
                                Count = item.GetIntValue("Count", 1),
                                Price = item.GetIntValue("Price"),
                                Period = item.GetIntValue("Period", 1),
                                Priority = item.GetIntValue("Priority"),
                                Gender = item.GetIntValue("Gender"),
                                OnSale = item.ResolveBool("OnSale") ?? false,
                                Classification = item.GetIntValue("Class"),
                            };
                            InsertItem(pEntry);
                            all.Add(pEntry);
                        }
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
