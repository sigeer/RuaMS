using Application.Shared.Constants;
using Application.Shared.Items;
using Application.Utility.Configs;
using Application.Utility.Extensions;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.ServerData
{
    public class CashShopDataManager
    {

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

        public ProtoService.BuyCashItemResponse BuyCashItem(ProtoService.BuyCashItemRequest request)
        {
            var chr = _server.CharacterManager.FindPlayerById(request.MasterId);
            var accountGame = chr?.Character == null
                ? null
                : _server.AccountGameManager.GetAccountGameData(chr.Character.AccountId);

            // 点数余额在中心服，扣费也必须在这里做：退出商城时频道那份会被丢弃，
            // 只在频道扣钱等于没扣
            if (request.Price > 0 && request.CashType > 0)
            {
                if (accountGame == null || !TryPay(accountGame, request.CashType, -request.Price))
                {
                    return new ProtoService.BuyCashItemResponse
                    {
                        Code = CashShopMessage.NotEnoughCash,
                        MasterId = request.MasterId,
                        Sn = request.CashItemSn,
                    };
                }
            }

            ProtoService.CreateGiftResponse? giftResult = null;
            if (request.GiftInfo != null)
            {
                giftResult = _server.GiftManager.CreateGift(
                    request.MasterId, request.GiftInfo.Recipient, request.CashItemSn, request.CashItemId, request.GiftInfo.Message, request.GiftInfo.CreateRing);
                if (giftResult.Code != 0)
                {
                    // 送礼失败要把钱退回去
                    if (request.Price > 0 && accountGame != null)
                    {
                        TryPay(accountGame, request.CashType, request.Price);
                    }
                    return new ProtoService.BuyCashItemResponse { Code = giftResult.Code };
                }
            }

            // 买到的物品只记「轻量条目」：中心服没有 WZ 模板，不生成真物品，
            // 只存 SN（+物品id/数量/有效期），玩家回主世界时由频道按 SN 补全成真道具。
            // 商城期间以中心服为准、退出时频道那份不保存，所以记录必须落在这里。
            //
            // 送礼时不落送礼人的现金仓：物品由 GiftManager 记到收礼人头上。
            // 唯一的例外是戒指——送礼人会拿回自己那一份，它必须跟着戒指 id 一起落仓，
            // 所以也在这里生成（request.Items 里频道已经放好了它的商品信息）。
            var created = new List<ProtoService.CashItemPlaceholderProto>();
            var placeToCash = request.GiftInfo == null || request.GiftInfo.CreateRing;
            if (placeToCash && accountGame != null && chr?.Character != null)
            {
                // 戒指那份的 cashId 必须是戒指 id，客户端之后就是拿它去取的
                long forcedCashId = 0;
                if (request.GiftInfo?.CreateRing == true && giftResult?.RingSource != null)
                {
                    forcedCashId = giftResult.RingSource.CharacterId1 == request.MasterId
                        ? giftResult.RingSource.RingId1
                        : giftResult.RingSource.RingId2;
                }

                var cashInv = CashShopStorage.GetCashInventory(chr.Character, accountGame);
                if (cashInv != null)
                {
                    foreach (var entry in request.Items)
                    {
                        // 客户端认的 cashId 就是 UniqueId：频道没指定时由这里补，并原样回给频道。
                        // 戒指那份必须用戒指 id，否则客户端取出来时找不到对应的戒指
                        var cashId = forcedCashId > 0
                            ? forcedCashId
                            : entry.CashId > 0 ? entry.CashId : CashShopStorage.NextCashUniqueId();
                        var quantity = entry.Quantity <= 0 ? 1 : entry.Quantity;

                        cashInv.Add(new ProtoModel.ItemProto
                        {
                            UniqueId = cashId,
                            Itemid = entry.ItemId,
                            Quantity = quantity,
                            Sn = entry.Sn,
                            Expiration = entry.Expiration,
                        });

                        // 回给频道的必须和刚写进去的一致，否则「现金仓 → 背包」会找不到
                        created.Add(new ProtoService.CashItemPlaceholderProto
                        {
                            Sn = entry.Sn,
                            ItemId = entry.ItemId,
                            Quantity = quantity,
                            Expiration = entry.Expiration,
                            CashId = cashId,
                        });
                    }

                    _server.AccountGameManager.UpdateAccountGame(accountGame);
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

            var response = new ProtoService.BuyCashItemResponse
            {
                Code = 0,
                GiftInfo = giftResult,
                MasterId = request.MasterId,
                Sn = request.CashItemSn,
                Cash = new ProtoModel.PlayerCashProto
                {
                    Meso = chr?.Character?.Meso ?? 0,
                    NxCredit = accountGame?.NxCredit ?? 0,
                    MaplePoint = accountGame?.MaplePoint ?? 0,
                    NxPrepaid = accountGame?.NxPrepaid ?? 0,
                },
            };
            response.Created.AddRange(created);
            return response;
        }

        /// <summary>
        /// 增减点券 / 抵用券 / 信用卡，余额不能为负、不能超过上限
        /// </summary>
        static bool TryPay(ProtoModel.AccountGameProto accountGame, int cashType, int delta)
        {
            switch (cashType)
            {
                case CashType.NX_CREDIT:
                    if (accountGame.NxCredit + delta < 0 || accountGame.NxCredit + delta > Limits.MaxCash)
                        return false;
                    accountGame.NxCredit += delta;
                    return true;
                case CashType.MAPLE_POINT:
                    if (accountGame.MaplePoint + delta < 0 || accountGame.MaplePoint + delta > Limits.MaxCash)
                        return false;
                    accountGame.MaplePoint += delta;
                    return true;
                case CashType.NX_PREPAID:
                    if (accountGame.NxPrepaid + delta < 0 || accountGame.NxPrepaid + delta > Limits.MaxCash)
                        return false;
                    accountGame.NxPrepaid += delta;
                    return true;
                default:
                    return false;
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
