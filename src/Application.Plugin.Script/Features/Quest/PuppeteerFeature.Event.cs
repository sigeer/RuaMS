using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Core.Server.life;
using System.Drawing;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class PuppeteerFeatureEvent : AbstractSoloEventTemplate
    {
        public PuppeteerFeatureEvent() : base(PuppeteerFeature.EventName)
        {
            EventTime = 10 * 60;
            EntryMap = 910510000;
            ExitMap = 105070300;
            MinMap = 910510000;
            MaxMap = 910510000;
        }

        public override async Task AfterSeup(AbstractEventInstanceManager eim)
        {
            var mapObj = await eim.getMapInstance(EntryMap);
            var chr = eim.getLeader()!;
            if (chr.getQuestStatus(PuppeteerFeature.PuppeteerCygnus) == 1 && chr.GetQuestProgressInt(PuppeteerFeature.PuppeteerCygnus, 9300285) == 0)
            {
                await mapObj.spawnMonsterOnGroundBelow(LifeFactory.Instance.GetMonsterTrust(9300285), new Point(680, 258));
            }

            if (chr.getQuestStatus(PuppeteerFeature.PuppeteerAran) == 1 && chr.GetQuestProgressInt(PuppeteerFeature.PuppeteerAran, 9300344) == 0)
            {
                await mapObj.spawnMonsterOnGroundBelow(LifeFactory.Instance.GetMonsterTrust(9300344), new Point(680, 258));
            }
        }

        public override async Task OnMobClear(AbstractEventInstanceManager eim, IMap map)
        {
            await eim.clearPQ();
        }
    }
}
