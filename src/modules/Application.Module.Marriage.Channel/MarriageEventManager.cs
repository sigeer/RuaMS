using Application.Core.Channel;
using Application.Core.scripting.Events.Instances;
using Application.Core.Scripting.Events;
using Application.Scripting;

namespace Application.Module.Marriage.Channel
{
    public class MarriageEventManager : AbstractEventManager<MarriageEventTemplate>
    {
        readonly IModuleChannelServerTransport _transport;
        public override MarriageEventTemplate Template { get; }

        public MarriageEventManager(WorldChannel cserv, MarriageEventTemplate template, IModuleChannelServerTransport transport) : base(cserv, template)
        {
            Template = template;
            _transport = transport;
        }

        protected override AbstractEventInstanceManager CreateNewInstance(string instanceName)
        {
            return new MarriageInstance(this, instanceName, _transport);
        }
    }
}
