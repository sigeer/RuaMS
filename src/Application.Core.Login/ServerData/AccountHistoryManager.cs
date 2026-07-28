using Application.Core.EF.Entities;
using Application.Core.Login.Models.Accounts;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Resources.Messages;
using Application.Shared.Login;
using Application.Shared.Message;
using Application.Utility;
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


    public class AccountBanManager : DataStorageBase<int, AccountBanModel, AccountBanEntity>
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

        protected override int GetKey(AccountBanModel model) => model.Id;

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
            return Query(x => x.AccountId == accountId && x.EndTime >= _server.GetCurrentTimeDateTimeOffset(), 
                x => x.AccountId == accountId && x.EndTime >= _server.GetCurrentTimeDateTimeOffset()).FirstOrDefault();
        }

        public bool BanAccount(int accountId, DateTimeOffset endTime, int level, int reason, string reasonDesc)
        {
            var banModel = GetAccountBanInfo(accountId);
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

            SetDirty(banModel);

            bannedIP.RemoveAll(x => x.Aid == accountId);
            bannedHWID.RemoveAll(x => x.AccountId == accountId);
            bannedMAC.RemoveAll(x => x.Aid == accountId);

            var dayBeforeMonth = _server.GetCurrentTimeDateTimeOffset().AddMonths(-1);
            var histories = _server.AccountHistoryManager.Query(
                x => x.AccountId == accountId && x.LastActiveTime >= dayBeforeMonth, 
                x => x.AccountId == accountId && x.LastActiveTime >= dayBeforeMonth);
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
            var banModel = GetAccountBanInfo(accountId);
            if (banModel == null)
                return false;

            SetRemoved(banModel);

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
            return Query(x => x.EndTime <= _server.GetCurrentTimeDateTimeOffset(), x => x.EndTime <= _server.GetCurrentTimeDateTimeOffset()).Select(x => x.AccountId).ToList();
        }



        protected override async Task CommitInternal(DBContext dbContext, Dictionary<int, StoreUnit<AccountBanModel>> updateData)
        {
            var updateKeys = updateData.Keys.ToList();
            await dbContext.AccountBans.Where(x => updateKeys.Contains(x.Id)).ExecuteDeleteAsync();

            foreach (var kw in updateData)
            {
                var item = kw.Value;
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
