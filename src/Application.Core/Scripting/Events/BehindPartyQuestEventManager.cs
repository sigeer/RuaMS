using Application.Core.Channel;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Resources.Messages;
using Application.Templates;
using tools;

namespace Application.Core.Scripting.Events
{
    /// <summary>
    /// 在启动之后才组建团队
    /// </summary>
    public class BehindPartyQuestEventManager : PartyQuestEventManager
    {
        public int RegistrationTime => GetTemplate.RegistrationTime;
        public int PrepareTime => GetTemplate.PrepareTime;
        public override AbstractBehindPartyQuestEventTemplate GetTemplate => (Template as AbstractBehindPartyQuestEventTemplate)!;
        public BehindPartyQuestEventManager(WorldChannel cserv, AbstractBehindPartyQuestEventTemplate template) : base(cserv, template)
        {
        }

        public TEim? GetOnlyEventInstanceManager<TEim>() where TEim : BehindPartyQuestEventInstanceManager => getInstance(Name) as TEim;



        public override AbstractEventInstanceManager Setup(int level, int lobbyId)
        {
            var eim = newInstance(Name + lobbyId);
            eim.setProperty("level", level);

            Template.OnSetup(eim, level, lobbyId);

            Template.respawnStages(eim);

            eim.startEventTimer(RegistrationTime * 1000);

            return eim;
        }
    }
}
