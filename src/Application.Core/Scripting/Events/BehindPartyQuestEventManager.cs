using Application.Core.Channel;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Core.Scripting.Events
{
    /// <summary>
    /// 在启动之后才组建团队
    /// </summary>
    public class BehindPartyQuestEventManager : PartyQuestEventManager
    {
        public int RegistrationTime => GetTemplate.RegistrationTime;
        public int PrepareTime => GetTemplate.PrepareTime;
        public override AbstractBehindPartyQuestEventTemplate GetTemplate { get; }
        public BehindPartyQuestEventManager(WorldChannel cserv, AbstractBehindPartyQuestEventTemplate template) : base(cserv, template)
        {
            GetTemplate = template;
        }

        public TEim? GetOnlyEventInstanceManager<TEim>() where TEim : BehindPartyQuestEventInstanceManager => getInstance(Name) as TEim;



        public override async Task<AbstractEventInstanceManager> Setup(int level, int lobbyId)
        {
            var eim = newInstance(Name + lobbyId);
            eim.setProperty("level", level);

            await Template.OnSetup(eim, level, lobbyId);

            await Template.respawnStages(eim);

            await eim.startEventTimer(RegistrationTime * 1000);

            return eim;
        }
    }
}
