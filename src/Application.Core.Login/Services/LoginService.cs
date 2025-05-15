using Application.Core.Datas;
using Application.Core.Login.Session;
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
        readonly IMasterServer _masterServer;
        readonly SessionCoordinator _sessionCoordinator;

        public LoginService(IMapper mapper, CharacterManager characterManager, IMasterServer masterServer, SessionCoordinator sessionCoordinator)
        {
            _mapper = mapper;
            _characterManager = characterManager;
            _masterServer = masterServer;
            _sessionCoordinator = sessionCoordinator;
        }

        /// <summary>
        /// 角色登录
        /// </summary>
        /// <param name="clientSession"></param>
        /// <param name="characterId"></param>
        /// <returns></returns>
        public async Task<CharacterValueObject?> PlayerLogin(string clientSession, int characterId)
        {
            var characterObj = await _characterManager.GetCharacter(characterId);
            if (characterObj == null)
                return null;

            if (characterObj.Account.Loggedin != LoginStage.LOGIN_SERVER_TRANSITION)
                return null;

            if (YamlConfig.config.server.USE_IP_VALIDATION && !_masterServer.ValidateCharacteridInTransition(clientSession, characterId))
                return null;

            return characterObj;
        }
    }
}
