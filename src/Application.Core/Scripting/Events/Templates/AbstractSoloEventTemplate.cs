using Application.Core.Channel;
using Application.Core.scripting.Events.Instances;
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
        public override List<Player> GetEligibleParty(Player leader)
        {
            List<Player> members = [leader];

            if (members.Count >= MinCount
                && members.Count <= MaxCount
                && members.All(x => x.Level >= MinLevel && x.Level <= MaxLevel))
            {
                return members;
            }
            return [];
        }
        public override bool IsEventTeamLackingNow(AbstractEventInstanceManager eim, bool leavingEventMap, Player quitter)
        {
            return leavingEventMap && eim.getLeaderId() == quitter.getId();
        }
    }
}