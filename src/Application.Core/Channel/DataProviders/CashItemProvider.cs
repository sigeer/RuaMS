using Application.Core.Channel.Services;
using Application.Templates.Etc;
using Application.Templates.Reader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZLinq;

namespace Application.Core.Channel.DataProviders
{
    public class CashItemProvider : DataBootstrap
    {
        volatile Dictionary<int, CashCommodityTemplate> items = new();
        volatile Dictionary<int, List<int>> packages = new();
        volatile List<SpecialCashItem> specialcashitems = new();

        readonly Lazy<ItemService> _lazyItemService;

        public CashItemProvider(IServiceProvider sp, ILogger<DataBootstrap> logger) : base(logger)
        {
            Name = "现金道具";
            _lazyItemService = new Lazy<ItemService>(() => sp.GetRequiredService<ItemService>());
        }

        protected override void LoadDataInternal()
        {
            items = ProviderSource.Instance.GetProvider<IProvider<CashCommodityTemplate>>(ProviderType.EtcCashCommodity).LoadAll()
                .ToDictionary(x => x.CashItemSN);

            packages = ProviderSource.Instance.GetProvider<IProvider<CashPackageTemplate>>(ProviderType.EtcCashPackage).LoadAll()
                .ToDictionary(x => x.TemplateId, x => x.SNList.ToList());

            specialcashitems = _lazyItemService.Value.GetSpecialCashItems();
        }

        public CashCommodityTemplate? getRandomCashItem()
        {
            if (items.Count == 0)
            {
                return null;
            }

            var list = items.Values.AsValueEnumerable().Where(x => x.OnSale && !ItemId.isCashPackage(x.ItemID)).ToList();
            return Randomizer.Select(list);
        }

        public CashCommodityTemplate? getItem(int sn)
        {
            return items.GetValueOrDefault(sn);
        }

        public CashCommodityTemplate GetItemTrust(int sn) => getItem(sn) ?? throw new BusinessResException($"getItem({sn})");

        public bool isPackage(int itemId)
        {
            return packages.ContainsKey(itemId);
        }
        public List<int> GetPackage(int itemId)
        {
            return packages.GetValueOrDefault(itemId) ?? [];
        }

        public List<SpecialCashItem> getSpecialCashItems()
        {
            return specialcashitems;
        }
    }

}
