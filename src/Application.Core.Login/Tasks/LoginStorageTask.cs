using Application.Core.Login.Session;
using Application.Utility.Tasks;
using net.server.coordinator.login;

namespace Application.Core.Login.Tasks
{
    public class LoginStorageTask : AbstractRunnable
    {

        readonly SessionCoordinator sessionCoordinator;

        public LoginStorageTask(SessionCoordinator sessionCoordinator)
        {
            this.sessionCoordinator = sessionCoordinator;
        }

        public override void HandleRun()
        {
            sessionCoordinator.runUpdateLoginHistory();
            LoginBypassCoordinator.getInstance().runUpdateLoginBypass();
        }
    }
}
