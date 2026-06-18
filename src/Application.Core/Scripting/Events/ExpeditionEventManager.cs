using Application.Core.Channel;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Core.Scripting.Events
{
    public class ExpeditionEventManager : BehindPartyQuestEventManager
    {
        public override AbstractExpeditionEventTemplate GetTemplate => (Template as AbstractExpeditionEventTemplate)!;
        public ExpeditionEventManager(WorldChannel cserv, AbstractExpeditionEventTemplate template) : base(cserv, template)
        {
        }

        protected override AbstractEventInstanceManager CreateNewInstance(string instanceName)
        {
            return new ExpeditionEventInstanceManager(this, instanceName);
        }



    }
}
