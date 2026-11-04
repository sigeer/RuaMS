using Application.Core.Game.Life;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Features.Dojo
{
    internal sealed class EventPre : AbstractSoloEventTemplate
    {
        public EventPre() : base(DojoFeature.PreEventName)
        {
            MinLevel = 25;
            MaxLevel = 255;

            MaxLobbys = 5;

            EntryMap = 925020010;

            ExitMap = DojoFeature.RecruitMap;

            MinMap = 925020010;
            MaxMap = 925020010;

            EventTime = 10 * 60;
        }

        public override async Task OnMobRevive(AbstractEventInstanceManager eim, Monster mob)
        {
            await base.OnMobRevive(eim, mob);

            if (mob.getId() == DojoFeature.HiddenMob)
            {
                foreach (var chr in eim.getPlayers())
                {
                    chr.FinishedDojoTutorial = true;
                }
                await eim.clearPQ();
            }
        }
    }
}
