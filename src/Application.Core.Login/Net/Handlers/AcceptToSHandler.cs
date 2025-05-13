using Application.Core.Client;
using Application.Core.Login.Database;
using Application.Core.Login.Net.Packets;
using Application.Core.Servers;
using Application.Shared.Login;
using Microsoft.Extensions.Logging;
using net.packet;

namespace Application.Core.Login.Net.Handlers;

/**
 * @author kevintjuh93
 */
public class AcceptToSHandler : LoginHandlerBase
{
    public AcceptToSHandler(IMasterServer server, AccountManager accountManager, ILogger<LoginHandlerBase> logger)
        : base(server, accountManager, logger)
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
