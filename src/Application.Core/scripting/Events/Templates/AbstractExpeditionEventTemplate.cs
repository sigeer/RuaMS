using Application.Core.Channel;
using Application.Core.Scripting.Events;

namespace Application.Core.scripting.Events.Templates
{
    public abstract class AbstractExpeditionEventTemplate : AbstractBehindPartyQuestEventTemplate
    {
        public int BossId { get; }
        public AbstractExpeditionEventTemplate(string name, int bossId) : base(name)
        {
            BossId = bossId;
        }

        public override AbstractEventManager GenerateEventManager(WorldChannel worldChannel)
        {
            return new ExpeditionEventManager(worldChannel, this);
        }
    }
}
