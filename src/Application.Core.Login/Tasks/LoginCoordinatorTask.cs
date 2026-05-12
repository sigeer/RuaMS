using Application.Core.Login.Session;
using Application.Utility.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Core.Login.Tasks
{
    public class LoginCoordinatorTask : ActorTask<MasterServer>
    {
        readonly SessionCoordinator _sessionCoordinator;

        public LoginCoordinatorTask(MasterServer server) : base(server, nameof(LoginCoordinatorTask), TimeSpan.FromHours(1))
        {
            _sessionCoordinator = server.ServiceProvider.GetRequiredService<SessionCoordinator>();
        }

        protected override void HandleRun()
        {
            _sessionCoordinator.clearExpiredHwidHistory();
        }
    }

}
