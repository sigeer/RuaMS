using Application.Utility.Pipeline;

namespace Application.Core.Login.Commands
{
    public class MasterCommandContext : ICommandContext
    {
        public MasterCommandContext(MasterServer server)
        {
            Server = server;
        }

        public MasterServer Server { get; }
    }
}
