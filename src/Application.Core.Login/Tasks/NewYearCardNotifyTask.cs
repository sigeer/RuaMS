using Application.Utility.Tasks;

namespace Application.Core.Login.Tasks
{
    internal class NewYearCardNotifyTask : ActorAsyncTask<MasterServer>
    {
        public NewYearCardNotifyTask(MasterServer actor) : base(actor, nameof(NewYearCardNotifyTask), TimeSpan.FromHours(1))
        {
        }

        protected override Task HandleRun()
        {
            return _actor.NewYearCardManager.NotifyNewYearCard();
        }
    }
}
