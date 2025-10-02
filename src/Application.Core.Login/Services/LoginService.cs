using Application.EF;
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
        /// <param name="channelId"></param>
        /// <param name="characterId"></param>
        /// <returns></returns>
        public SyncProto.PlayerGetterDto? PlayerLogin(string clientSession, int characterId)
        {
            var characterObj = _masterServer.CharacterManager.FindPlayerById(characterId);
            if (characterObj == null || characterObj.Character == null)
                return null;

            var accountData = _masterServer.AccountManager.GetAccount(characterObj.Character.AccountId);
            if (accountData == null || accountData.CurrentHwid == null || accountData.CurrentMac == null)
                return null;

            var accountModel = _masterServer.AccountManager.GetAccountLoginStatus(characterObj.Character.AccountId);
            if (accountModel.State != LoginStage.LOGIN_SERVER_TRANSITION && accountModel.State != LoginStage.PlayerServerTransition)
                return null;

            if (YamlConfig.config.server.USE_IP_VALIDATION && !_masterServer.ValidateCharacteridInTransition(clientSession, characterId))
                return null;

            var data = _mapper.Map<SyncProto.PlayerGetterDto>(characterObj);
            data.LoginInfo = new SyncProto.LoginInfo { 
                IsNewCommer = accountModel.State == LoginStage.LOGIN_SERVER_TRANSITION ,
                Language = accountModel.Language
            };


            using var dbContext = _dbContextFactory.CreateDbContext();


            data.Link = dbContext.Characters.Where(x => x.AccountId == data.Character.AccountId && x.Id != data.Character.Id).OrderByDescending(x => x.Level)
                .Select(x => new SyncProto.CharacterLinkDto() { Level = x.Level, Name = x.Name }).FirstOrDefault();

            data.AccountGame = _mapper.Map<Dto.AccountGameDto>(_masterServer.AccountManager.GetAccountGameData(data.Character.AccountId));
            data.Account = _mapper.Map<Dto.AccountCtrlDto>(accountData);
            data.RemoteCallList.AddRange(_masterServer.CrossServerService.GetCallback(characterId));
            return data;
        }

        public void SetPlayerLogedIn(int playerId, int channel)
        {
            _masterServer.CharacterManager.CompleteLogin(playerId, channel, out var accId);
            _masterServer.AccountManager.UpdateAccountState(accId, LoginStage.LOGIN_LOGGEDIN);
        }
    }
}
