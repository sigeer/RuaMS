using Application.Core.Client;
using Application.Core.Server.maps;
using Application.Shared.MapObjects;
using scripting.reactor;
using System.Drawing;

namespace Application.Plugin.Script.Features.DavyJohn
{
    internal sealed class ReactorActHandler : ReactorActScriptBase
    {
        public ReactorActHandler(IChannelClient c, Reactor reactor) : base(c, reactor)
        {
        }

        public async Task davyMob0()
        {
            var eim = getEventInstance();
            if (eim == null) return;

            eim.setIntProperty(DavyJohnFeature.OpenedBoxesProperty, eim.getIntProperty(DavyJohnFeature.OpenedBoxesProperty) + 1);

            await spawnMonster(DavyJohnFeature.GuardMobId, DavyJohnFeature.GuardMobQuantity);
            await spawnMonster(DavyJohnFeature.GruntMobId, DavyJohnFeature.GruntMobQuantity);
        }

        public async Task davyMob1()
        {
            for (var i = 0; i < DavyJohnFeature.WindflowerQuantity; i++)
            {
                await spawnMonster(DavyJohnFeature.WindflowerMobId1);
                await spawnMonster(DavyJohnFeature.WindflowerMobId2);
            }
        }

        public async Task davyItem0()
        {
            var eim = getEventInstance();
            if (eim == null) return;

            eim.setIntProperty(DavyJohnFeature.OpenedBoxesProperty, eim.getIntProperty(DavyJohnFeature.OpenedBoxesProperty) + 1);

            await dropItems(true, 1, 30, 60, 15);

            var map = getMap();
            if (map.countMonsters() == 0
                && (eim.getIntProperty(DavyJohnFeature.GrindModeProperty) == 0
                    || eim.activatedAllReactorsOnMap(map, DavyJohnFeature.SealedBossKey, DavyJohnFeature.LastSealedBossKey)))
            {
                await eim.showClearEffect(map.getId());
            }
        }

        public async Task davyItem1()
        {
            var eim = getEventInstance();
            if (eim == null) return;

            eim.setIntProperty(DavyJohnFeature.OpenedChestsProperty, eim.getIntProperty(DavyJohnFeature.OpenedChestsProperty) + 1);
            await dropItems(true, 1, 50, 100, 15);
        }

        public async Task davyNPC0()
        {
            await Pink("老海盗已经被打败, 无恙被释放了！");
            await spawnNpc(2094001);
        }

        public Task davyScript0() => ClearIfAllSealedBossDefeated();

        public Task davyScript1() => ClearIfAllSealedBossDefeated();

        public Task davyScript2() => ClearIfAllSealedBossDefeated();

        public Task davyScript3() => ClearIfAllSealedBossDefeated();

        async Task ClearIfAllSealedBossDefeated()
        {
            var denyWidth = 320;
            var denyHeight = 150;
            var denyPos = getReactor().getPosition();
            var denyArea = new Rectangle(denyPos.X - denyWidth / 2, denyPos.Y - denyHeight / 2, denyWidth, denyHeight);

            var map = getReactor().getMap();
            map.setAllowSpawnPointInBox(false, denyArea);

            var eim = getEventInstance();
            if (eim == null) return;

            if (map.GetRequiredMapObjects<Reactor>(MapObjectType.REACTOR, r => r.getName().StartsWith("sMob") && r.getState() < 1).Count == 0
                && map.countMonsters() == 0)
            {
                await eim.showClearEffect(map.getId());
            }
        }
    }
}
