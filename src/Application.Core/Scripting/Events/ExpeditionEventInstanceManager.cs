using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Abstraction;
using server.expeditions;

namespace Application.Core.Scripting.Events
{
    public class ExpeditionEventInstanceManager : AbstractEventInstanceManager
    {
        private Expedition? expedition = null;
        public ExpeditionEventInstanceManager(WorldChannel worldChannel, string emName, string name) : base(worldChannel, emName, name)
        {
            Type = Shared.Events.EventInstanceType.Expedition;
        }
        protected override void OnMonsterValueChanged(Player chr, Monster mob, int val)
        {
            expedition?.monsterKilled(chr, mob);
        }

        public override void setEventCleared()
        {
            base.setEventCleared();
            disposeExpedition();
        }

        private void disposeExpedition()
        {
            if (expedition != null)
            {
                expedition.dispose(eventCleared);

                expedition.removeChannelExpedition(EventManager.ChannelServer);

                expedition = null;
            }
        }

        public void registerExpedition(Expedition exped)
        {
            expedition = exped;

            foreach (Player chr in exped.getActiveMembers())
            {
                if (chr.getMapId() == exped.getRecruitingMap().getId())
                {
                    registerPlayer(chr);
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            disposeExpedition();
        }
    }
}
