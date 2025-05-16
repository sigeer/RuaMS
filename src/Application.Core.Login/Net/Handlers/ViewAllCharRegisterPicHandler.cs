using Application.Core.Client;
using Application.Core.Login.Datas;
using Application.Core.Login.Session;
using Application.Core.Servers;
using Application.Shared.Sessions;
using Microsoft.Extensions.Logging;
using net.packet;
using net.server.coordinator.session;
using tools;

namespace Application.Core.Login.Net.Handlers;


public class ViewAllCharRegisterPicHandler : LoginHandlerBase
{
    readonly SessionCoordinator _sessionCoordinator;
    public ViewAllCharRegisterPicHandler(IMasterServer server, AccountManager accountManager, ILogger<LoginHandlerBase> logger, SessionCoordinator sessionCoordinator)
        : base(server, accountManager, logger)
    {
        _sessionCoordinator = sessionCoordinator;

    }

    public override void HandlePacket(InPacket p, ILoginClient c)
    {
        p.readByte();
        int charId = p.readInt();
        p.readInt(); // please don't let the client choose which world they should login

        string mac = p.readString();
        string hostString = p.readString();

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

        c.UpdateMacs(mac);
        c.Hwid = hwid;

        if (c.HasBannedMac() || c.HasBannedHWID())
        {
            _sessionCoordinator.closeSession(c, true);
            return;
        }

        AntiMulticlientResult res = _sessionCoordinator.attemptGameSession(c, c.AccountEntity!.Id, hwid);
        if (res != AntiMulticlientResult.SUCCESS)
        {
            c.sendPacket(PacketCreator.getAfterLoginError(ParseAntiMulticlientError(res)));
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


        string pic = p.readString();
        if (string.IsNullOrEmpty(c.AccountEntity.Pic))
        {
            c.AccountEntity.Pic = pic;

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

            try
            {
                c.sendPacket(PacketCreator.getServerIP(socket, charId));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }
        else
        {
            _sessionCoordinator.closeSession(c, true);
        }
    }
}
