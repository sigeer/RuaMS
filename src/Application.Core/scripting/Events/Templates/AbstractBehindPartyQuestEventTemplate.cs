using Application.Core.Channel;
using Application.Core.scripting.Events.Instances;
using Application.Core.Scripting.Events;

namespace Application.Core.scripting.Events.Templates
{
    public abstract class AbstractBehindPartyQuestEventTemplate : AbstractPartyQuestEventTemplate
    {
        protected AbstractBehindPartyQuestEventTemplate(string name) : base(name)
        {
            PartyLeaderRequired = false;
            MaxLobbys = 1;
        }

        public int RegistrationTime { get; init; }
        public int PrepareTime { get; init; }

        public virtual void OnBattlePrepare(AbstractEventInstanceManager eim)
        {

        }

        public virtual void OnBattleStarted(AbstractEventInstanceManager eim)
        {

        }

        public virtual void OnPlayerBanned(AbstractEventInstanceManager eim, Player chr)
        {

        }

        public virtual void OnPlayerJoined(AbstractEventInstanceManager eim, Player chr)
        {

        }
    }
}
