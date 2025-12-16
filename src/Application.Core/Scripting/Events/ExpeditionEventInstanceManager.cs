using Application.Core.Game.Life;
using server.expeditions;

namespace Application.Core.Scripting.Events
{
    public class ExpeditionEventInstanceManager : AbstractEventInstanceManager
    {
        private Expedition? expedition = null;
        public ExpeditionEventInstanceManager(AbstractInstancedEventManager em, string name) : base(em, name)
        {
            Type = Shared.Events.EventInstanceType.Expedition;
        }
        protected override void OnMonsterValueChanged(IPlayer chr, Monster mob, int val)
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

                Monitor.Enter(scriptLock);
                try
                {
                    expedition.removeChannelExpedition(EventManager.getChannelServer());
                }
                finally
                {
                    Monitor.Exit(scriptLock);
                }

                expedition = null;
            }
        }

        public void registerExpedition(Expedition exped)
        {
            expedition = exped;

            foreach (IPlayer chr in exped.getActiveMembers())
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
