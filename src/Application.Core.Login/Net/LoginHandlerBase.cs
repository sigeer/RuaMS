using Application.Core.Login.Client;
using Application.Shared.Sessions;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Net
{
    public abstract class LoginHandlerBase : ILoginHandler
    {
        protected readonly MasterServer _server;
        protected readonly ILogger<LoginHandlerBase> _logger;

        protected LoginHandlerBase(MasterServer server, ILogger<LoginHandlerBase> logger)
        {
            _server = server;
            _logger = logger;
        }

        public abstract Task HandlePacket(InPacket p, ILoginClient c);
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
