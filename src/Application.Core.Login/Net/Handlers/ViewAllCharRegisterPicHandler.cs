using Application.Core.Login.Client;
using Application.Core.Login.Net.Packets;
using Application.Core.Login.Session;
using Application.Shared.Sessions;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Net.Handlers;


public class ViewAllCharRegisterPicHandler : LoginHandlerBase
{
    readonly SessionCoordinator _sessionCoordinator;
    public ViewAllCharRegisterPicHandler(MasterServer server, ILogger<LoginHandlerBase> logger, SessionCoordinator sessionCoordinator)
        : base(server, logger)
    {
        _sessionCoordinator = sessionCoordinator;

    }

    public override async Task HandlePacket(InPacket p, ILoginClient c)
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
            c.sendPacket(LoginPacketCreator.getAfterLoginError(17));
            return;
        }

        if (_server.AccountBanManager.IsIPBlocked(c.RemoteAddress) || _server.AccountBanManager.IsMACBlocked(mac) || _server.AccountBanManager.IsHWIDBlocked(hwid.hwid))
        {
            await _sessionCoordinator.closeSession(c, true);
            return;
        }

        AntiMulticlientResult res = _sessionCoordinator.attemptGameSession(c, c.AccountEntity!.Id, hwid);
        if (res != AntiMulticlientResult.SUCCESS)
        {
            c.sendPacket(LoginPacketCreator.getAfterLoginError(ParseAntiMulticlientError(res)));
            return;
        }

        if (!_server.AccountManager.ValidAccountCharacter(c.AccountEntity.Id, charId))
        {
            await _sessionCoordinator.closeSession(c, true);
            return;
        }

        if (_server.IsWorldCapacityFull())
        {
            c.sendPacket(LoginPacketCreator.getAfterLoginError(10));
            return;
        }


        string pic = p.readString();
        if (string.IsNullOrEmpty(c.AccountEntity.Pic))
        {
            c.AccountEntity.Pic = pic;
            c.CurrentServer.CommitAccountEntity(c.AccountEntity);

            if (_server.IsWorldCapacityFull())
            {
                c.sendPacket(LoginPacketCreator.getAfterLoginError(10));
                return;
            }

            var socket = _server.GetChannelIPEndPoint(c.SelectedChannel);
            if (socket == null)
            {
                c.sendPacket(LoginPacketCreator.getAfterLoginError(10));
                return;
            }

            _server.UnregisterLoginState(c);
            c.SetCharacterOnSessionTransitionState(charId);

            try
            {
                c.sendPacket(LoginPacketCreator.getServerIP(socket, charId));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }
        else
        {
            await _sessionCoordinator.closeSession(c, true);
        }
    }
}
