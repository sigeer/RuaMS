using Application.Core.Channel;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Core.Scripting.Events;

namespace Application.Core.scripting.Events
{
    public class AriantEventManager : BehindPartyQuestEventManager
    {
        public AriantEventManager(WorldChannel cserv, AbstractBehindPartyQuestEventTemplate template) : base(cserv, template)
        {
        }
        protected override AbstractEventInstanceManager CreateNewInstance(string instanceName)
        {
            return new AriantEventInstanceManager(this, instanceName);
        }
    }
}
