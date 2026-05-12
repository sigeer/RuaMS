using Application.Core.Login.Session;
using Application.Utility.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Core.Login.Tasks
{
    public class LoginStorageTask : ActorTask<MasterServer>
    {

        readonly SessionCoordinator sessionCoordinator;
        readonly LoginBypassCoordinator loginBypassCoordinator;

        public LoginStorageTask(MasterServer server): base(server, nameof(LoginStorageTask), TimeSpan.FromMinutes(2))
        {
            this.sessionCoordinator = server.ServiceProvider.GetRequiredService<SessionCoordinator>();
            this.loginBypassCoordinator = server.ServiceProvider.GetRequiredService<LoginBypassCoordinator>();
        }

        protected override void HandleRun()
        {
            sessionCoordinator.runUpdateLoginHistory();
            loginBypassCoordinator.runUpdateLoginBypass();
        }
    }
}
