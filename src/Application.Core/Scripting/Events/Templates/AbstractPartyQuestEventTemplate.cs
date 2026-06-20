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

        public override List<Player> GetEligibleParty(Player leader)
        {
            var party = leader.getParty();
            if (party == null)
            {
                return [];
            }

            var members = party.GetChannelMembers(leader.Client.CurrentServer)
                .Where(x => x.MapModel == leader.MapModel && x.MapModel.Id == RecruitMap).ToList();

            if (members.Count >= MinCount
                && members.Count <= MaxCount
                && members.All(x => x.Level >= MinLevel && x.Level <= MaxLevel))
            {
                return members;
            }
            return [];
        }

    }
}
