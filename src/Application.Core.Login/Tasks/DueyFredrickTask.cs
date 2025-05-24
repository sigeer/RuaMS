using Application.Utility.Tasks;
using client.processor.npc;

namespace Application.Core.Login.Tasks
{
    public class DueyFredrickTask : AbstractRunnable
    {
        private FredrickProcessor fredrickProcessor;

        public DueyFredrickTask(FredrickProcessor fredrickProcessor)
        {
            this.fredrickProcessor = fredrickProcessor;
        }

        public override void HandleRun()
        {
            fredrickProcessor.runFredrickSchedule();
            DueyProcessor.runDueyExpireSchedule();
        }
    }

}
