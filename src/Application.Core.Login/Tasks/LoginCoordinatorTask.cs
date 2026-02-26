using Application.Core.Login.Session;
using Application.Utility.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Core.Login.Tasks
{
    public class LoginCoordinatorTask : AbstractRunnable
    {
        readonly SessionCoordinator _sessionCoordinator;

        public LoginCoordinatorTask(MasterServer server) : base($"{server.InstanceName}_{nameof(LoginCoordinatorTask)}")
        {
            _sessionCoordinator = server.ServiceProvider.GetRequiredService<SessionCoordinator>();
        }

        public override void HandleRun()
        {
            _sessionCoordinator.clearExpiredHwidHistory();
        }
    }

}
