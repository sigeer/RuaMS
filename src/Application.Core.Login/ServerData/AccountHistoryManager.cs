using Application.Core.EF.Entities;
using Application.Core.Login.Dtos.Ban;
using Application.Core.Login.Models.Accounts;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Resources.Messages;
using Application.Shared.Login;
using Application.Shared.Message;
using Application.Utility;
using Application.Utility.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using SystemProto;

namespace Application.Core.Login.ServerData
{
    public class AccountHistoryManager : DataStorageBase<int, AccountHistoryModel, AccountBindingsEntity>
    {
        readonly MasterServer _server;

        public AccountHistoryManager(IDbContextFactory<DBContext> dbContextFactory, IMapper mapper, MasterServer server, ILogger<AccountHistoryManager> logger)
            : base(StorageCategory.AccountHistory, dbContextFactory, mapper, logger)
        {
            _server = server;
        }

        protected override int GetKey(AccountHistoryModel model) => model.Id;

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
            SetDirty(model);
            return model;
        }

        public void AttachAccountMAC(int id, string mac)
        {
            var model = Find(id);
            if (model != null)
            {
                model.MAC = mac;
                SetDirty(model);
            }
        }

    }


    public class AccountBanManager : DBStorageBase
    {
        readonly MasterServer _server;


        List<IpbanEntity> bannedIP = new();
        List<MacbanEntity> bannedMAC = new();
        List<HwidbanEntity> bannedHWID = new();

        public AccountBanManager(IDbContextFactory<DBContext> dbContextFactory, IMapper mapper, MasterServer server, ILogger<AccountBanManager> logger) 
            : base(StorageCategory.Ban, dbContextFactory, mapper, logger)
        {
            _server = server;
        }

        public override async Task InitializeAsync(DBContext dbContext)
        {
            await base.InitializeAsync(dbContext);

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
        public List<AccountBanEntity> FilterAccount(IEnumerable<int> accIdList)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            return dbContext.AccountBans.Where(x => accIdList.Contains(x.AccountId) && x.EndTime >= _server.GetCurrentTimeDateTimeOffset() && !x.Canceled).ToList();
        }
        public AccountBanEntity? GetAccountBanInfo(DBContext dbContext, int accountId)
        {
            return dbContext.AccountBans.Where(x => x.AccountId == accountId && x.EndTime >= _server.GetCurrentTimeDateTimeOffset() && !x.Canceled).FirstOrDefault();
        }

        public AccountBanEntity? GetAccountBanInfo(int accountId)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            return dbContext.AccountBans.Where(x => x.AccountId == accountId && x.EndTime >= _server.GetCurrentTimeDateTimeOffset() && !x.Canceled).FirstOrDefault();
        }

        public async Task<bool> BanAccount(int operatorId, int accountId, DateTimeOffset endTime, int level, int reason, string reasonDesc)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var banModel = GetAccountBanInfo(dbContext, accountId);
            if (banModel != null)
                return false;

            var banLevel = (BanLevel)level;
            banModel = new AccountBanEntity(accountId, _server.GetCurrentTimeDateTimeOffset(), endTime, level, reason, reasonDesc, operatorId);
            dbContext.AccountBans.Add(banModel);
            await dbContext.SaveChangesAsync();

            var dayBeforeMonth = _server.GetCurrentTimeDateTimeOffset().AddMonths(-1);
            var histories = _server.AccountHistoryManager.Query(
                x => x.AccountId == accountId && x.LastActiveTime >= dayBeforeMonth, 
                x => x.AccountId == accountId && x.LastActiveTime >= dayBeforeMonth);
            foreach (var his in histories)
            {
                if (banLevel.HasFlag(BanLevel.IP))
                {
                    bannedIP.Add(new IpbanEntity(his.IP, accountId) { LinkedBanId = banModel.Id});
                }
                if (banLevel.HasFlag(BanLevel.Mac))
                {
                    foreach (var mac in his.MAC.Split(','))
                    {
                        bannedMAC.Add(new MacbanEntity(mac.Trim(), accountId) { LinkedBanId = banModel.Id });
                    }
                }
                if (banLevel.HasFlag(BanLevel.Hwid))
                {
                    bannedHWID.Add(new HwidbanEntity(his.HWID, accountId) { LinkedBanId = banModel.Id });
                }
            }

            return true;
        }

        public async Task<bool> UnbanAccount(int opAccId, int accountId)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var banModel = GetAccountBanInfo(dbContext, accountId);
            if (banModel == null)
                return false;

            banModel.UnBan(opAccId);
            await dbContext.SaveChangesAsync();

            bannedIP.RemoveAll(x => x.LinkedBanId == banModel.Id);
            bannedMAC.RemoveAll(x => x.LinkedBanId == banModel.Id);
            bannedHWID.RemoveAll(x => x.LinkedBanId == banModel.Id);
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

            else if (!await UnbanAccount(request.OperatorId, targetChr.Character.AccountId))
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

            if (!await BanAccount(request.OperatorId, targetChr.Character.AccountId,
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
            using var dbContext = _dbContextFactory.CreateDbContext();
            return dbContext.AccountBans.Where(x => x.EndTime <= _server.GetCurrentTimeDateTimeOffset() && x!.Canceled).Select(x => x.AccountId).ToList();
        }

        public override async Task Commit(DBContext dbContext)
        {
            await base.Commit(dbContext);

            await dbContext.Ipbans.ExecuteDeleteAsync();
            await dbContext.Macbans.ExecuteDeleteAsync();
            await dbContext.Hwidbans.ExecuteDeleteAsync();

            dbContext.Ipbans.AddRange(bannedIP);
            dbContext.Macbans.AddRange(bannedMAC);
            dbContext.Hwidbans.AddRange(bannedHWID);

            await dbContext.SaveChangesAsync();
        }

        public (List<BanResponseDto> Data, int Total) GetBanPagedData(int inBan, int pageIndex, int pageSize)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var now = _server.GetCurrentTimeDateTimeOffset();

            var all = dbContext.AccountBans.OrderByDescending(x => x.StartTime).AsQueryable();

            if (inBan == 0)
                all = all.Where(x => now < x.StartTime || now > x.EndTime || x.Canceled);
            else if (inBan > 1)
                all = all.Where(x => now >= x.StartTime && now <= x.EndTime && !x.Canceled);

            var pagedData = all.ToPage(pageIndex, pageSize).ToList();
            List<BanResponseDto> list = [];
            foreach (var item in pagedData)
            {
                var data = _mapper.Map<BanResponseDto>(item);
                data.Account = _server.AccountManager.GetAccountPreview(item.AccountId);
                data.OperateAccount = _server.AccountManager.GetAccountPreview(item.OperateAccountId);
                data.AuditAccount = _server.AccountManager.GetAccountPreview(item.AuditAccountId);
                list.Add(data);
            }

            return (list, all.Count());
        }
    }
}
