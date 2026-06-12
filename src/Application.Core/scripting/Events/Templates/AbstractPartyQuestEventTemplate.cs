using Application.Core.Channel;
using Application.Core.Scripting.Events;

namespace Application.Core.scripting.Events.Templates
{
    public abstract class AbstractPartyQuestEventTemplate : AbstractEventTemplate
    {
        public bool PartyLeaderRequired { get; init; }
        public int RecruitMap { get; init; }

        public AbstractPartyQuestEventTemplate(string name) : base(name)
        {
        }

        public override AbstractEventManager GenerateEventManager(WorldChannel worldChannel)
        {
            return new PartyQuestEventManager(worldChannel, this);
        }
    }
}
