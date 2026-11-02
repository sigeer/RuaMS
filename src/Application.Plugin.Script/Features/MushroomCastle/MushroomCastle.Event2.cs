using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Features.MushroomCastle
{
    internal sealed class Event2 : AbstractPartyQuestEventTemplate
    {
        const int PrimeMinisterMobId = 3300008;

        public Event2() : base(MushroomCastleFeature.Event2Name)
        {
            MinCount = 1;
            MaxCount = 3;
            MinLevel = 30;
            MaxLevel = 255;

            RecruitMap = 106021402;
            EntryMap = 106021601;
            ExitMap = 106021402;
            ClearMap = 106021402;

            MinMap = 106021601;
            MaxMap = 106021601;

            EventTime = 10 * 60;
            Type = Shared.Events.EventInstanceType.PartyQuest;
        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            eim.Level = level;
            eim.setProperty("level", level);

            var map = await eim.getInstanceMap(106021601);
            if (map != null)
            {
                var mob = Application.Core.Server.life.LifeFactory.Instance.GetMonsterTrust(PrimeMinisterMobId);
                await map.spawnMonsterOnGroundBelow(mob, new System.Drawing.Point(292, 143));
            }
        }

        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            if (mob.getId() != PrimeMinisterMobId)
                return;

            await eim.showClearEffect(mob.getMap().getId());
            await eim.clearPQ();
        }


    }
}
