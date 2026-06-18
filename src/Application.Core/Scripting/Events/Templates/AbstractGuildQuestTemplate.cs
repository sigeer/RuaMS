using Application.Core.Channel;
using Application.Core.Scripting.Events;

namespace Application.Core.scripting.Events.Templates
{
    public abstract class AbstractGuildQuestTemplate : AbstractSoloEventTemplate
    {
        protected AbstractGuildQuestTemplate(string name) : base(name)
        {
        }

        public override AbstractEventManager GenerateEventManager(WorldChannel worldChannel)
        {
            return new GuildQuestEventManager(worldChannel, this);
        }
    }
}
