using Application.Core.Channel;
using Application.Core.Scripting.Events;
using Application.Scripting;

namespace Application.Module.Marriage.Channel
{
    public class MarriageEventManager : AbstractInstancedEventManager
    {
        readonly IModuleChannelServerTransport _transport;
        public MarriageEventManager(WorldChannel cserv, IEngine iv, ScriptFile file, IModuleChannelServerTransport transport) : base(cserv, iv, file)
        {
            _transport = transport;
        }

        protected override AbstractEventInstanceManager CreateNewInstance(string instanceName)
        {
            return new MarriageInstance(this, instanceName, _transport);
        }
    }
}
