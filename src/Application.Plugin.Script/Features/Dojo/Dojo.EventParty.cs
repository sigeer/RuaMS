using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.Game.Players;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Features.Dojo
{
    internal sealed class EventParty : AbstractPartyQuestEventTemplate
    {
        public EventParty() : base(DojoFeature.PartyEventName)
        {
            MinLevel = 25;
            MaxLevel = 255;

            MinCount = 1;
            MaxCount = 6;

            MaxLobbys = 5;

            EntryMap = 925030100;
            ExitMap = DojoFeature.ExitMap;
            RecruitMap = DojoFeature.RecruitMap;
            ClearMap = DojoFeature.ClearMap;

            MinMap = 925030100;
            MaxMap = 925039999;

            // 首关（第 1 层）限时；后续每层进入时会按层数重新计时
            EventTime = 5 * 60;
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

            var minLevel = members.Min(x => x.Level);
            var maxLevel = members.Max(x => x.Level);

            if (members.Count >= MinCount
                && members.Count <= MaxCount
                && maxLevel - minLevel <= 30
                && members.All(x => x.Level >= MinLevel && x.Level <= MaxLevel))
            {
                return members;
            }
            return [];
        }

        public override Task OnPlayerEntry(AbstractEventInstanceManager eim, Player chr)
        {
            chr.setDojoEnergy(0);
            return base.OnPlayerEntry(eim, chr);
        }

        public override string? HandleCreateInstanceResult(CreateInstanceResult r, IChannelClient c)
        {
            return DojoFeature.HandleCreateInstanceResult(r, c);
        }

        public override async Task OnMobRevive(AbstractEventInstanceManager eim, Monster mob)
        {
            await base.OnMobRevive(eim, mob);

            if (mob.getId() == DojoFeature.HiddenMob)
            {
                await OnMapCompleted(eim, mob.MapModel);

                if (DojoFeature.GetStage(mob.MapModel.Id) == DojoFeature.MaxStage)
                {
                    await eim.clearPQ();
                }
            }
        }

    }
}
