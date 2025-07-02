using Application.EF;
using Application.Shared.Login;
using Application.Utility.Configs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;

namespace Application.Core.Login.Services
{
    public class LoginService
    {
        readonly IMapper _mapper;
        readonly MasterServer _masterServer;
        readonly IDbContextFactory<DBContext> _dbContextFactory;


        public LoginService(IMapper mapper, MasterServer masterServer, IDbContextFactory<DBContext> dbContextFactory)
        {
            _mapper = mapper;
            _masterServer = masterServer;
            _dbContextFactory = dbContextFactory;
        }


        /// <summary>
        /// 角色登录
        /// </summary>
        /// <param name="clientSession"></param>
        /// <param name="channelId"></param>
        /// <param name="characterId"></param>
        /// <returns></returns>
        public Dto.PlayerGetterDto? PlayerLogin(string clientSession, int channelId, int characterId)
        {
            var characterObj = _masterServer.CharacterManager.FindPlayerById(characterId);
            if (characterObj == null || characterObj.Character == null)
                return null;

            var accountData = _masterServer.AccountManager.GetAccount(characterObj.Character.AccountId);
            if (accountData == null || accountData.Hwid == null)
                return null;

            var accountModel = _masterServer.AccountManager.GetAccountLoginStatus(characterObj.Character.AccountId);
            if (accountModel.State != LoginStage.LOGIN_SERVER_TRANSITION && accountModel.State != LoginStage.PlayerServerTransition)
                return null;

            if (YamlConfig.config.server.USE_IP_VALIDATION && !_masterServer.ValidateCharacteridInTransition(clientSession, characterId))
                return null;


            var data = _mapper.Map<Dto.PlayerGetterDto>(characterObj);
            data.LoginInfo = new Dto.LoginInfo { IsNewCommer = accountModel.State == LoginStage.LOGIN_SERVER_TRANSITION };


            using var dbContext = _dbContextFactory.CreateDbContext();
            var now = DateTimeOffset.UtcNow;
            var fameRecords = dbContext.Famelogs.AsNoTracking().Where(x => x.Characterid == characterId && Microsoft.EntityFrameworkCore.EF.Functions.DateDiffDay(now, x.When) < 30).ToList();

            data.FameRecord = new Dto.RecentFameRecordDto();
            data.FameRecord.LastUpdateTime = fameRecords.Count == 0 ? 0 : fameRecords.Max(x => x.When).ToUnixTimeMilliseconds();
            data.FameRecord.ChararacterIds.AddRange(fameRecords.Select(x => x.CharacteridTo));

            data.Link = dbContext.Characters.Where(x => x.AccountId == data.Character.AccountId && x.Id != data.Character.Id).OrderByDescending(x => x.Level)
                .Select(x => new Dto.CharacterLinkDto() { Level = x.Level, Name = x.Name }).FirstOrDefault();

            data.AccountGame = _mapper.Map<Dto.AccountGameDto>(_masterServer.AccountManager.GetAccountGameData(data.Character.AccountId));
            data.Account = _mapper.Map<Dto.AccountCtrlDto>(accountData);
            data.PendingTransactions.AddRange(_masterServer.ItemTransactionManager.GetPlayerPendingTransactions(data.Character.Id));
            return data;
        }

        public void SetPlayerLogedIn(int playerId, int channel)
        {
            _masterServer.CharacterManager.CompleteLogin(playerId, channel, out var accId);
            _masterServer.AccountManager.UpdateAccountState(accId, LoginStage.LOGIN_LOGGEDIN);
        }

        public void UnBanAccount(string playerName)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var aid = dbContext.Characters.Where(x => x.Name == playerName).FirstOrDefault()?.AccountId;
            dbContext.Accounts.Where(x => x.Id == aid).ExecuteUpdate(x => x.SetProperty(y => y.Banned, -1));

            dbContext.Ipbans.Where(x => x.Aid == aid.ToString()).ExecuteDelete();

            dbContext.Macbans.Where(x => x.Aid == aid.ToString()).ExecuteDelete();
        }
    }
}
