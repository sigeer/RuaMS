using Application.Core.Login.Session;
using Application.Utility.Tasks;

namespace Application.Core.Login.Tasks
{
    public class LoginStorageTask : AbstractRunnable
    {

        readonly SessionCoordinator sessionCoordinator;
        readonly LoginBypassCoordinator loginBypassCoordinator;

        public LoginStorageTask(SessionCoordinator sessionCoordinator, LoginBypassCoordinator loginBypassCoordinator)
        {
            this.sessionCoordinator = sessionCoordinator;
            this.loginBypassCoordinator = loginBypassCoordinator;
        }

        public override void HandleRun()
        {
            sessionCoordinator.runUpdateLoginHistory();
            loginBypassCoordinator.runUpdateLoginBypass();
        }
    }
}
