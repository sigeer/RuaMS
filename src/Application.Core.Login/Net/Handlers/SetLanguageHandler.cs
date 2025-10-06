using Application.Core.Login;
using Application.Core.Login.Client;
using Application.Core.Login.Net;
using Microsoft.Extensions.Logging;

namespace Application.Core.Channel.Net.Handlers
{
    public class SetLanguageHandler : LoginHandlerBase
    {
        public SetLanguageHandler(MasterServer server, ILogger<LoginHandlerBase> logger) : base(server, logger)
        {
        }

        public override void HandlePacket(InPacket p, ILoginClient c)
        {
            var lang = p.readByte();
            c.Language = lang;
        }
    }
}
