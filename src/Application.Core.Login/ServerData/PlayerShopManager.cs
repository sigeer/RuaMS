using Application.Core.Login.Datas;
using Application.Core.Login.Models;
using Application.Core.Login.Models.Items;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Items;
using Application.Utility;
using Application.Utility.Configs;
using ItemProto;
using Mapster;
using MapsterMapper;
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
        ConcurrentDictionary<int, PlayerShopRegistry> _playerShopData = new();
        /// <summary>
        /// 正在营业的雇佣商店
        /// </summary>

        ConcurrentDictionary<int, PlayerShopRegistry> _hiredMerchantData = new();

        int _localId = 0;
        public PlayerShopManager(IMapper mapper, MasterServer server, IDbContextFactory<DBContext> dbContextFactory)
        {
            _mapper = mapper;
            _server = server;
            _dbContextFactory = dbContextFactory;
        }

        public override Task InitializeAsync(DBContext dbContext)
        {
            return Task.CompletedTask;
        }

        public override List<FredrickStoreModel> Query(Expression<Func<FredrickStoreModel, bool>> expression)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var dataFromDB = dbContext.Fredstorages.AsNoTracking().ProjectToType<FredrickStoreModel>().Where(expression).ToList();

            foreach (var item in dataFromDB)
            {
                item.Items = _server.InventoryManager.LoadItems(dbContext, ItemFactory.MERCHANT.IsAccount, item.Id, ItemType.Merchant).ToArray();
            }

            return QueryWithDirty(dataFromDB, expression.Compile());
        }


        private void Store(PlayerShopRegistry hm)
        {
            var item = Query(x => x.Cid == hm.Id).FirstOrDefault();
            if (item == null)
            {
                item = new FredrickStoreModel
                {
                    Id = Interlocked.Increment(ref _localId),
                    Cid = hm.Id,
                };
            }
            item.Meso = hm.Meso;
            item.Items = _mapper.Map<ItemModel[]>(hm.Items);
            item.ItemMeso = hm.Items.Sum(x => (long)x.Price * x.Bundles);
            item.Daynotes = 0;
            item.UpdateTime = _server.getCurrentTime();

            SetDirty(item.Id, new StoreUnit<FredrickStoreModel>(StoreFlag.AddOrUpdate, item));
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
                var data = new PlayerShopRegistry();
                data.Channel = request.Channel;
                data.MapId = request.MapId;
                data.Id = request.OwnerId;
                data.Items = _mapper.Map<List<PlayerShopItemModel>>(request.Items);
                data.Meso = request.Meso;
                data.Title = request.Title;
                data.Type = (PlayerShopType)request.Type;
                data.MapObjectId = request.MapObjectId;

                if (shopType == PlayerShopType.HiredMerchant)
                    _hiredMerchantData[data.Id] = data;
                else
                    _playerShopData[data.Id] = data;
            }

        }

        public ItemProto.SearchHiredMerchantChannelResponse FindHiredMerchantChannel(ItemProto.SearchHiredMerchantChannelRequest request)
        {
            if (_hiredMerchantData.TryGetValue(request.MasterId, out var hm))
            {
                return new SearchHiredMerchantChannelResponse { Channel = hm.Channel };
            }
            return new SearchHiredMerchantChannelResponse();
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
                    res.Meso = store.GetMerchantNetMeso(_server.getCurrentTime());
                    res.Items.AddRange(_mapper.Map<Dto.ItemDto[]>(store.Items));
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
            await dbContext.Fredstorages.Where(x => updateData.Keys.Contains(x.Id)).ExecuteDeleteAsync();
            foreach (var item in updateData)
            {
                if (item.Value.Flag == StoreFlag.AddOrUpdate)
                {
                    var obj = item.Value.Data!;

                    await InventoryManager.CommitInventoryByTypeAsync(dbContext, item.Key, obj.Items, ItemFactory.MERCHANT);
                    dbContext.Fredstorages.Add(new FredstorageEntity(obj.Id, obj.Cid, obj.Daynotes, obj.Meso, DateTimeOffset.FromUnixTimeMilliseconds(obj.UpdateTime)));
                }
                if (item.Value.Flag == StoreFlag.Remove)
                {
                    await InventoryManager.CommitInventoryByTypeAsync(dbContext, item.Key, [], ItemFactory.MERCHANT);
                }
            }
            await dbContext.SaveChangesAsync();
        }

        private static int[] dailyReminders = new int[] { 2, 5, 10, 15, 30, 60, 90, int.MaxValue };
        public void RunFredrickSchedule()
        {
            var allData = Query(x => true);
            List<int> expiredCids = [];
            allData.ForEach(x =>
            {
                int daynotes = Math.Min(dailyReminders.Length - 1, x.Daynotes);

                int elapsedDays = TimeUtils.DayDiff(x.UpdateTime, _server.getCurrentTime());
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

                        int inactivityDays = TimeUtils.DayDiff(x.UpdateTime, _server.getCurrentTime());

                        if (inactivityDays < 7 || daynotes >= dailyReminders.Length - 1)
                        {
                            x.Daynotes = daynotes;
                            SetDirty(x.Id, new StoreUnit<FredrickStoreModel>(StoreFlag.AddOrUpdate, x));

                            string msg = fredrickReminderMessage(x.Daynotes - 1);
                            _server.NoteManager.SendNormal(msg, -NpcId.FREDRICK, x.Id);
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

        public CanHiredMerchantResponse CanHiredMerchant(CanHiredMerchantRequest request)
        {
            PlayerHiredMerchantStatus final = PlayerHiredMerchantStatus.Available;

            if (_hiredMerchantData.TryGetValue(request.MasterId, out var hm))
            {
                final = PlayerHiredMerchantStatus.Unavailable_Opening;
            }
            else
            {
                var store = Query(x => x.Cid == request.MasterId).FirstOrDefault();
                if (store != null && (store.GetMerchantNetMeso(_server.getCurrentTime()) > 0 || store.Items.Length > 0))
                {
                    final = PlayerHiredMerchantStatus.Unavailable_NeedRetrieve;
                }
            }

            return new CanHiredMerchantResponse { Code = (int)final };
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
                    OwnerName = _server.CharacterManager.GetPlayerName(x.Id),
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
