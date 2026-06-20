using Application.Core.Channel;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Core.Scripting.Events
{
    public class MonsterCarnivalEventManager : PartyQuestEventManager
    {
        public override AbstractMonsterCarnivalEventTemplate GetTemplate { get; }
        public MonsterCarnivalEventManager(WorldChannel cserv, AbstractMonsterCarnivalEventTemplate template) : base(cserv, template)
        {
            GetTemplate = template;
        }

        protected override AbstractEventInstanceManager CreateNewInstance(string instanceName)
        {
            return new MonsterCarnivalEventInstanceManager(this, instanceName);
        }

        public TEim? GetOnlyEventInstanceManager<TEim>() where TEim : MonsterCarnivalEventInstanceManager => getInstance(Name) as TEim;
        //public bool Check2(MonsterCarnivalEventInstanceManager eim)
        //{
        //    try
        //    {
        //        var t0 = iv.CallFunction("getEligibleParty", eim.Team0.EligibleMembers, eim.Room, 1).ToObject<List<Player>>() ?? [];
        //        var t1 = iv.CallFunction("getEligibleParty", eim.Team1.EligibleMembers, eim.Room, 1).ToObject<List<Player>>() ?? [];

        //        return t0.Count == t1.Count && t0.Count >= eim.Room.MinCount;
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error(ex, "Script: {Script}", _name);
        //        return false;
        //    }
        //}




        public override void SetupInstance(AbstractEventInstanceManager eim, Player leader, List<Player> members)
        {
            var pEim = (eim as MonsterCarnivalEventInstanceManager)!;
            pEim.RegisterParty(members);
            base.SetupInstance(eim, leader, members);
        }

        public async Task<MCJoinInstanceResult> SendJoinRequest(Player chr)
        {
            var eim = getInstance(Name) as MonsterCarnivalEventInstanceManager;
            if (eim == null)
                return MCJoinInstanceResult.Unknown;

            if (chr.Party == 0)
                return MCJoinInstanceResult.RequiredParty;

            if (!chr.isLeader())
                return MCJoinInstanceResult.RequiredLeader;

            var members = Template.GetEligibleParty(chr);
            if (members.Count == 0)
                return MCJoinInstanceResult.Requirement;

            if (eim.InstanceStatus != InstanceStatus.Recruitment)
                return MCJoinInstanceResult.NotInWaiting;

            if (eim.RequestTeam != null)
                return MCJoinInstanceResult.AnthorRequest;

            eim.RequestTeam = new TeamRegistry(members);
            // send challenge
            if (await ChannelServer.NodeService.PluginManager.StartNpcConversation(
                eim.getLeader()!.Client,
                2042001,
                null,
                "mc_enter1"))
            {
                return MCJoinInstanceResult.Success;
            }
            return MCJoinInstanceResult.AnthorRequest;
        }

    }
}
