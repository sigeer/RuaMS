using Application.Core.Client;
using Application.Core.Login.Datas;
using Application.Core.Login.Net.Packets;
using Application.Core.Login.Session;
using Application.Core.Servers;
using Application.Shared.Sessions;
using Application.Utility;
using Microsoft.Extensions.Logging;
using net.server.coordinator.session;
using tools;

namespace Application.Core.Login.Net.Handlers
{
    public abstract class OnCharacterSelectedHandler : LoginHandlerBase
    {
        readonly SessionCoordinator _sessionCoordinator;
        protected OnCharacterSelectedHandler(IMasterServer server, AccountManager accountManager, ILogger<LoginHandlerBase> logger, SessionCoordinator sessionCoordinator)
            : base(server, accountManager, logger)
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
                c.sendPacket(PacketCreator.getAfterLoginError(17));
                return;
            }

            c.UpdateMacs(macs);
            c.Hwid = hwid;

            AntiMulticlientResult res = _sessionCoordinator.attemptGameSession(c, c.AccountEntity.Id, hwid);
            if (res != AntiMulticlientResult.SUCCESS)
            {
                c.sendPacket(PacketCreator.getAfterLoginError(ParseAntiMulticlientError(res)));
                return;
            }

            if (c.HasBannedMac() || c.HasBannedHWID())
            {
                _sessionCoordinator.closeSession(c, true);
                return;
            }


            if (!_accountManager.ValidAccountCharacter(c.AccountEntity.Id, charId))
            {
                _sessionCoordinator.closeSession(c, true);
                return;
            }

            if (_server.IsWorldCapacityFull())
            {
                c.sendPacket(PacketCreator.getAfterLoginError(10));
                return;
            }

            var socket = _server.GetChannelIPEndPoint(c.SelectedChannel);
            if (socket == null)
            {
                c.sendPacket(PacketCreator.getAfterLoginError(10));
                return;
            }

            _server.UnregisterLoginState(c);
            c.SetCharacterOnSessionTransitionState(charId);
            c.CommitAccount();

            try
            {
                c.sendPacket(PacketCreator.getServerIP(socket, charId));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }
    }

    public abstract class OnCharacterSelectedWithPicHandler : OnCharacterSelectedHandler
    {
        protected OnCharacterSelectedWithPicHandler(IMasterServer server, AccountManager accountManager, ILogger<LoginHandlerBase> logger, SessionCoordinator sessionCoordinator)
            : base(server, accountManager, logger, sessionCoordinator)
        {
        }


        protected virtual void Process(ILoginClient c, int charId, string pic, string hostString, string macs)
        {
            if (!c.CheckPic(pic))
            {
                c.sendPacket(LoginPacketCreator.WrongPic());
                return;
            }

            c.SelectedChannel = Randomizer.rand(1, _server.ChannelServerList.Count);
            base.Process(c, charId, hostString, macs);
        }
    }
}
