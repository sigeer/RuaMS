using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;
using Application.Utility;
using Application.Utility.Extensions;
using Application.Utility.Tasks;
using server.life;
using System.Collections.Concurrent;

namespace Application.Plugin.TreasureMap
{
    internal class NpcScript : NpcScriptBase
    {
        public NpcScript(IChannelClient c, int npc, NPC? npcObj) : base(c, npc, npcObj)
        {
        }

        static int[] Maps = [
            100010000,
            100020000,

            101010000,

            101030000,
            102010000,

            102050000,
            103010000,
            ];
        /// <summary>
        /// level.45
        /// </summary>
        static int[] Mobs = [9400100];
        const int TreasureMapItemId = 1;

        ConcurrentDictionary<int, int> _quests = [];

        public async Task getTreasureMap()
        {
            if (getLevel() < 30)
            {
                await SayOK("30级之后再来吧");
                return;
            }

            if (c.CurrentServer.Id != 2)
            {
                await SayOK("只有频道2才有这个活动");
                return;
            }

            await AskMenu("", [
                "打听消息",
                ]);

            if (_quests.TryGetValue(getPlayer().Id, out var map))
            {
                await SayOK($"听说出没在 #r#m{map}##k 附近");
                return;
            }
            else
            {
                if (getMeso() < 20000)
                {
                    await SayOK("");
                    return;
                }

                await gainMeso(-20000);

                var mapId = Randomizer.Select(Maps);
                var targetMap = await getMap(mapId);

                var rndPos = targetMap.getMapArea().GetRandomPoint();
                var pos = targetMap.getGroundBelow(rndPos);

                var mobId = Randomizer.Select(Mobs);
                var mobTemplate = LifeFactory.Instance.GetMonsterTrust(mobId);

                var mob = await targetMap.spawnMonsterOnGroundBelow(mobTemplate, pos, m =>
                {
                    m.setOverrideStats(new OverrideMonsterStats());
                    m.CustomeDrops = [DropEntry.MobDrop(m.getId(), TreasureMapItemId, 400_000, 1, 1, 0), DropEntry.MobDrop(m.getId(), 0, 1_000_000, 10000, 20000, 0)];
                    m.AllowedAttacker = [getPlayer().getObjectId()];

                    ScheduledFuture? scheduledFuture = null;
                    m.OnSpawned += async (o, s) =>
                    {
                        scheduledFuture = await c.CurrentServer.TimerManager.ScheduleAsync($"TreasureMap_{getPlayer().Id}", () =>
                        {
                            return m.MapModel.Send(async v =>
                            {
                                await v.RemoveMob(m, null, false);
                            });
                        }, TimeSpan.FromMinutes(30));
                    };
                    m.OnLifeCleared += async (o, s) =>
                    {
                        _quests.TryRemove(getPlayer().Id, out _);
                        if (scheduledFuture != null)
                        {
                            await scheduledFuture.CancelAsync(false);
                        }
                    };
                });
                _quests[getPlayer().Id] = mapId;

                await SayOK($"听说出没在 #r#m{mapId}##k 附近");
            }
        }
    }
}
