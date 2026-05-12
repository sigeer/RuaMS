using Application.Utility.Tasks;

namespace Application.Core.Login.Tasks
{
    internal class DisconnectIdlesOnLoginStateTask : ActorAsyncTask<MasterServer>
    {
        public DisconnectIdlesOnLoginStateTask(MasterServer actor) : base(actor, nameof(DisconnectIdlesOnLoginStateTask), TimeSpan.FromMinutes(5))
        {
        }

        protected override Task HandleRun()
        {
            return _actor.DisconnectIdlesOnLoginState();
        }
    }
}
