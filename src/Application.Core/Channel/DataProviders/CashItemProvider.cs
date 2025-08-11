using Application.Core.Channel.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZLinq;

namespace Application.Core.Channel.DataProviders
{
    public class CashItemProvider : DataBootstrap
    {
        volatile Dictionary<int, CashItem> items = new();
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
            DataProvider etc = DataProviderFactory.getDataProvider(WZFiles.ETC);

            Dictionary<int, CashItem> loadedItems = new();
            var itemsRes = etc.getData("Commodity.img").getChildren();
            foreach (Data item in itemsRes)
            {
                int sn = DataTool.getIntConvert("SN", item);
                int itemId = DataTool.getIntConvert("ItemId", item);
                int price = DataTool.getIntConvert("Price", item, 0);
                long period = DataTool.getIntConvert("Period", item, 1);
                short count = (short)DataTool.getIntConvert("Count", item, 1);
                bool onSale = DataTool.getIntConvert("OnSale", item, 0) == 1;
                loadedItems.AddOrUpdate(sn, new CashItem(sn, itemId, price, period, count, onSale));
            }
            items = loadedItems;

            Dictionary<int, List<int>> loadedPackages = new();
            foreach (Data cashPackage in etc.getData("CashPackage.img").getChildren())
            {
                List<int> cPackage = new();

                foreach (Data item in cashPackage.getChildByPath("SN").getChildren())
                {
                    cPackage.Add(int.Parse(item.getData().ToString()));
                }

                loadedPackages.AddOrUpdate(int.Parse(cashPackage.getName()), cPackage);
            }
            packages = loadedPackages;

            try
            {

                specialcashitems = _lazyItemService.Value.GetSpecialCashItems();

            }
            catch (Exception ex)
            {
                LogFactory.GetLogger(LogType.ItemData).Error(ex.ToString());
            }
        }

        public CashItem? getRandomCashItem()
        {
            if (items.Count == 0)
            {
                return null;
            }

            var list = items.Values.AsValueEnumerable().Where(x => x.isOnSale() && !ItemId.isCashPackage(x.getItemId())).ToList();
            return Randomizer.Select(list);
        }

        public CashItem? getItem(int sn)
        {
            return items.GetValueOrDefault(sn);
        }

        public CashItem GetItemTrust(int sn) => getItem(sn) ?? throw new BusinessResException($"getItem({sn})");

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
