using Application.Core.Channel;
using Application.Core.Channel.Net.Packets;
using Application.Core.Channel.QuestRecordEx;
using Application.Core.scripting.Events.Instances;
using Application.Core.Scripting.Events;

namespace Application.Core.scripting.Events.Templates
{
    public abstract class AbstractPartyQuestEventTemplate : AbstractEventTemplate
    {
        public short QuestId { get; init; }
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

        public override async Task AfterSeup(AbstractEventInstanceManager eim)
        {
            await base.AfterSeup(eim);

            foreach (var chr in eim.getPlayers())
            {
                await StartPartyQuest(chr);
            }
        }

        public override async Task OnPlayerUnregister(AbstractEventInstanceManager eim, Player chr)
        {
            if (!eim.isEventCleared())
            {
                await AbortPartyQuest(chr);
            }
            await base.OnPlayerUnregister(eim, chr);
        }

        public override async Task ClearPQ(AbstractEventInstanceManager eim)
        {
            var now = eim.EventManager.ChannelServer.Node.getCurrentTime();
            foreach (var chr in eim.getPlayers())
            {
                await CompletePartyQuest(eim, chr, now);
            }

            await base.ClearPQ(eim);
        }
        protected virtual async Task StartPartyQuest(Player chr)
        {
            if (QuestId <= 0)
                return;

            if (chr.GetPartyQuestRecord(QuestId) is PartyQuestRecordEx model)
            {
                model.Try++;

                await model.Flush(chr);
            }

        }

        protected virtual Task AbortPartyQuest(Player chr)
        {
            return Task.CompletedTask;
        }
        protected virtual async Task CompletePartyQuest(AbstractEventInstanceManager eim, Player chr, long now)
        {
            if (QuestId <= 0)
                return;

            if (chr.GetPartyQuestRecord(QuestId) is PartyQuestRecordEx model)
            {
                model.Cmp++;

                var cost = now - eim.InstanceStartTime;
                if (model.TotalCost <= 0)
                {
                    model.TotalCost = cost;
                    model.CompleteTime = now;
                }
                else if (cost < model.TotalCost)
                {
                    model.TotalCost = cost;
                    model.CompleteTime = now;
                }

                await model.Flush(chr);
            }
        }
    }
}
