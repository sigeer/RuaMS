using Application.Core.Login.Models.Items;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Items;
using Application.Utility;
using Application.Utility.Configs;
using ItemProto;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using ZLinq;

namespace Application.Core.Login.ServerData
{
    public class PlayerShopManager : StorageBase<int, FredrickStoreModel>
    {
        readonly IMapper _mapper;
        readonly MasterServer _server;
        readonly IDbContextFactory<DBContext> _dbContextFactory;

        /// <summary>
        /// 正在营业的个人商店
        /// </summary>
        ConcurrentDictionary<int, ItemProto.SyncPlayerShopRequest> _playerShopData = new();
        /// <summary>
        /// 正在营业的雇佣商店
        /// </summary>

        ConcurrentDictionary<int, ItemProto.SyncPlayerShopRequest> _hiredMerchantData = new();


        int _localId = 0;
        public PlayerShopManager(IMapper mapper, MasterServer server, IDbContextFactory<DBContext> dbContextFactory) : base(x => x.Id)
        {
            _mapper = mapper;
            _server = server;
            _dbContextFactory = dbContextFactory;
        }

        public override async Task InitializeAsync(DBContext dbContext)
        {
            _localId = (await dbContext.Fredstorages.Select(x => x.Id).DefaultIfEmpty().MaxAsync());
        }

        public override List<FredrickStoreModel> Query(Expression<Func<FredrickStoreModel, bool>> expression)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var dataFromDB = dbContext.Fredstorages.ProjectToType<FredrickStoreModel>().Where(expression).ToList();

            return QueryWithDirty(dataFromDB, expression.Compile());
        }


        private void Store(ItemProto.SyncPlayerShopRequest hm)
        {
            var item = Query(x => x.Cid == hm.OwnerId).FirstOrDefault();
            if (item == null)
            {
                item = new FredrickStoreModel
                {
                    Id = Interlocked.Increment(ref _localId),
                    Cid = hm.OwnerId,
                    StoreTime = _server.getCurrentTime()
                };
            }
            item.Meso = hm.Meso;
            item.Items.Items.AddRange(hm.Items);
            item.ItemMeso = hm.Items.Sum(x => x.Price * x.Bundles);
            item.Daynotes = 0;

            SetDirty(item);
        }

        public void SyncPlayerStorage(ItemProto.SyncPlayerShopRequest request)
        {
            var shopType = (PlayerShopType)request.Type;

            var operation = (SyncPlayerShopOperation)request.Operation;
            if (operation == SyncPlayerShopOperation.Close || operation == SyncPlayerShopOperation.CloseWithoutStore)
            {
                if (shopType == PlayerShopType.PlayerShop && _playerShopData.TryRemove(request.OwnerId, out var ps))
                {
                    if (operation != SyncPlayerShopOperation.CloseWithoutStore)
                        Store(ps);
                }
                else if (shopType == PlayerShopType.HiredMerchant && _hiredMerchantData.TryRemove(request.OwnerId, out var hm))
                {
                    if (operation != SyncPlayerShopOperation.CloseWithoutStore)
                        Store(hm);
                }
            }

            else if (operation == SyncPlayerShopOperation.UpdateByTrade)
            {
                // 交易通知
                //_server.Transport.SendHiredMerchantSellNotify(
                //    new ItemProto.NotifyItemPurchasedResponse 
                //    { 
                //        OwnerId = data.Id,
                //        ItemId = 
                //    });
            }

            else if (operation == SyncPlayerShopOperation.Update)
            {
                if (shopType == PlayerShopType.HiredMerchant)
                    _hiredMerchantData[request.OwnerId] = request;
                else
                    _playerShopData[request.OwnerId] = request;
            }

        }

        public ItemProto.RemoteHiredMerchantDto GetPlayerHiredMerchant(ItemProto.GetPlayerHiredMerchantRequest request)
        {
            var res = new ItemProto.RemoteHiredMerchantDto()
            {
                OwnerId = request.MasterId,
            };
            if (_hiredMerchantData.TryGetValue(request.MasterId, out var hm))
            {
                res.Title = hm.Title;
                res.MapId = hm.MapId;
                res.Channel = hm.Channel;
            }
            else
            {
                var store = Query(x => x.Cid == request.MasterId).FirstOrDefault();
                if (store != null)
                {
                    res.Meso = store.Meso;
                    res.FeePercentage = store.GetFeePercentage(_server.getCurrentTime());
                    res.FeeMeso = (store.Meso + store.ItemMeso) * res.FeePercentage;

                    res.Items.AddRange(store.Items.Items.Select(x => { var m = x.Item; m.Quantity = m.Quantity * x.Bundles; return m; }));
                }
            }
            return res;

        }

        public CommitRetrievedResponse CommitRetrieve(ItemProto.CommitRetrievedRequest request)
        {
            var obj = Query(x => x.Cid == request.OwnerId).FirstOrDefault();
            return new CommitRetrievedResponse() { IsSuccess = obj == null ? false : SetRemoved(obj.Id) };
        }


        protected override async Task CommitInternal(DBContext dbContext, Dictionary<int, StoreUnit<FredrickStoreModel>> updateData)
        {
            var updatePackages = updateData.Keys.ToArray();

            var allDbList = await dbContext.Fredstorages.Where(x => updatePackages.Contains(x.Id)).ToListAsync();
            foreach (var item in updateData)
            {
                var dbModel = allDbList.FirstOrDefault(x => x.Id == item.Key);
                if (item.Value.Flag == StoreFlag.Remove)
                {
                    if (dbModel != null)
                    {
                        dbContext.Fredstorages.Remove(dbModel);
                    }
                    continue;
                }

                if (item.Value.Data is null)
                    continue;

                if (dbModel == null)
                {
                    dbModel = _mapper.Map<FredstorageEntity>(item.Value.Data);
                    dbContext.Fredstorages.Add(dbModel);
                }
                else
                {
                    _mapper.Map(item.Value.Data, dbModel);
                }
            }
            await dbContext.SaveChangesAsync();
        }

        private static int[] dailyReminders = new int[] { 2, 5, 10, 15, 30, 60, 90, int.MaxValue };
        public void RunFredrickSchedule()
        {
            var allData = Query(x => true);
            List<int> expiredCids = [];
            allData.ForEach(async x =>
            {
                int daynotes = Math.Min(dailyReminders.Length - 1, x.Daynotes);

                int elapsedDays = TimeUtils.DayDiff(x.StoreTime, _server.getCurrentTime());
                if (elapsedDays > 100)
                {
                    SetRemoved(x.Id);

                    expiredCids.Add(x.Id);
                }
                else
                {
                    int notifDay = dailyReminders[daynotes];

                    if (elapsedDays >= notifDay)
                    {
                        do
                        {
                            daynotes++;
                            notifDay = dailyReminders[daynotes];
                        } while (elapsedDays >= notifDay);

                        int inactivityDays = TimeUtils.DayDiff(x.StoreTime, _server.getCurrentTime());

                        if (inactivityDays < 7 || daynotes >= dailyReminders.Length - 1)
                        {
                            x.Daynotes = daynotes;
                            SetDirty(x);

                            string msg = fredrickReminderMessage(x.Daynotes - 1);
                            await _server.NoteManager.SendNormal(msg, -NpcId.FREDRICK, x.Id);
                        }
                    }
                }
            });

            _server.NoteManager.removeFredrickReminders(expiredCids);
        }

        private static string fredrickReminderMessage(int daynotes)
        {
            string msg;

            if (daynotes < 4)
            {
                msg = "Hi customer! I am Fredrick, the Union Chief of the Hired Merchant Union. A reminder that " + dailyReminders[daynotes] + " days have passed since you used our service. Please reclaim your stored goods at FM Entrance.";
            }
            else
            {
                msg = "Hi customer! I am Fredrick, the Union Chief of the Hired Merchant Union. " + dailyReminders[daynotes] + " days have passed since you used our service. Consider claiming back the items before we move them away for refund.";
            }

            return msg;
        }

        public OwlSearchResponse OwlSearch(OwlSearchRequest request)
        {
            var res = new OwlSearchResponse();
            res.Items.AddRange(_hiredMerchantData.Values.Concat(_playerShopData.Values).AsValueEnumerable()
                .SelectMany(x => x.Items.Where(y => y.Item.Itemid == request.SearchItemId).Select(y => new ItemProto.OwlSearchResultItemDto
                {
                    MapObjectId = x.MapObjectId,
                    Channel = x.Channel,
                    MapId = x.MapId,
                    OwnerName = _server.CharacterManager.GetPlayerName(x.OwnerId),
                    Title = x.Title,
                    Item = _mapper.Map<ItemProto.PlayerShopItemDto>(y)
                })).OrderBy(x => x.Item.Price).Take(200).ToArray());

            return res;
        }
        private ConcurrentDictionary<int, int> owlSearched = new();
        public void AddOwlItemSearch(int itemid)
        {
            if (owlSearched.TryGetValue(itemid, out var d))
                owlSearched[itemid] = d + 1;
            else
                owlSearched[itemid] = 1;
        }

        public ItemProto.OwlSearchRecordResponse GetOwlSearchedItems()
        {
            if (YamlConfig.config.server.USE_ENFORCE_ITEM_SUGGESTION)
            {
                return new();
            }

            var res = new ItemProto.OwlSearchRecordResponse();
            res.Items.AddRange(owlSearched.Select(x => new ItemProto.OwlSearchRecordDto() { ItemId = x.Key, Count = x.Value }).ToList());
            return res;
        }

    }
}
