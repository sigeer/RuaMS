using Application.Core.Scripting.Events;

namespace Application.Core.scripting.Events.Instances
{
    public class ExpeditionEventInstanceManager : BehindPartyQuestEventInstanceManager
    {
        public ExpeditionEventInstanceManager(ExpeditionEventManager em, string name) : base(em, name)
        {
        }
    }
}
