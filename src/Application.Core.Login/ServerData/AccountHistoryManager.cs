using Application.Core.EF.Entities;
using Application.Core.Login.Models.Accounts;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Login;
using Application.Utility;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using SystemProto;

namespace Application.Core.Login.ServerData
{
    public class AccountHistoryManager : StorageBase<int, AccountHistoryModel>
    {
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly IMapper _mapper;
        readonly MasterServer _server;

        int _localId = 0;

        public AccountHistoryManager(IDbContextFactory<DBContext> dbContextFactory, IMapper mapper, MasterServer server)
        {
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
            _server = server;
        }

        public override async Task InitializeAsync(DBContext dbContext)
        {
            _localId = await dbContext.AccountBindings.MaxAsync(x => (int?)x.Id) ?? 0;
        }

        public void InsertLoginHistory(int accId, string ip, string mac, string hwid)
        {
            var model = new AccountHistoryModel()
            {
                Id = Interlocked.Increment(ref _localId),
                AccountId = accId,
                HWID = hwid,
                IP = ip,
                MAC = mac,
                LastActiveTime = _server.GetCurrentTimeDateTimeOffset()
            };
            SetDirty(model.Id, new StoreUnit<AccountHistoryModel>(StoreFlag.AddOrUpdate, model));
        }

        public override List<AccountHistoryModel> Query(Expression<Func<AccountHistoryModel, bool>> expression)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var dataFromDB = dbContext.AccountBindings.AsNoTracking().ProjectToType<AccountHistoryModel>().Where(expression).ToList();

            return QueryWithDirty(dataFromDB, expression.Compile());
        }

        protected override async Task CommitInternal(DBContext dbContext, Dictionary<int, StoreUnit<AccountHistoryModel>> updateData)
        {
            var updateKeys = updateData.Keys.ToArray();
            await dbContext.AccountBindings.Where(x => updateKeys.Contains(x.Id)).ExecuteDeleteAsync();

            foreach (var item in updateData.Values)
            {
                var obj = item.Data;
                if (item.Flag == StoreFlag.AddOrUpdate && obj != null)
                {
                    var dbData = new AccountBindingsEntity(obj.Id, obj.AccountId, obj.IP, obj.MAC, obj.HWID, obj.LastActiveTime);
                    dbContext.AccountBindings.Add(dbData);
                }
            }

            await dbContext.SaveChangesAsync();
        }

    }


    public class AccountBanManager : StorageBase<int, AccountBanModel>
    {
        readonly IDbContextFactory<DBContext> _dbContextFactory;
        readonly IMapper _mapper;
        readonly MasterServer _server;

        int _localId = 0;

        List<IpbanEntity> bannedIP = new();
        List<MacbanEntity> bannedMAC = new();
        List<HwidbanEntity> bannedHWID = new();

        public AccountBanManager(IDbContextFactory<DBContext> dbContextFactory, IMapper mapper, MasterServer server)
        {
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
            _server = server;
        }

        public override async Task InitializeAsync(DBContext dbContext)
        {
            _localId = await dbContext.AccountBans.MaxAsync(x => (int?)x.Id) ?? 0;

            bannedIP = await dbContext.Ipbans.AsNoTracking().ToListAsync();
            bannedMAC = await dbContext.Macbans.AsNoTracking().ToListAsync();
            bannedHWID = await dbContext.Hwidbans.AsNoTracking().ToListAsync();
        }

        public bool IsIPBlocked(string ip)
        {
            return bannedIP.Any(x => x.Ip == ip);
        }

        public bool IsMACBlocked(string mac)
        {
            return bannedMAC.Any(x => x.Mac == mac);
        }

        public bool IsHWIDBlocked(string hwid)
        {
            return bannedHWID.Any(x => x.Hwid == hwid);
        }

        public AccountBanModel? GetAccountBanInfo(int accountId)
        {
            return Query(x => x.AccountId == accountId && x.EndTime >= _server.GetCurrentTimeDateTimeOffset()).FirstOrDefault();
        }

        public bool BanAccount(int accountId, DateTimeOffset endTime, int level, int reason, string reasonDesc)
        {
            var banModel = Query(x => x.AccountId == accountId && x.EndTime >= _server.GetCurrentTimeDateTimeOffset()).FirstOrDefault();
            if (banModel != null)
                return false;

            var banLevel = (BanLevel)level;
            banModel = new AccountBanModel
            {
                Id = Interlocked.Increment(ref _localId),
                AccountId = accountId,
                BanLevel = banLevel,
                StartTime = _server.GetCurrentTimeDateTimeOffset(),
                EndTime = endTime,
                Reason = reason,
                ReasonDescription = reasonDesc
            };

            SetDirty(banModel.Id, new StoreUnit<AccountBanModel>(StoreFlag.AddOrUpdate, banModel));

            bannedIP.RemoveAll(x => x.Aid == accountId);
            bannedHWID.RemoveAll(x => x.AccountId == accountId);
            bannedMAC.RemoveAll(x => x.Aid == accountId);

            var dayBeforeMonth = _server.GetCurrentTimeDateTimeOffset().AddDays(30);
            var histories = _server.AccountHistoryManager.Query(x => x.AccountId == accountId && x.LastActiveTime >= dayBeforeMonth);
            foreach (var his in histories)
            {
                if (banLevel.HasFlag(BanLevel.IP))
                {
                    bannedIP.Add(new IpbanEntity(his.IP, accountId));
                }
                if (banLevel.HasFlag(BanLevel.Mac))
                {
                    foreach (var mac in his.MAC.Split(','))
                    {
                        bannedMAC.Add(new MacbanEntity(mac.Trim(), accountId));
                    }
                }
                if (banLevel.HasFlag(BanLevel.Hwid))
                {
                    bannedHWID.Add(new HwidbanEntity(his.HWID, accountId));
                }
            }

            return true;
        }

        public bool UnbanAccount(int accountId)
        {
            var banModel = Query(x => x.AccountId == accountId && x.EndTime >= _server.GetCurrentTimeDateTimeOffset()).FirstOrDefault();
            if (banModel == null)
                return false;

            SetRemoved(banModel.Id);

            bannedIP.RemoveAll(x => x.Aid == accountId);
            bannedHWID.RemoveAll(x => x.AccountId == accountId);
            bannedMAC.RemoveAll(x => x.Aid == accountId);

            return true;
        }


        public UnbanResponse Unban(UnbanRequest request)
        {
            var targetChr = _server.CharacterManager.FindPlayerByName(request.Victim);
            if (targetChr == null)
                return new UnbanResponse { Code = 1 };

            if (!UnbanAccount(targetChr.Character.AccountId))
            {
                // 已经处于封禁状态了
                return new UnbanResponse { Code = 2 };
            }


            return new UnbanResponse();
        }

        public BanResponse Ban(BanRequest request)
        {
            var targetChr = _server.CharacterManager.FindPlayerByName(request.Victim);
            if (targetChr == null)
                return new BanResponse { Code = 1 };

            if (!BanAccount(targetChr.Character.AccountId,
                request.Days < 0 ? DateTimeOffset.MaxValue : _server.GetCurrentTimeDateTimeOffset().AddDays(request.Days),
                request.BanLevel,
                request.Reason,
                request.ReasonDesc))
            {
                // 已经处于封禁状态了
                return new BanResponse { Code = 2 };
            }

            var data = new BanBroadcast
            {
                TargetId = targetChr.Character.Id,
                TargetName = targetChr.Character.Name,
                OperatorName = _server.CharacterManager.GetPlayerName(request.OperatorId),
                Reason = request.Reason,
                ReasonDesc = request.ReasonDesc
            };
            _server.Transport.BroadcastBanned(data);

            return new BanResponse();
        }

        public List<int> GetBannedAccounts()
        {
            return Query(x => x.EndTime <= _server.GetCurrentTimeDateTimeOffset()).Select(x => x.AccountId).ToList();
        }

        public override List<AccountBanModel> Query(Expression<Func<AccountBanModel, bool>> expression)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var dataFromDB = dbContext.AccountBans.AsNoTracking().ProjectToType<AccountBanModel>().Where(expression).ToList();

            return QueryWithDirty(dataFromDB, expression.Compile());
        }

        protected override async Task CommitInternal(DBContext dbContext, Dictionary<int, StoreUnit<AccountBanModel>> updateData)
        {
            var updateKeys = updateData.Keys.ToArray();
            await dbContext.AccountBans.Where(x => updateKeys.Contains(x.Id)).ExecuteDeleteAsync();

            foreach (var item in updateData.Values)
            {
                var obj = item.Data;
                if (item.Flag == StoreFlag.AddOrUpdate && obj != null)
                {
                    var dbData = new AccountBanEntity(obj.Id, obj.AccountId, obj.StartTime, obj.EndTime, (int)obj.BanLevel, obj.Reason, obj.ReasonDescription);
                    dbContext.AccountBans.Add(dbData);
                }
            }

            await dbContext.Ipbans.ExecuteDeleteAsync();
            await dbContext.Macbans.ExecuteDeleteAsync();
            await dbContext.Hwidbans.ExecuteDeleteAsync();
            dbContext.Ipbans.AddRange(bannedIP);
            dbContext.Macbans.AddRange(bannedMAC);
            dbContext.Hwidbans.AddRange(bannedHWID);

            await dbContext.SaveChangesAsync();
        }

    }
}
