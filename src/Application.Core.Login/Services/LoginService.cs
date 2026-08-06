using Application.EF;
using Application.Shared.Login;
using Application.Utility.Configs;
using Google.Protobuf;
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
        public ProtoModel.PlayerGetterProto? PlayerLogin(string clientSession, int characterId)
        {
            var characterObj = _masterServer.CharacterManager.FindPlayerById(characterId);
            if (characterObj == null || characterObj.Character == null)
                return null;

            var accountData = _masterServer.AccountManager.GetAccountDto(characterObj.Character.AccountId);
            if (accountData == null || accountData.CurrentHwid == null || accountData.CurrentMac == null)
                return null;

            var accountModel = _masterServer.AccountManager.GetAccountLoginStatus(characterObj.Character.AccountId);
            if (accountModel.State != LoginStage.LOGIN_SERVER_TRANSITION && accountModel.State != LoginStage.PlayerServerTransition)
                return null;

            if (YamlConfig.config.server.USE_IP_VALIDATION && !_masterServer.ValidateCharacteridInTransition(clientSession, characterId))
                return null;

            var banInfo = _masterServer.AccountBanManager.GetAccountBanInfo(characterObj.Character.AccountId);
            if (banInfo != null)
                return null;

            _masterServer.CharacterManager.FlushCharacter(characterObj);

            var data = _mapper.Map<ProtoModel.PlayerGetterProto>(characterObj);
            data.LoginInfo = new ProtoModel.LoginInfoProto
            {
                IsNewCommer = accountModel.State == LoginStage.LOGIN_SERVER_TRANSITION,
                Language = accountModel.Language
            };

            using var dbContext = _dbContextFactory.CreateDbContext();
            data.Link = dbContext.Characters.Where(x => x.AccountId == data.Character.AccountId && x.Id != data.Character.Id).OrderByDescending(x => x.Level)
                .Select(x => new ProtoModel.CharacterLinkProto() { Level = x.Level, Name = x.Name }).FirstOrDefault();

            data.RingSourceList.AddRange(_masterServer.RingManager.LoadCharacterRings(data.Character.Id));
            data.AccountGame = _masterServer.AccountGameManager.GetAccountGameData(data.Character.AccountId);
            data.Account = _mapper.Map<ProtoModel.AccountInfoProto>(accountData);
            data.NewYearCards.AddRange(_masterServer.NewYearCardManager.LoadPlayerNewYearCard(data.Character.Id));
            data.RemoteCallList.AddRange(_masterServer.CrossServerService.GetCallback(characterId));
            return data;
        }

        public async Task SetPlayerLogedIn(int playerId, int channel)
        {
            var accId = await _masterServer.CharacterManager.CompleteLogin(playerId, channel);
            _masterServer.AccountManager.UpdateAccountState(accId, LoginStage.LOGIN_LOGGEDIN);
        }
    }
}
