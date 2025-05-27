using Application.Core.Login.Datas;
using Application.Core.Servers;
using Application.Shared.Characters;
using Application.Shared.Dto;
using Application.Shared.Login;
using Application.Utility.Configs;
using AutoMapper;

namespace Application.Core.Login.Services
{
    public class LoginService
    {
        readonly IMapper _mapper;
        readonly CharacterManager _characterManager;
        readonly AccountManager _accManager;
        readonly IMasterServer _masterServer;

        public LoginService(IMapper mapper, CharacterManager characterManager, AccountManager accountManager, IMasterServer masterServer)
        {
            _mapper = mapper;
            _characterManager = characterManager;
            _accManager = accountManager;
            _masterServer = masterServer;
        }

        /// <summary>
        /// 角色登录
        /// </summary>
        /// <param name="clientSession"></param>
        /// <param name="characterId"></param>
        /// <returns></returns>
        public CharacterValueObject? PlayerLogin(string clientSession, int characterId)
        {
            var characterObj = _characterManager.GetCharacter(characterId);
            if (characterObj == null || characterObj.Character == null)
                return null;

            if (characterObj.Account == null || characterObj.Account.Hwid == null)
                return null;

            var accountModel = _accManager.GetAccountLoginStatus(characterObj.Account.Id);
            if (accountModel.State != LoginStage.LOGIN_SERVER_TRANSITION && accountModel.State != LoginStage.PlayerServerTransition)
                return null;

            if (YamlConfig.config.server.USE_IP_VALIDATION && !_masterServer.ValidateCharacteridInTransition(clientSession, characterId))
                return null;

            characterObj.LoginInfo = new LoginInfo { IsNewCommer = accountModel.State == LoginStage.LOGIN_SERVER_TRANSITION };

            return characterObj;
        }

        public void SetPlayerLogedIn(int playerId, int channel)
        {
            _characterManager.SetPlayerChannel(playerId, channel, out var accId);
            _accManager.UpdateAccountState(accId, LoginStage.LOGIN_LOGGEDIN);
        }
    }
}
