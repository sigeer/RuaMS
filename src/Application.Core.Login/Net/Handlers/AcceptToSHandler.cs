using Application.Core.Login.Client;
using Application.Core.Login.Net.Packets;
using Application.Shared.Login;
using Microsoft.Extensions.Logging;
using net.packet;

namespace Application.Core.Login.Net.Handlers;

/**
 * @author kevintjuh93
 */
public class AcceptToSHandler : LoginHandlerBase
{
    public AcceptToSHandler(MasterServer server, ILogger<LoginHandlerBase> logger)
        : base(server, logger)
    {
    }

    public override bool ValidateState(ILoginClient c)
    {
        return !c.isLoggedIn();
    }

    public override void HandlePacket(InPacket p, ILoginClient c)
    {
        if (p.available() == 0 || p.readByte() != 1 || c.AccountEntity!.Tos)
        {
            c.Disconnect();//IClient dc's but just because I am cool I do this (:
            return;
        }
        if (c.FinishLogin() == LoginResultCode.Success)
        {
            c.sendPacket(LoginPacketCreator.GetAuthSuccess(c));
        }
        else
        {
            c.sendPacket(LoginPacketCreator.GetLoginFailed(9));//shouldn't happen XD
        }
    }
}
