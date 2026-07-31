using Application.Core.Login.Dtos.Account;
using Application.Core.Login.Shared;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Constants;
using Application.Shared.Login;
using Application.Shared.Message;
using Application.Utility;
using Application.Utility.Exceptions;
using Application.Utility.Extensions;
using Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Application.Core.Login.Datas
{
    public class AccountManager : DataStorageBase<int, AccountCtrl, AccountEntity>
    {
        /// <summary>
        /// 账户登录态记录
        /// </summary>
        ConcurrentDictionary<int, AccountLoginStatus> _accStageCache = new();

        /// <summary>
        /// 账户及其拥有的角色id缓存
        /// </summary>
        ConcurrentDictionary<int, HashSet<int>> _accPlayerCache = new();

        readonly MasterServer _server;
        public AccountManager(ILogger<AccountManager> logger, IDbContextFactory<DBContext> dbContextFactory, IMapper maaper, MasterServer server)
            :base(StorageCategory.Account, dbContextFactory, maaper, logger)
        {
            _server = server;
        }

        static AccountCtrl System = new AccountCtrl { Name = "System", Id = ServerConstants.SystemCId, GMLevel = 6 };

        protected override int GetKey(AccountCtrl model) => model.Id;

        public AccountCtrl? GetAccountDto(int accId)
        {
            return GetAccount(accId);
        }

        public override AccountCtrl? Find(int key)
        {
            if (key == ServerConstants.SystemCId)
            {
                return System;
            }
            return base.Find(key);
        }

        public int GetAccountIdByName(string accName)
        {
            return Find(x => x.Name == accName, x => x.Name == accName)?.Id ?? -2;
        }

        public AccountLoginStatus GetAccountLoginStatus(int accId)
        {
            return _accStageCache.GetOrAdd(accId, (id) =>
            {
                using var dbContext = _dbContextFactory.CreateDbContext();
                var dbModel = dbContext.Accounts.AsNoTracking().FirstOrDefault(x => x.Id == id);
                if (dbModel != null)
                {
                    return new AccountLoginStatus(0, DateTimeOffset.MinValue);
                }
                else
                    throw new BusinessException($"账号不存在，Id = {accId}");
            });
        }



        public AccountLoginStatus UpdateAccountState(int accId, sbyte newState)
        {
            var d = GetAccountLoginStatus(accId);
            d.State = newState;
            d.ProcessTime = _server.GetCurrentTimeDateTimeOffset();
            return d;
        }

        public void SetClientLanguage(int accId, int language)
        {
            var d = GetAccountLoginStatus(accId);
            d.Language = language;
        }

        public void CreateAccount(string loginAccount, string pwd)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var password = HashDigest.HashByType("SHA-512", pwd).ToHexString();
            var newAccModel = new AccountEntity(loginAccount, password);
            dbContext.Accounts.Add(newAccModel);
            dbContext.SaveChanges();
        }

        public HashSet<int> GetAccountPlayerIds(int accId)
        {
            if (_accPlayerCache.TryGetValue(accId, out var d))
                return d;

            using var dbContext = _dbContextFactory.CreateDbContext();
            var e = dbContext.Characters.Where(x => x.AccountId == accId).AsNoTracking().Select(x => x.Id).ToHashSet();
            _accPlayerCache[accId] = e;
            return e;
        }

        public bool ValidAccountCharacter(int accId, int charId)
        {
            return GetAccountPlayerIds(accId).Contains(charId);
        }

        public void UpdateAccountCharacterCacheByAdd(int accId, int charId)
        {
            if (_accPlayerCache.TryGetValue(accId, out var d))
                d.Add(charId);
            else
            {
                _accPlayerCache[accId] = [charId];
            }
        }

        public void UpdateAccountCharacterCacheByRemove(int accId, int charId)
        {
            if (_accPlayerCache.TryGetValue(accId, out var d))
                d.Remove(charId);
        }


        internal AccountCtrl? GetAccount(int accountId)
        {
            return Find(accountId);
        }

        public void UpdateAccount(AccountCtrl obj)
        {
            SetDirty(obj);
        }

        public ConfigProto.SetFlyResponse SetFly(ConfigProto.SetFlyRequest request)
        {
            var chr = _server.CharacterManager.FindPlayerById(request.CId);
            if (chr != null)
            {
                var acc = GetAccount(chr.Character.AccountId);
                if (acc != null)
                {
                    acc.GmMode = request.SetStatus;
                    return new ConfigProto.SetFlyResponse { Code = 0, Request = request };
                }
            }
            return new ConfigProto.SetFlyResponse() { Code = 1 };
        }

        public int[] GetOnlinedGmAccId()
        {
            return Query(x => x.GMLevel > 1, x => x.IsGmAccount()).Select(x => x.Id).ToArray();
        }

        public async Task SetGmLevel(SystemProto.SetGmLevelRequest request)
        {
            var res = new SystemProto.SetGmLevelResponse { Request = request };
            var targetChr = _server.CharacterManager.FindPlayerByName(request.TargetName);
            if (targetChr == null)
            {
                res.Code = 1;
                await _server.Transport.SendMessageN(ChannelRecvCode.InvokeSetGmLevel, res, [request.OperatorId]);
                return;
            }

            var accountDto = GetAccount(targetChr.Character.AccountId)!;
            accountDto.GMLevel = (sbyte)request.Level;
            UpdateAccount(accountDto);

            res.TargetId = targetChr.Character.Id;
            await _server.Transport.SendMessageN(ChannelRecvCode.InvokeSetGmLevel, res, [request.OperatorId, res.TargetId]);
        }

        public bool GainCharacterSlot(int accId)
        {
            var acc = GetAccount(accId)!;
            if (acc.Characterslots < Limits.MaxCharacterSlots)
            {
                acc.Characterslots += 1;
                UpdateAccount(acc);

                return true;
            }
            return false;
        }

        public GetAllClientInfo GetOnliendClientInfo()
        {
            var onlinedPlayerAccounts = _server.CharacterManager.GetOnlinedPlayerAccountId();
            var accountInfo = Query(x => onlinedPlayerAccounts.Contains(x.Id), x => onlinedPlayerAccounts.Contains(x.Id));

            var res = new GetAllClientInfo();
            res.List.AddRange(accountInfo.Select(x => new ClientInfo { AccountName = x.Name, CharacterName = "", CurrentHWID = x.CurrentHwid, CurrentIP = x.CurrentIP, CurrentMAC = x.CurrentMac }));
            return res;
        }

        public bool TryGetGMInfo(int accId, out int gmLevel)
        {
            gmLevel = 0;
            var acc = GetAccountDto(accId);
            if (acc == null)
                return false;

            gmLevel = acc.GMLevel;

            return acc.IsGmAccount();
        }

        public override async Task InitializeAsync(DBContext dbContext)
        {
            await base.InitializeAsync(dbContext);

            _accPlayerCache = new((await dbContext.Characters.AsNoTracking().Select(x => new { Id = x.Id, AccountId = x.AccountId }).ToListAsync())
                .GroupBy(x => x.AccountId)
                .ToDictionary(x => x.Key, x => x.Select(y => y.Id).ToHashSet()));
        }

        protected override AccountEntity MapExsitedEntity(AccountCtrl localModel, AccountEntity dbModel)
        {
            dbModel.Pic = localModel.Pic;
            dbModel.Pin = localModel.Pin;
            dbModel.Gender = localModel.Gender;
            dbModel.Tos = localModel.Tos;
            dbModel.GMLevel = localModel.GMLevel;
            dbModel.Characterslots = localModel.Characterslots;
            return dbModel;
        }

        #region Console 非实时数据
        public (List<AccountResponseDto> Data, int Total) GetAccountPagedData(int pageIndex, int pageSize)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var data = dbContext.Accounts.OrderBy(x => x.Id).ToPage(pageIndex, pageSize).ToList();
            var ids = data.Select(x => x.Id).ToList();
            var final = data.Select(x => new AccountResponseDto 
            {
                Id= x.Id,
                Name= x.Name,
                GMLevel = x.GMLevel,
            }).ToList();
            var banInfos = _server.AccountBanManager.FilterAccount(ids);
            foreach (var item in final)
            {
                var banInfo = banInfos.FirstOrDefault(x => x.AccountId == item.Id);
                if (banInfo != null)
                    item.BanInfo = new AccountBanPreviewDto
                    {
                        Start = banInfo.StartTime,
                        End = banInfo.EndTime
                    };
            }
            return (final, dbContext.Accounts.Count());
        }

        public AccountPreviewResponseDto? GetAccountPreview(int accId)
        {
            var model = Find(accId);
            if (model == null)
            {
                return null;
            }

            return _mapper.Map<AccountPreviewResponseDto>(model);
        }

        public AccountDetailDto? GetAccountDetailAsync(int accId)
        {
            var model = Find(accId);
            if (model == null)
            {
                return null;
            }

            return new AccountDetailDto
            {

            };
        }
        #endregion
    }
}
