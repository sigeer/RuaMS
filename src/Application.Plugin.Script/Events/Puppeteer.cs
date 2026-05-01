using Application.Core.Channel;
using Application.Core.Game.Maps;
using Application.Core.Scripting.Events;
using server.life;
using System.Drawing;

namespace Application.Plugin.Script.Events
{
    internal class Puppeteer : SoloEventManager
    {
        public Puppeteer(WorldChannel cserv) : base(cserv, nameof(Puppeteer))
        {
            EventTime = 10 * 60;
            EntryMap = 910510000;
            ExitMap = 105070300;
            MinMap = 910510000;
            MaxMap = 910510000;
        }

        public override void AfterSeup(AbstractEventInstanceManager eim)
        {
            var mapObj = eim.getMapInstance(EntryMap);
            var chr = eim.getLeader()!;
            if (chr.getQuestStatus(20730) == 1 && chr.GetQuestProgressInt(20730, 9300285) == 0)
            {
                mapObj.spawnMonsterOnGroundBelow(LifeFactory.Instance.getMonster(9300285), new Point(680, 258));
            }

            if (chr.getQuestStatus(21731) == 1 && chr.GetQuestProgressInt(21731, 9300344) == 0)
            {
                mapObj.spawnMonsterOnGroundBelow(LifeFactory.Instance.getMonster(9300344), new Point(680, 258));
            }
        }

        public override void OnMobClear(AbstractEventInstanceManager eim, IMap map)
        {
            eim.clearPQ();
        }
    }
}
