using Application.Core.Channel;
using Application.Core.Scripting.Events;

namespace Application.Core.scripting.Events.Templates
{
    public abstract class AbstractSoloEventTemplate : AbstractEventTemplate
    {
        public AbstractSoloEventTemplate(string name) : base(name)
        {
            MinCount = 1;
            MaxCount = 1;
        }

        public override AbstractEventManager GenerateEventManager(WorldChannel worldChannel)
        {
            return new SoloEventManager(worldChannel, this);
        }
    }
}