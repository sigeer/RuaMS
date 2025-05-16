using Application.Core.Datas;
using Application.Core.Login.Datas;
using Application.Core.Servers;
using Application.Shared.Characters;
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

            var accountModel = _accManager.GetAccountEntity(characterObj.Character.AccountId);
            if (accountModel == null)
                return null;

            if (accountModel.Loggedin != LoginStage.LOGIN_SERVER_TRANSITION && accountModel.Loggedin != LoginStage.PlayerServerTransition)
                return null;

            if (accountModel.Hwid == null)
                return null;

            if (YamlConfig.config.server.USE_IP_VALIDATION && !_masterServer.ValidateCharacteridInTransition(clientSession, characterId))
                return null;

            _accManager.UpdateAccountState(accountModel, LoginStage.LOGIN_LOGGEDIN);
            characterObj.Account = _mapper.Map<AccountDto>(accountModel);
            return characterObj;
        }
    }
}
