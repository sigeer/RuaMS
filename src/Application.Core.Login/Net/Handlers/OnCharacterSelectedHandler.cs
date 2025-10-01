using Application.Core.Login.Client;
using Application.Core.Login.Net.Packets;
using Application.Core.Login.Session;
using Application.Shared.Sessions;
using Application.Utility;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Net.Handlers
{
    public abstract class OnCharacterSelectedHandler : LoginHandlerBase
    {
        readonly SessionCoordinator _sessionCoordinator;
        protected OnCharacterSelectedHandler(MasterServer server, ILogger<LoginHandlerBase> logger, SessionCoordinator sessionCoordinator)
            : base(server, logger)
        {
            _sessionCoordinator = sessionCoordinator;
        }


        protected virtual void Process(ILoginClient c, int charId, string hostString, string macs)
        {
            if (c.AccountEntity == null)
                return;

            Hwid hwid;
            try
            {
                hwid = Hwid.fromHostString(hostString);
            }
            catch (ArgumentException e)
            {
                _logger.LogWarning(e, "Invalid host string: {Host}", hostString);
                c.sendPacket(LoginPacketCreator.getAfterLoginError(17));
                return;
            }


            if (_server.AccountBanManager.IsIPBlocked(c.RemoteAddress) || _server.AccountBanManager.IsMACBlocked(macs) || _server.AccountBanManager.IsHWIDBlocked(hwid.hwid))
            {
                _sessionCoordinator.closeSession(c, true);
                return;
            }

            AntiMulticlientResult res = _sessionCoordinator.attemptGameSession(c, c.AccountEntity.Id, hwid);
            if (res != AntiMulticlientResult.SUCCESS)
            {
                c.sendPacket(LoginPacketCreator.getAfterLoginError(ParseAntiMulticlientError(res)));
                return;
            }


            if (!_server.AccountManager.ValidAccountCharacter(c.AccountEntity.Id, charId))
            {
                _sessionCoordinator.closeSession(c, true);
                return;
            }

            if (_server.IsWorldCapacityFull())
            {
                c.sendPacket(LoginPacketCreator.getAfterLoginError(10));
                return;
            }

            c.SelectedChannel = c.SelectedChannel < 1 ? Randomizer.rand(1, _server.ChannelServerList.Count) : c.SelectedChannel;

            var socket = _server.GetChannelIPEndPoint(c.SelectedChannel);
            if (socket == null)
            {
                c.sendPacket(LoginPacketCreator.getAfterLoginError(10));
                return;
            }

            _server.UnregisterLoginState(c);
            c.SetCharacterOnSessionTransitionState(charId);

            c.AccountEntity!.CurrentIP = c.RemoteAddress;
            c.AccountEntity!.CurrentMac = macs;
            c.AccountEntity!.CurrentHwid = hwid.hwid;
            _server.AccountHistoryManager.InsertLoginHistory(c.AccountId, c.RemoteAddress, macs, hwid.hwid);
            _server.AccountManager.SetClientLanguage(c.AccountId, c.Language);

            try
            {
                c.sendPacket(LoginPacketCreator.getServerIP(socket, charId));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }
    }

    public abstract class OnCharacterSelectedWithPicHandler : OnCharacterSelectedHandler
    {
        protected OnCharacterSelectedWithPicHandler(MasterServer server, ILogger<LoginHandlerBase> logger, SessionCoordinator sessionCoordinator)
            : base(server, logger, sessionCoordinator)
        {
        }


        protected virtual void Process(ILoginClient c, int charId, string pic, string hostString, string macs)
        {
            if (!c.CheckPic(pic))
            {
                c.sendPacket(LoginPacketCreator.WrongPic());
                return;
            }


            base.Process(c, charId, hostString, macs);
        }
    }
}
