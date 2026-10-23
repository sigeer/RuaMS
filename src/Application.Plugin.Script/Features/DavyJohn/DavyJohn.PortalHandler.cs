using Application.Core.Client;
using Application.Core.Server.life;
using Application.Core.Server.maps;
using scripting.portal;
using System.Drawing;

namespace Application.Plugin.Script.Features.DavyJohn
{
    internal sealed class PortalHandler : PortalPlayerInteraction
    {
        public PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 925100200 | pt: 8
        public async Task<bool> davy2_hd1()
        {
            var eim = getEventInstance();
            if (eim == null || eim.EventName != DavyJohnFeature.EventName)
            {
                return false;
            }

            var level = eim.getIntProperty(DavyJohnFeature.StackProperty);
            if (eim.getProperty(DavyJohnFeature.Stage2AProperty) == "0")
            {
                await (await getMap(DavyJohnFeature.Stage2CheckMap)).spawnAllMonstersFromMapSpawnList(level, true);
                eim.setProperty(DavyJohnFeature.Stage2AProperty, "1");
            }

            await playPortalSound();
            await warp(DavyJohnFeature.Stage2CheckMap, 0);
            return true;
        }

        // Map: 925100300 | pt: 8
        public async Task<bool> davy3_hd1()
        {
            var eim = getEventInstance();
            if (eim == null || eim.EventName != DavyJohnFeature.EventName)
            {
                return false;
            }

            var level = eim.getIntProperty(DavyJohnFeature.StackProperty);
            if (eim.getProperty(DavyJohnFeature.Stage3AProperty) == "0")
            {
                await (await getMap(DavyJohnFeature.Stage3CheckMap)).spawnAllMonstersFromMapSpawnList(level, true);
                eim.setProperty(DavyJohnFeature.Stage3AProperty, "1");
            }

            await playPortalSound();
            await warp(DavyJohnFeature.Stage3CheckMap, 0);
            return true;
        }

        // Map: 925100000 | pt: 7
        public async Task<bool> davy_next0()
        {
            var eim = getEventInstance();
            if (eim == null || eim.EventName != DavyJohnFeature.EventName)
            {
                return false;
            }

            if (getMap().countMonsters() == 0 && DavyJohnFeature.PassedGrindMode(getMap(), eim))
            {
                await playPortalSound();
                await warp(DavyJohnFeature.Stage2Map, 0);
                return true;
            }

            await Pink("The portal is not opened yet.");
            return false;
        }

        // Map: 925100100 | pt: 7
        public async Task<bool> davy_next1()
        {
            var eim = getEventInstance();
            if (eim != null && eim.EventName == DavyJohnFeature.EventName && eim.getProperty(DavyJohnFeature.Stage2Property) == "3")
            {
                await playPortalSound();
                await warp(DavyJohnFeature.Stage2PassageMap, 0);
                return true;
            }

            await Pink("The portal is not opened yet.");
            return false;
        }

        // Map: 925100200 | pt: 7
        public async Task<bool> davy_next2()
        {
            var eim = getEventInstance();
            if (eim == null || eim.EventName != DavyJohnFeature.EventName)
            {
                return false;
            }

            if (getMap().countMonsters() == 0 && DavyJohnFeature.PassedGrindMode(getMap(), eim))
            {
                await playPortalSound();
                await warp(DavyJohnFeature.Stage3PassageMap, 0);
                return true;
            }

            await Pink("The portal is not opened yet.");
            return false;
        }

        // Map: 925100300 | pt: 7
        public async Task<bool> davy_next3()
        {
            var eim = getEventInstance();
            if (eim == null || eim.EventName != DavyJohnFeature.EventName)
            {
                return false;
            }

            if (getMap().countMonsters() == 0 && DavyJohnFeature.PassedGrindMode(getMap(), eim))
            {
                await playPortalSound();
                await warp(DavyJohnFeature.Stage4Map, 0);
                return true;
            }

            await Pink("The portal is not opened yet.");
            return false;
        }

        // Map: 925100400 | pt: 7
        public async Task<bool> davy_next4()
        {
            if (getMap().getReactorByName("sMob1")?.getState() < 1
                || getMap().getReactorByName("sMob2")?.getState() < 1
                || getMap().getReactorByName("sMob3")?.getState() < 1
                || getMap().getReactorByName("sMob4")?.getState() < 1
                || getMap().countMonsters() != 0)
            {
                await Pink("The portal is not opened yet.");
                return false;
            }

            var eim = getEventInstance();
            if (eim == null || eim.EventName != DavyJohnFeature.EventName)
            {
                return false;
            }

            if (eim.getProperty(DavyJohnFeature.SpawnedBossProperty) == null)
            {
                var level = int.Parse(eim.getProperty(DavyJohnFeature.StackProperty));
                var chests = int.Parse(eim.getProperty(DavyJohnFeature.OpenedChestsProperty));

                // 打开宝箱越多，老海盗越强
                var bossId = chests switch
                {
                    0 => 9300119,
                    1 => 9300105,
                    _ => 9300106
                };

                await (await getMap(DavyJohnFeature.BossMap)).spawnMonsterOnGroundBelow(LifeFactory.Instance.GetMonsterTrust(bossId), new Point(777, 140), mob =>
                {
                    mob.changeDifficulty(level, true);
                });
                eim.setProperty(DavyJohnFeature.SpawnedBossProperty, "true");
            }

            await playPortalSound();
            await warp(DavyJohnFeature.BossMap, 0);
            return true;
        }

        [ScriptName("davy_next00")]
        public Task<bool> p_davy_next00()
        {
            return Task.FromResult(true);
        }
    }
}
