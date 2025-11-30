using Application.Core.Login.Datas;
using Application.Core.Login.Models;
using Application.Core.Login.Net.Packets;
using Application.Core.Login.Session;
using Application.EF;
using Application.Shared.Login;
using Application.Shared.Net.Logging;
using Application.Shared.Sessions;
using Application.Utility;
using Application.Utility.Configs;
using DotNetty.Transport.Channels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using net.server;

namespace Application.Core.Login.Client
{
    public class LoginClient : ClientBase, ILoginClient
    {
        public AccountLoginStatus AccountLoginStatus { get; private set; }
        public override bool IsOnlined => AccountLoginStatus.State > LoginStage.LOGIN_NOTLOGGEDIN;
        IPacketProcessor<ILoginClient> _packetProcessor;
        readonly SessionCoordinator _sessionCoordinator;
        readonly LoginBypassCoordinator _loginBypassCoordinator;
        public LoginClient(long sessionId, MasterServer currentServer, IChannel nettyChannel, IPacketProcessor<ILoginClient> packetProcessor, SessionCoordinator sessionCoordinator, ILogger<IClientBase> log, LoginBypassCoordinator loginBypassCoordinator)
            : base(sessionId, currentServer, nettyChannel, log)
        {
            CurrentServer = currentServer;
            _sessionCoordinator = sessionCoordinator;
            _packetProcessor = packetProcessor;
            _loginBypassCoordinator = loginBypassCoordinator;

            AccountLoginStatus = AccountLoginStatus.Default;
        }

        public MasterServer CurrentServer { get; set; }
        public int SelectedChannel { get; set; }

        public override int AccountId => AccountEntity?.Id ?? 0;

        public override string AccountName => AccountEntity?.Name ?? "-";
        public override int AccountGMLevel => AccountEntity?.GMLevel ?? 0;

        protected override void ProcessPacket(InPacket packet)
        {
            short opcode = packet.readShort();
            var handler = _packetProcessor.GetPacketHandler(opcode);

            if (YamlConfig.config.server.USE_DEBUG_SHOW_RCVD_PACKET && !LoggingUtil.isIgnoredRecvPacket(opcode))
            {
                log.LogDebug("Received packet id {Code}", opcode);
            }

            if (handler != null && handler.ValidateState(this))
            {
                handler.HandlePacket(packet, this);
            }
        }

        protected override void CloseSessionInternal()
        {
            _sessionCoordinator.closeLoginSession(this);

            if (!IsServerTransition)
            {
                Disconnect();
            }
        }

        public override void SetCharacterOnSessionTransitionState(int cid)
        {
            updateLoginState(LoginStage.LOGIN_SERVER_TRANSITION);
            CurrentServer.SetCharacteridInTransition(GetSessionRemoteHost(), cid);
        }
        public void Disconnect()
        {
            _sessionCoordinator.closeSession(this, false);

            updateLoginState(LoginStage.LOGIN_NOTLOGGEDIN);
            CurrentServer.UnregisterLoginState(this);

            Dispose();
        }

        public override void ForceDisconnect()
        {
            Disconnect();
        }


        public void updateLoginState(sbyte newState)
        {
            if (AccountEntity == null)
                return;

            if (newState == LoginStage.LOGIN_LOGGEDIN)
            {
                _sessionCoordinator.updateOnlineClient(this);
            }

            AccountLoginStatus = CurrentServer.UpdateAccountState(AccountEntity.Id, newState);

            if (newState == LoginStage.LOGIN_NOTLOGGEDIN)
            {
                AccountEntity = null;
            }
            IsServerTransition = newState == LoginStage.LOGIN_SERVER_TRANSITION;
        }

        public override void Dispose()
        {
            base.Dispose();
            AccountEntity = null;
        }

        public bool CanBypassPin()
        {
            return _loginBypassCoordinator.canLoginBypass(Hwid, AccountEntity!.Id, false);
        }
        int pinattempt = 0;
        public bool CheckPin(string other)
        {
            if (!YamlConfig.config.server.ENABLE_PIN || CanBypassPin())
            {
                return true;
            }

            pinattempt++;
            if (pinattempt > 5)
            {
                _sessionCoordinator.closeSession(this, false);
            }
            if (AccountEntity?.Pin == other)
            {
                pinattempt = 0;
                _loginBypassCoordinator.registerLoginBypassEntry(Hwid, AccountEntity!.Id, false);
                return true;
            }
            return false;
        }

        public bool CanBypassPic()
        {
            return _loginBypassCoordinator.canLoginBypass(Hwid, AccountEntity!.Id, true);
        }
        int picattempt = 0;
        public bool CheckPic(string other)
        {
            if (!YamlConfig.config.server.ENABLE_PIC || CanBypassPic())
            {
                return true;
            }

            picattempt++;
            if (picattempt > 5)
            {
                _sessionCoordinator.closeSession(this, false);
            }
            if (AccountEntity?.Pic == other)
            {    // thanks ryantpayton (HeavenClient) for noticing null pics being checked here
                picattempt = 0;
                _loginBypassCoordinator.registerLoginBypassEntry(Hwid, AccountEntity!.Id, true);
                return true;
            }
            return false;
        }

        private int GetLoginState()
        {
            if (AccountEntity == null)
                return 0;

            var localState = CurrentServer.AccountManager.GetAccountLoginStatus(AccountEntity.Id);
            if (localState.State == LoginStage.LOGIN_SERVER_TRANSITION || localState.State == LoginStage.PlayerServerTransition)
            {
                if (localState.DateTime.AddSeconds(30).ToUnixTimeMilliseconds() < CurrentServer.getCurrentTime())
                {
                    updateLoginState(LoginStage.LOGIN_NOTLOGGEDIN);   // ACCID = 0, issue found thanks to Tochi & K u ssss o & Thora & Omo Oppa
                    return LoginStage.LOGIN_NOTLOGGEDIN;
                }
            }

            return localState.State;
        }
        int loginattempt = 0;
        public LoginResultCode Login(string login, string pwd, Hwid nibbleHwid)
        {
            LoginResultCode loginok = LoginResultCode.Fail_AccountNotExsited;

            loginattempt++;
            if (loginattempt > 4)
            {
                _sessionCoordinator.closeSession(this, false);
                return LoginResultCode.Fail_Count;   // thanks Survival_Project for finding out an issue with AUTOMATIC_REGISTER here
            }

            int accId = CurrentServer.GetAccountIdByAccountName(login);
            try
            {
                var dbModel = CurrentServer.GetAccountDto(accId);
                if (dbModel != null)
                {
                    accId = dbModel.Id;
                    if (accId <= 0)
                    {
                        log.LogError("Tried to login with accid " + accId);
                        return LoginResultCode.Fail_SpecialAccount;
                    }

                    AccountEntity = dbModel;
                    string passhash = dbModel.Password;
                    var tos = dbModel.Tos;

                    if (GetLoginState() > LoginStage.LOGIN_NOTLOGGEDIN)
                    {
                        loginok = LoginResultCode.Fail_AlreadyLoggedIn;
                    }
                    else if (pwd.Equals(passhash) || HashDigest.HashByType("SHA-512", pwd).ToHexString().Equals(passhash))
                    {
                        // thanks GabrielSin for detecting some no-bcrypt inconsistencies here
                        loginok = !tos ? LoginResultCode.Fail_Agreement : LoginResultCode.Success; // migrate to bcrypt
                    }
                    else
                    {
                        loginok = LoginResultCode.Fail_IncorrectPassword;
                    }
                }
                else
                {
                    accId = -3;
                }
            }
            catch (DbUpdateException e)
            {
                log.LogError(e.ToString());
            }

            if (loginok == LoginResultCode.Success || loginok == LoginResultCode.Fail_IncorrectPassword)
            {
                AntiMulticlientResult res = _sessionCoordinator.attemptLoginSession(this, nibbleHwid, accId, loginok == LoginResultCode.Fail_IncorrectPassword);

                switch (res)
                {
                    case AntiMulticlientResult.SUCCESS:
                        if (loginok == 0)
                        {
                            loginattempt = 0;
                        }

                        return loginok;

                    case AntiMulticlientResult.REMOTE_LOGGEDIN:
                        return LoginResultCode.Fail_Error17;

                    case AntiMulticlientResult.REMOTE_REACHED_LIMIT:
                        return LoginResultCode.Fail_Error13;

                    case AntiMulticlientResult.REMOTE_PROCESSING:
                        return LoginResultCode.Fail_Error10;

                    case AntiMulticlientResult.MANY_ACCOUNT_ATTEMPTS:
                        return LoginResultCode.Fail_Error16;

                    default:
                        return LoginResultCode.Fail_Error8;
                }
            }
            else
            {
                return loginok;
            }
        }

        public LoginResultCode FinishLogin()
        {
            if (GetLoginState() > LoginStage.LOGIN_NOTLOGGEDIN)
            {
                return LoginResultCode.Fail_AlreadyLoggedIn;
            }
            updateLoginState(LoginStage.LOGIN_LOGGEDIN);

            return LoginResultCode.Success;
        }

        public bool AcceptToS()
        {
            if (AccountEntity == null)
                return false;

            if (AccountEntity.Tos)
                return true;

            AccountEntity.Tos = true;
            CurrentServer.CommitAccountEntity(AccountEntity);
            return false;
        }

        public bool CheckChar(int accid)
        {
            // issue with multiple chars from same account login found by shavit, resinate
            if (!YamlConfig.config.server.USE_CHARACTER_ACCOUNT_CHECK)
            {
                return true;
            }

            var allChrs = CurrentServer.AccountManager.GetAccountPlayerIds(accid);
            foreach (var chrId in allChrs)
            {
                var chr = CurrentServer.CharacterManager.FindPlayerById(chrId);
                if (chr?.Channel != 0)
                {
                    CurrentServer.DisconnectChr(chrId);
                    return false;
                }
            }
            return true;
        }

        public bool isLoggedIn()
        {
            return IsOnlined;
        }

        public void SendCharList()
        {
            sendPacket(LoginPacketCreator.GetCharListPacket(this));
        }

        /// <summary>
        /// 选择角色时加载的角色
        /// </summary>
        /// <param name="worldId"></param>
        /// <returns></returns>
        public List<CharacterViewObject> LoadCharactersView()
        {
            return CurrentServer.LoadAccountCharactersView(AccountEntity!.Id);
        }

        DateTimeOffset lastRequestCharList;
        public bool CanRequestCharlist()
        {
            return lastRequestCharList.AddMilliseconds(877) < DateTimeOffset.UtcNow;
        }
        public void UpdateRequestCharListTick()
        {
            lastRequestCharList = DateTimeOffset.UtcNow;
        }
    }
}
