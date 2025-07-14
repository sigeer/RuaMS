using Application.Shared.Items;
using Application.Utility.Configs;
using Application.Utility.Extensions;
using AutoMapper;
using CashProto;
using constants.game;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.ServerData
{
    public class CashShopDataManager
    {
        private Dictionary<int, int> owlSearched = new();
        private List<Dictionary<int, int>> cashItemBought = new(9);
        private ReaderWriterLockSlim suggestLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        readonly MasterServer _server;
        ILogger<CashShopDataManager> _logger;
        readonly IMapper _mapper;

        public CashShopDataManager(MasterServer server, ILogger<CashShopDataManager> logger, IMapper mapper)
        {
            _server = server;
            _logger = logger;
            _mapper = mapper;

            for (int i = 0; i < 9; i++)
            {
                cashItemBought.Add(new());
            }

        }

        public void BuyCashItem(BuyCashItemRequest request)
        {
            CreateGiftResponse? giftResult = null;
            if (request.GiftInfo != null)
            {
                giftResult = _server.GiftManager.CreateGift(
                    request.MasterId, request.GiftInfo.Recipient, request.CashItemSn, request.CashItemId, request.GiftInfo.Message, request.GiftInfo.CreateRing);
                if (giftResult.Code != 0)
                {
                    _server.Transport.ReturnBuyCashItem(new BuyCashItemResponse { 
                        Code = giftResult.Code, 
                        GiftInfo = giftResult,
                        MasterId = request.MasterId,
                        Sn = request.CashItemSn,
                        Transaction = _server.ItemTransactionManager.CreateTransaction(request.Transaction, Application.Shared.Items.ItemTransactionStatus.PendingForRollback) });
                    return;
                }
            }

            if (!YamlConfig.config.server.USE_ENFORCE_ITEM_SUGGESTION)
            {
                suggestLock.EnterWriteLock();
                try
                {
                    Dictionary<int, int> tabItemBought = cashItemBought[request.CashItemSn / 10000000];

                    var cur = tabItemBought.GetValueOrDefault(request.CashItemSn);
                    tabItemBought.AddOrUpdate(request.CashItemSn, cur + 1);
                }
                finally
                {
                    suggestLock.ExitWriteLock();
                }
            }

            _server.Transport.ReturnBuyCashItem(new BuyCashItemResponse
            {
                Code = 0,
                GiftInfo = giftResult,
                MasterId = request.MasterId,
                Sn = request.CashItemSn,
                Transaction = _server.ItemTransactionManager.CreateTransaction(request.Transaction, Application.Shared.Items.ItemTransactionStatus.PendingForCommit)
            });
        }

        public void AddOwlItemSearch(int itemid)
        {
            suggestLock.EnterWriteLock();
            try
            {
                var cur = owlSearched.GetValueOrDefault(itemid);
                owlSearched.AddOrUpdate(itemid, cur + 1);
            }
            finally
            {
                suggestLock.ExitWriteLock();
            }
        }

        public List<Dto.OwlSearchRecordDto> GetOwlSearchedItems()
        {
            if (YamlConfig.config.server.USE_ENFORCE_ITEM_SUGGESTION)
            {
                return new(0);
            }

            suggestLock.EnterReadLock();
            try
            {
                return owlSearched.Select(x => new Dto.OwlSearchRecordDto() { ItemId = x.Key, Count = x.Value }).ToList();
            }
            finally
            {
                suggestLock.ExitReadLock();
            }
        }

        private List<List<KeyValuePair<int, int>>> GetBoughtCashItems()
        {
            if (YamlConfig.config.server.USE_ENFORCE_ITEM_SUGGESTION)
            {
                List<List<KeyValuePair<int, int>>> boughtCounts = new(9);

                // thanks GabrielSin for pointing out an issue here
                for (int i = 0; i < 9; i++)
                {
                    List<KeyValuePair<int, int>> tabCounts = new(0);
                    boughtCounts.Add(tabCounts);
                }

                return boughtCounts;
            }

            suggestLock.EnterReadLock();
            try
            {
                List<List<KeyValuePair<int, int>>> boughtCounts = new(cashItemBought.Count);

                foreach (Dictionary<int, int> tab in cashItemBought)
                {
                    List<KeyValuePair<int, int>> tabItems = new();
                    boughtCounts.Add(tabItems);

                    foreach (var e in tab)
                    {
                        tabItems.Add(new(e.Key, e.Value));
                    }
                }

                return boughtCounts;
            }
            finally
            {
                suggestLock.ExitReadLock();
            }
        }

        private List<int> GetMostSellerOnTab(List<KeyValuePair<int, int>> tabSellers)
        {
            return tabSellers.OrderByDescending(x => x.Value).Select(x => x.Key).Take(5).ToList();
        }

        public List<List<int>> GetMostSellerCashItems()
        {
            List<List<KeyValuePair<int, int>>> mostSellers = GetBoughtCashItems();
            List<List<int>> cashLeaderboards = new(9);
            List<int> tabLeaderboards;
            List<int> allLeaderboards = new List<int>();

            foreach (var tabSellers in mostSellers)
            {
                if (tabSellers.Count < 5)
                {
                    if (allLeaderboards == null)
                    {
                        List<KeyValuePair<int, int>> allSellers = new();
                        foreach (var tabItems in mostSellers)
                        {
                            allSellers.AddRange(tabItems);
                        }

                        allLeaderboards = GetMostSellerOnTab(allSellers);
                    }

                    tabLeaderboards = new();
                    if (allLeaderboards.Count < 5)
                    {
                        foreach (int i in GameConstants.CASH_DATA)
                        {
                            tabLeaderboards.Add(i);
                        }
                    }
                    else
                    {
                        tabLeaderboards.AddRange(allLeaderboards);
                    }
                }
                else
                {
                    tabLeaderboards = GetMostSellerOnTab(tabSellers);
                }

                cashLeaderboards.Add(tabLeaderboards);
            }

            return cashLeaderboards;
        }

    }
}
