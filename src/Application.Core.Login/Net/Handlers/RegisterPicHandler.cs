using Application.Core.Login.Client;
using Application.Core.Login.Session;
using Application.Shared.Sessions;
using Microsoft.Extensions.Logging;
using net.packet;
using net.server.coordinator.session;
using tools;

namespace Application.Core.Login.Net.Handlers;




public class RegisterPicHandler : LoginHandlerBase
{
    readonly SessionCoordinator sessionCoordinator;
    public RegisterPicHandler(MasterServer server, SessionCoordinator sessionCoordinator, ILogger<LoginHandlerBase> logger)
        : base(server, logger)
    {
        this.sessionCoordinator = sessionCoordinator;
    }

    public override void HandlePacket(InPacket p, ILoginClient c)
    {
        p.readByte();
        int charId = p.readInt();

        string macs = p.readString();
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

        if (c.AccountEntity == null)
        {
            // 登录失败
            c.sendPacket(PacketCreator.getAfterLoginError(0));
            return;
        }

        c.UpdateMacs(macs);
        c.Hwid = hwid;

        AntiMulticlientResult res = sessionCoordinator.attemptGameSession(c, c.AccountEntity.Id, hwid);
        if (res != AntiMulticlientResult.SUCCESS)
        {
            c.sendPacket(PacketCreator.getAfterLoginError(ParseAntiMulticlientError(res)));
            return;
        }

        if (c.HasBannedMac() || c.HasBannedHWID())
        {
            sessionCoordinator.closeSession(c, true);
            return;
        }

        if (!_server.AccountManager.ValidAccountCharacter(c.AccountEntity.Id, charId))
        {
            sessionCoordinator.closeSession(c, true);
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
            sessionCoordinator.closeSession(c, true);
        }
    }
}