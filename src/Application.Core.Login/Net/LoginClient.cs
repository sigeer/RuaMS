using Application.Core.Client;
using Application.Core.Game.Players;
using Application.Core.Login.Net.Packets;
using Application.Core.Net;
using Application.Core.Servers;
using Application.Core.tools;
using Application.EF;
using Application.EF.Entities;
using Application.Shared.Login;
using Application.Utility.Configs;
using DotNetty.Transport.Channels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using net;
using net.packet;
using net.packet.logging;
using net.server;
using net.server.coordinator.login;
using net.server.coordinator.session;
using tools;
using XmlWzReader;
using static net.server.coordinator.session.SessionCoordinator;

namespace Application.Core.Login.Net
{
    public class LoginClient : SocketClient, ILoginClient
    {
        public AccountEntity? AccountEntity { get; set; }
        public override bool IsOnlined => AccountEntity?.Loggedin == AccountStage.LOGIN_LOGGEDIN;
        IPacketProcessor<ILoginClient> _packetProcessor;
        public LoginClient(long sessionId, IMasterServer currentServer, IChannel nettyChannel, IPacketProcessor<ILoginClient> packetProcessor, ILogger<IClientBase> log)
            : base(sessionId, currentServer, nettyChannel, log)
        {
            CurrentServer = currentServer;
            _packetProcessor = packetProcessor;
        }

        public new IMasterServer CurrentServer { get; set; }
        public int SelectedChannel { get ; set; }

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
            SessionCoordinator.getInstance().closeLoginSession(this);
        }

        public override void ForceDisconnect()
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            SessionCoordinator.getInstance().closeSession(this, false);

            updateLoginState(AccountStage.LOGIN_NOTLOGGEDIN);
            CurrentServer.UnregisterLoginState(this);
        }

        public override void Dispose()
        {
            AccountEntity = null;
            this.packetChannel.Writer.TryComplete();
        }

        public void updateLoginState(sbyte newState)
        {
            if (AccountEntity == null)
                return;

            // rules out possibility of multiple account entries
            if (newState == AccountStage.LOGIN_LOGGEDIN)
            {
                SessionCoordinator.getInstance().updateOnlineClient(this);
            }

            AccountEntity.Loggedin = newState;
            AccountEntity.Lastlogin = DateTimeOffset.Now;

            if (newState == AccountStage.LOGIN_NOTLOGGEDIN)
            {
                AccountEntity = null;
            }
        }
        public bool CanBypassPin()
        {
            return LoginBypassCoordinator.getInstance().canLoginBypass(Hwid, AccountEntity!.Id, false);
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
                SessionCoordinator.getInstance().closeSession(this, false);
            }
            if (AccountEntity?.Pin == other)
            {
                pinattempt = 0;
                LoginBypassCoordinator.getInstance().registerLoginBypassEntry(Hwid, AccountEntity!.Id, false);
                return true;
            }
            return false;
        }

        public bool CanBypassPic()
        {
            return LoginBypassCoordinator.getInstance().canLoginBypass(Hwid, AccountEntity!.Id, true);
        }
        int picattempt = 0;
        public bool CheckPic(string other)
        {
            if (!(YamlConfig.config.server.ENABLE_PIC && !CanBypassPic()))
            {
                return true;
            }

            picattempt++;
            if (picattempt > 5)
            {
                SessionCoordinator.getInstance().closeSession(this, false);
            }
            if (AccountEntity?.Pic == other)
            {    // thanks ryantpayton (HeavenClient) for noticing null pics being checked here
                picattempt = 0;
                LoginBypassCoordinator.getInstance().registerLoginBypassEntry(Hwid, AccountEntity!.Id, true);
                return true;
            }
            return false;
        }

        int loginattempt = 0;
        public int GetLoginState()
        {
            if (AccountEntity == null)
                return 0;


            if (AccountEntity.Loggedin == AccountStage.LOGIN_SERVER_TRANSITION)
            {
                if (AccountEntity.Lastlogin!.Value.AddSeconds(30).ToUnixTimeMilliseconds() < Server.getInstance().getCurrentTime())
                {
                    updateLoginState(AccountStage.LOGIN_NOTLOGGEDIN);   // ACCID = 0, issue found thanks to Tochi & K u ssss o & Thora & Omo Oppa
                    return AccountStage.LOGIN_NOTLOGGEDIN;
                }
                else
                {
                    return AccountStage.LOGIN_SERVER_TRANSITION;
                }
            }

            AccountEntity.Loggedin = AccountStage.LOGIN_LOGGEDIN;
            return AccountEntity.Loggedin;
        }

        public LoginResultCode Login(string login, string pwd, Hwid nibbleHwid)
        {
            LoginResultCode loginok = LoginResultCode.Fail_AccountNotExsited;

            loginattempt++;
            if (loginattempt > 4)
            {
                SessionCoordinator.getInstance().closeSession(this, false);
                return LoginResultCode.Fail_Count;   // thanks Survival_Project for finding out an issue with AUTOMATIC_REGISTER here
            }

            int accId = -2;
            try
            {
                using var dbContext = new DBContext();
                var dbModel = dbContext.Accounts.Where(x => x.Name == login).AsNoTracking().FirstOrDefault();
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
                    if (dbModel.Banned == 1)
                        return LoginResultCode.Fail_Banned;

                    if (GetLoginState() > AccountStage.LOGIN_NOTLOGGEDIN)
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
                AntiMulticlientResult res = SessionCoordinator.getInstance().attemptLoginSession(this, nibbleHwid, accId, loginok == LoginResultCode.Fail_IncorrectPassword);

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
            if (GetLoginState() > AccountStage.LOGIN_NOTLOGGEDIN)
            {
                return LoginResultCode.Fail_AlreadyLoggedIn;
            }
            updateLoginState(AccountStage.LOGIN_LOGGEDIN);

            return LoginResultCode.Success;
        }

        public bool HasBannedIP()
        {
            using var dbContext = new DBContext();
            return dbContext.Ipbans.Any(x => x.Ip.Contains(RemoteAddress));
        }

        private HashSet<string> GetMac()
        {
            if (AccountEntity == null || string.IsNullOrEmpty(AccountEntity.Macs))
                return [];

            return AccountEntity.Macs.Split(",").Select(x => x.Trim()).ToHashSet();
        }

        public void UpdateMacs(string macData)
        {
            if (AccountEntity == null)
                return;

            if (!string.IsNullOrWhiteSpace(macData))
                AccountEntity.Macs += "," + macData;
        }

        public bool HasBannedMac()
        {
            var macs = GetMac();
            if (macs.Count == 0)
            {
                return false;
            }
            using var _dbContext = new DBContext();
            return _dbContext.Macbans.Any(x => macs.Contains(x.Mac));
        }

        public bool HasBannedHWID()
        {
            if (Hwid == null)
            {
                return false;
            }
            using var dbContext = new DBContext();
            return dbContext.Hwidbans.Any(x => x.Hwid.Contains(Hwid.hwid));
        }

        public bool CheckChar(int accid)
        {
            // issue with multiple chars from same account login found by shavit, resinate
            if (!YamlConfig.config.server.USE_CHARACTER_ACCOUNT_CHECK)
            {
                return true;
            }

            foreach (var w in Server.getInstance().getWorlds())
            {
                foreach (IPlayer chr in w.getPlayerStorage().GetAllOnlinedPlayers())
                {
                    if (accid == chr.getAccountID())
                    {
                        log.LogWarning("Chr {CharacterName} has been removed from world {WorldName}. Possible Dupe attempt.", chr.getName(), w.Name);
                        chr.getClient().forceDisconnect();
                        return false;
                    }
                }
            }
            return true;
        }

        public bool isLoggedIn()
        {
            return IsOnlined;
        }

        public void CommitAccount()
        {
            if (AccountEntity == null)
                return;

            using (var dbContext = new DBContext())
            {
                dbContext.Accounts.Attach(AccountEntity).State = EntityState.Modified;
                dbContext.SaveChanges();
            }
        }

        public void SetCharacterOnSessionTransitionState(int cid)
        {
            this.updateLoginState(AccountStage.LOGIN_SERVER_TRANSITION);
            CurrentServer.SetCharacteridInTransition(this, cid);
        }

        public void SendCharList()
        {
            this.sendPacket(LoginPacketCreator.GetCharListPacket(this));
        }

        /// <summary>
        /// 选择角色时加载的角色
        /// </summary>
        /// <param name="worldId"></param>
        /// <returns></returns>
        public List<IPlayer> LoadCharacters()
        {
            return Server.getInstance().LoadAccountCharList(AccountEntity!.Id,  1).GetValueOrDefault(0, []);
        }
    }
}
