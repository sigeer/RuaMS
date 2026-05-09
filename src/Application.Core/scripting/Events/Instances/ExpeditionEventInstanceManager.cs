using Application.Core.Channel;
using Application.Core.scripting.Events.Abstraction;
using Application.Resources.Messages;
using tools;

namespace Application.Core.scripting.Events.Instances
{
    public class ExpeditionEventInstanceManager : BehindPartyQuestEventInstanceManager
    {
        public ExpeditionEventInstanceManager(WorldChannel worldChannel, string emName, string name) : base(worldChannel, emName, name)
        {
        }

    }
}
