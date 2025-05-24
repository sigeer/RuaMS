using Application.Core.Login.Session;
using Application.Utility.Tasks;

namespace Application.Core.Login.Tasks
{
    public class LoginCoordinatorTask : AbstractRunnable
    {
        readonly SessionCoordinator _sessionCoordinator;

        public LoginCoordinatorTask(SessionCoordinator sessionCoordinator)
        {
            _sessionCoordinator = sessionCoordinator;
        }

        public override void HandleRun()
        {
            _sessionCoordinator.clearExpiredHwidHistory();
        }
    }

}
