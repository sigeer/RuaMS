using Application.Core.EF.Entities;
using Application.Core.Login.Models.Accounts;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Login;
using Application.Utility;
using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using SystemProto;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Application.Shared.Message;
using Application.Resources.Messages;

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

        public AccountHistoryModel InsertAccountLoginHistory(int accId, string ip, string hwid)
        {
            var model = new AccountHistoryModel()
            {
                Id = Interlocked.Increment(ref _localId),
                AccountId = accId,
                HWID = hwid,
                IP = ip,
                LastActiveTime = _server.GetCurrentTimeDateTimeOffset()
            };
            SetDirty(model.Id, new StoreUnit<AccountHistoryModel>(StoreFlag.AddOrUpdate, model));
            return model;
        }

        public void AttachAccountMAC(int id, string mac)
        {
            var model = Query(x => x.Id == id).FirstOrDefault();
            if (model != null)
            {
                model.MAC = mac;
                SetDirty(model.Id, new StoreUnit<AccountHistoryModel>(StoreFlag.AddOrUpdate, model));
            }
        }

        public override List<AccountHistoryModel> Query(Expression<Func<AccountHistoryModel, bool>> expression)
        {
            var entityExpress = _mapper.MapExpression<Expression<Func<AccountBindingsEntity, bool>>>(expression);

            using var dbContext = _dbContextFactory.CreateDbContext();

            var dataFromDB = _mapper.Map<List<AccountHistoryModel>>(dbContext.AccountBindings.Where(entityExpress).ToList());

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


        public async Task Unban(UnbanRequest request)
        {
            var res = new UnbanResponse() { Request = request };
            var targetChr = _server.CharacterManager.FindPlayerByName(request.Victim);
            if (targetChr == null)
            {
                res.Code = 1;
            }

            else if (!UnbanAccount(targetChr.Character.AccountId))
            {
                res.Code = 2;
            }

            await _server.Transport.SendMessageN(ChannelRecvCode.Unban, res, [request.OperatorId]);
        }

        public async Task Ban(BanRequest request)
        {
            var res = new BanResponse { Request = request };
            var targetChr = _server.CharacterManager.FindPlayerByName(request.Victim);
            if (targetChr == null)
            {
                res.Code = 1;
                await _server.Transport.SendMessageN(ChannelRecvCode.BanPlayer, res, [request.OperatorId]);
                return;
            }

            if (!BanAccount(targetChr.Character.AccountId,
                request.Days < 0 ? DateTimeOffset.MaxValue : _server.GetCurrentTimeDateTimeOffset().AddDays(request.Days),
                request.BanLevel,
                request.Reason,
                request.ReasonDesc))
            {
                res.Code = 2;
                await _server.Transport.SendMessageN(ChannelRecvCode.BanPlayer, res, [request.OperatorId]);
                return;
            }

            await _server.Transport.SendMessageN(ChannelRecvCode.BanPlayer, res, [request.OperatorId, targetChr.Character.Id]);
            await _server.DropWorldMessage(6, nameof(SystemMessage.Ban_NoticeGM), true);
        }

        public List<int> GetBannedAccounts()
        {
            return Query(x => x.EndTime <= _server.GetCurrentTimeDateTimeOffset()).Select(x => x.AccountId).ToList();
        }

        public override List<AccountBanModel> Query(Expression<Func<AccountBanModel, bool>> expression)
        {
            var entityExpress = _mapper.MapExpression<Expression<Func<AccountBanEntity, bool>>>(expression);

            using var dbContext = _dbContextFactory.CreateDbContext();

            var dataFromDB = _mapper.Map<List<AccountBanModel>>(dbContext.AccountBans.Where(entityExpress).ToList());

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
