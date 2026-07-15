using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;
using Application.Shared.Constants.Map;
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

        static ConcurrentDictionary<int, int> _quests = [];

        public async Task n1052103()
        {
            if (getLevel() < 30)
            {
                await SayOK("30级之后再来吧");
                return;
            }

            if (c.CurrentServer.Id != Settings.ActiveChannel)
            {
                await SayOK($"只有#r频道{Settings.ActiveChannel}#k才有这个活动");
                return;
            }

            await AskMenu($"最近有村民提到有流氓的消息，想不想听听？不过你要给我好处费哦，{Settings.QuestPrice}金币一条消息。", [
                "听听无妨",
                ]);

            if (_quests.TryGetValue(getPlayer().Id, out var map))
            {
                await SayOK($"听说出没在 #r#m{map}##k 附近");
                return;
            }
            else
            {
                if (getMeso() < Settings.QuestPrice)
                {
                    await SayOK("金币不足");
                    return;
                }

                await gainMeso(-Settings.QuestPrice);

                var mapId = Randomizer.Select(Settings.MobMaps);
                var targetMap = await getMap(mapId);

                var rndPos = targetMap.getMapArea().GetRandomPoint();
                var pos = targetMap.getGroundBelow(rndPos);

                var mobId = Randomizer.Select(Settings.Mobs);
                var mobTemplate = LifeFactory.Instance.GetMonsterTrust(mobId);

                var mob = await targetMap.spawnMonsterOnGroundBelow(mobTemplate, pos, m =>
                {
                    m.CustomeDrops = [
                        DropEntry.MobDrop(m.getId(), Settings.TreasureMapItemId, 300_000, 1, 1, 0), 
                        DropEntry.MobDrop(m.getId(), 0, DropEntry.MaxChance, 2000, 8000, 0)
                        ];
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
