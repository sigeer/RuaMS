using Application.Core.EF.Entities.Items;
using Application.Core.Login.Models;
using Application.Core.Login.Models.Items;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Items;
using Application.Utility;
using AutoMapper;
using client.inventory;
using ItemProto;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Application.Core.Login.ServerData
{
    /// <summary>
    /// 仅存在数据库里的必定是未开启的
    /// </summary>
    public class PlayerShopManager : StorageBase<int, PlayerShopRegistry>
    {
        readonly IMapper _mapper;
        readonly MasterServer _server;
        readonly IDbContextFactory<DBContext> _dbContextFactory;

        /// <summary>
        /// 个人商店不需要存数据库
        /// </summary>
        ConcurrentDictionary<int, PlayerShopRegistry> _playerShopData = new();

        ConcurrentDictionary<int, PlayerShopRegistry> _hiredMerchantData = new();

        public PlayerShopManager(IMapper mapper, MasterServer server, IDbContextFactory<DBContext> dbContextFactory)
        {
            _mapper = mapper;
            _server = server;
            _dbContextFactory = dbContextFactory;
        }

        public override List<PlayerShopRegistry> Query(Expression<Func<PlayerShopRegistry, bool>> expression)
        {
            return [];
        }

        public void SyncPlayerStorage(ItemProto.SyncPlayerShopRequest request)
        {
            var data = new PlayerShopRegistry();
            data.Channel = request.Channel;
            data.MapId = request.MapId;
            data.Id = request.OwnerId;

            data.Daynotes = 0;
            data.UpdateTime = _server.getCurrentTime();

            var operation = (SyncPlayerShopOperation)request.Operation;
            if (operation == SyncPlayerShopOperation.Close)
            {
                data.Channel = 0;
                data.MapId = 0;
            }

            else if (operation == SyncPlayerShopOperation.UpdateByTrade)
            {
                // 交易通知
                // _server.Transport.SendHiredMerchantSellNotify();
            }

            var shopType = (PlayerShopType)request.Type;
            if (shopType == PlayerShopType.HiredMerchant)
                SetDirty(data.Id, new Utility.StoreUnit<PlayerShopRegistry>(Utility.StoreFlag.AddOrUpdate, data));
            else
                _playerShopData[data.Id] = data;
        }

        public ItemProto.RemoteHiredMerchantDto GetPlayerHiredMerchant(ItemProto.GetPlayerHiredMerchantRequest request)
        {
            if (!_localData.TryGetValue(request.MasterId, out var data) || data.Data == null || data.Flag == StoreFlag.Remove)
            {
                var dbData = LoadMerchantItems(request.MasterId);

                var store = new ItemProto.RemoteHiredMerchantDto()
                {
                    Meso = dbData.FirstOrDefault(x => x.Itemid == 0)?.Accountid ?? 0,
                };
                store.Items.AddRange(_mapper.Map<Dto.ItemDto[]>(dbData.Where(x => x.Itemid != 0)));
                return store;
            }
            else
            {
                var store = new ItemProto.RemoteHiredMerchantDto
                {
                    Channel = data.Data.Channel,
                    MapId = data.Data.MapId,
                    Meso = data.Data.Meso,
                    OwnerId = data.Data.Id,
                    Title = data.Data.Title
                };
                store.Items.AddRange(_mapper.Map<Dto.ItemDto[]>(data.Data.Items));
                return store;
            }

        }

        public CommitRetrievedResponse CommitRetrieve(ItemProto.CommitRetrievedRequest request)
        {
            return new CommitRetrievedResponse() { IsSuccess = SetRemoved(request.OwnerId) };
        }

        public void ClearData(int chrId)
        {
            SetRemoved(chrId);
        }

        public int GetMerchantNetMeso(PlayerShopRegistry data)
        {
            int elapsedDays = TimeUtils.DayDiff(data.UpdateTime, _server.getCurrentTime());
            if (elapsedDays > 100)
            {
                elapsedDays = 100;
            }

            long netMeso = data.Meso; // negative mesos issues found thanks to Flash, Vcoc
            netMeso = (netMeso * (100 - elapsedDays)) / 100;
            return (int)netMeso;
        }

        protected override async Task CommitInternal(DBContext dbContext, Dictionary<int, StoreUnit<PlayerShopRegistry>> updateData)
        {
            await dbContext.Fredstorages.Where(x => updateData.Keys.Contains(x.Cid)).ExecuteDeleteAsync();
            foreach (var item in updateData)
            {
                if (item.Value.Flag == StoreFlag.AddOrUpdate)
                {
                    await CommitMerchantItemsAsync(dbContext, item.Key, item.Value.Data!.Meso, _mapper.Map<PlayerShopItemModel[]>(item.Value.Data!.Items));
                    dbContext.Fredstorages.Add(new Fredstorage
                    {
                        Daynotes = item.Value.Data.Daynotes,
                        Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(item.Value.Data.UpdateTime),
                        Cid = item.Key
                    });
                }
                if (item.Value.Flag == StoreFlag.Remove)
                {
                    await CommitMerchantItemsAsync(dbContext, item.Key, 0, []);
                }
            }
            await dbContext.SaveChangesAsync();
        }

        private List<ItemModel> LoadMerchantItems(int id)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var value = ItemFactory.MERCHANT.getValue();
            var isAccount = ItemFactory.MERCHANT.IsAccount;

            var dataList = (from a in dbContext.Inventoryitems.AsNoTracking()
                                .Where(x => isAccount ? id == x.Accountid : id == x.Characterid!.Value)
                                .Where(x => x.Type == value)
                            join b in dbContext.Inventoryequipments on a.Inventoryitemid equals b.Inventoryitemid into bss
                            from bs in bss.DefaultIfEmpty()
                            join c in dbContext.Pets.AsNoTracking() on a.Petid equals c.Petid into css
                            from cs in css.DefaultIfEmpty()
                            select new ItemEntityPair(a, bs, cs)).ToList();

            var allItemIdList = dataList.Select(x => x.Item.Inventoryitemid).ToList();
            var filteredMerchantList = dbContext.Inventorymerchants.Where(y => allItemIdList.Contains(y.Inventoryitemid)).ToList();

            var ringIds = dataList.Where(x => x.Equip != null && x.Equip.RingId > 0).Select(x => x.Equip!.RingId).ToArray();
            var rings = _server.RingManager.LoadRings(ringIds);
            List<ItemModel> items = [];
            foreach (var item in dataList)
            {
                short bundles = (sbyte)filteredMerchantList.Where(y => y.Inventoryitemid == item.Item.Inventoryitemid).Select(y => y.Bundles).FirstOrDefault();

                var obj = _mapper.Map<ItemModel>(item);
                obj.Quantity = (short)(bundles * obj.Quantity);

                RingSourceModel? ring = null;
                if (item.Equip != null && item.Equip.RingId > 0)
                    ring = rings.FirstOrDefault(x => x.RingId1 == item.Equip.RingId || x.RingId2 == item.Equip.RingId);
                if (obj.EquipInfo != null)
                    obj.EquipInfo.RingSourceInfo = ring;

                items.Add(obj);
            }
            return items;
        }

        private async Task CommitMerchantItemsAsync(DBContext dbContext, int targetId, int meso, PlayerShopItemModel[] items)
        {
            var itemType = (byte)ItemFactory.MERCHANT.getValue();

            var allItems = dbContext.Inventoryitems.Where(x => x.Characterid == targetId && x.Type == itemType).ToList();
            if (allItems.Count != 0)
            {
                var itemIds = allItems.Select(x => x.Inventoryitemid).ToArray();

                var petIds = allItems.Select(x => x.Petid).ToArray();
                await dbContext.Inventoryitems.Where(x => itemIds.Contains(x.Inventoryitemid)).ExecuteDeleteAsync();
                await dbContext.Inventoryequipments.Where(x => itemIds.Contains(x.Inventoryitemid)).ExecuteDeleteAsync();
                await dbContext.Pets.Where(x => petIds.Contains(x.Petid)).ExecuteDeleteAsync();
            }

            if (meso > 0)
            {
                // quantity是short，暂时用accountid存放实际数量
                var mesoItem = new Inventoryitem()
                {
                    Itemid = 0,
                    Characterid = targetId,
                    Accountid = meso,
                    Type = itemType,
                };
                await dbContext.Inventoryitems.AddAsync(mesoItem);
            }

            foreach (var obj in items)
            {
                var item = obj.Item;
                var model = new Inventoryitem()
                {
                    Itemid = item.Itemid,
                    Characterid = targetId,
                    Expiration = item.Expiration,
                    Flag = item.Flag,
                    GiftFrom = item.GiftFrom,
                    Inventorytype = item.InventoryType,
                    Owner = item.Owner,
                    Petid = item.PetInfo == null ? -1 : item.PetInfo.Petid,
                    Position = item.Position,
                    Quantity = item.Quantity,
                    Type = itemType,
                };
                await dbContext.Inventoryitems.AddAsync(model);
                await dbContext.SaveChangesAsync();

                var merchantInfo = new Inventorymerchant()
                {
                    Bundles = obj.Bundles,
                    Inventoryitemid = model.Inventoryitemid,
                    Characterid = targetId
                };
                await dbContext.Inventorymerchants.AddAsync(merchantInfo);

                if (item.EquipInfo != null)
                {
                    dbContext.Inventoryequipments.Add(new Inventoryequipment(model.Inventoryitemid,
                        item.EquipInfo.Upgradeslots,
                        item.EquipInfo.Level,
                        item.EquipInfo.Str,
                        item.EquipInfo.Dex,
                        item.EquipInfo.Int,
                        item.EquipInfo.Luk,
                        item.EquipInfo.Hp,
                        item.EquipInfo.Mp,
                        item.EquipInfo.Watk,
                        item.EquipInfo.Matk,
                        item.EquipInfo.Wdef,
                        item.EquipInfo.Mdef,
                        item.EquipInfo.Acc,
                        item.EquipInfo.Avoid,
                        item.EquipInfo.Hands,
                        item.EquipInfo.Speed,
                        item.EquipInfo.Jump,
                        item.EquipInfo.Locked,
                        item.EquipInfo.Vicious,
                        item.EquipInfo.Itemlevel,
                        item.EquipInfo.Itemexp,
                        item.EquipInfo.RingId));
                }
                if (item.PetInfo != null)
                {
                    await dbContext.Pets.AddAsync(
                        new PetEntity(item.PetInfo.Petid, item.PetInfo.Name, item.PetInfo.Level, item.PetInfo.Closeness, item.PetInfo.Fullness, item.PetInfo.Summoned, item.PetInfo.Flag));
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
                    ClearData(x.Id);

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

            var data = Query(x => x.Id == request.MasterId).FirstOrDefault();
            if (data != null)
            {
                if (data.Channel > 0)
                    final = PlayerHiredMerchantStatus.Unavailable_Opening;
                else if (GetMerchantNetMeso(data) > 0 || data.Items.Count > 0)
                    final = PlayerHiredMerchantStatus.Unavailable_NeedRetrieve;
            }

            return new CanHiredMerchantResponse { Code = (int)final};
        }
    }
}
