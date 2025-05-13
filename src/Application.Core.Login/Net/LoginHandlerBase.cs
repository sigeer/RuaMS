using Application.Core.Client;
using Application.Core.Login.Database;
using Application.Core.Login.Net.Packets;
using Application.Core.Net;
using Application.Core.Servers;
using Microsoft.Extensions.Logging;
using net.packet;
using static net.server.coordinator.session.SessionCoordinator;

namespace Application.Core.Login.Net
{
    public abstract class LoginHandlerBase : ILoginHandler
    {
        protected readonly IMasterServer _server;
        protected readonly AccountManager _accountManager;
        protected readonly ILogger<LoginHandlerBase> _logger;

        protected LoginHandlerBase(IMasterServer server, AccountManager accountManager, ILogger<LoginHandlerBase> logger)
        {
            _server = server;
            _accountManager = accountManager;
            _logger = logger;
        }

        public abstract void HandlePacket(InPacket p, ILoginClient c);
        public virtual bool ValidateState(ILoginClient c)
        {
            return c.IsOnlined;
        }

        protected static int ParseAntiMulticlientError(AntiMulticlientResult res)
        {
            return (res) switch
            {
                AntiMulticlientResult.REMOTE_PROCESSING => 10,
                AntiMulticlientResult.REMOTE_LOGGEDIN => 7,
                AntiMulticlientResult.REMOTE_NO_MATCH => 17,
                AntiMulticlientResult.COORDINATOR_ERROR => 8,
                _ => 9
            };
        }
    }
}
