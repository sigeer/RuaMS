using Application.Utility.Pipeline;

namespace Application.Core.Login.Commands
{
    public class MasterCommandLoop : CommandLoop<MasterCommandContext>
    {
        protected MasterServer _server;
        public MasterCommandLoop(MasterServer server) : base()
        {
            _server = server;
        }

        protected override MasterCommandContext CreateContext()
        {
            return new MasterCommandContext(_server);
        }
    }
}
