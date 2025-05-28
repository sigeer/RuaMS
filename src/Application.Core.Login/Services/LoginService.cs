using Application.Core.Login.Datas;
using Application.Core.Servers;
using Application.EF;
using Application.Shared.Characters;
using Application.Shared.Dto;
using Application.Shared.Login;
using Application.Utility.Configs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

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
        /// <param name="characterId"></param>
        /// <returns></returns>
        public CharacterValueObject? PlayerLogin(string clientSession, int characterId)
        {
            var characterObj = _masterServer.CharacterManager.GetCharacter(characterId);
            if (characterObj == null || characterObj.Character == null)
                return null;

            if (characterObj.Account == null || characterObj.Account.Hwid == null)
                return null;

            var accountModel = _masterServer.AccountManager.GetAccountLoginStatus(characterObj.Account.Id);
            if (accountModel.State != LoginStage.LOGIN_SERVER_TRANSITION && accountModel.State != LoginStage.PlayerServerTransition)
                return null;

            if (YamlConfig.config.server.USE_IP_VALIDATION && !_masterServer.ValidateCharacteridInTransition(clientSession, characterId))
                return null;

            characterObj.LoginInfo = new LoginInfo { IsNewCommer = accountModel.State == LoginStage.LOGIN_SERVER_TRANSITION };

            return characterObj;
        }

        public void SetPlayerLogedIn(int playerId, int channel)
        {
            _masterServer.CharacterManager.SetPlayerChannel(playerId, channel, out var accId);
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
