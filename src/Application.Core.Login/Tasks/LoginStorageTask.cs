using Application.Core.Login.Session;
using Application.Utility.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Core.Login.Tasks
{
    public class LoginStorageTask : AbstractRunnable
    {

        readonly SessionCoordinator sessionCoordinator;
        readonly LoginBypassCoordinator loginBypassCoordinator;

        public LoginStorageTask(MasterServer server): base($"{server.InstanceName}_{nameof(LoginStorageTask)}")
        {
            this.sessionCoordinator = server.ServiceProvider.GetRequiredService<SessionCoordinator>();
            this.loginBypassCoordinator = server.ServiceProvider.GetRequiredService<LoginBypassCoordinator>();
        }

        public override void HandleRun()
        {
            sessionCoordinator.runUpdateLoginHistory();
            loginBypassCoordinator.runUpdateLoginBypass();
        }
    }
}
