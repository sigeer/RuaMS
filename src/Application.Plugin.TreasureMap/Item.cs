using Application.Core.Client;
using Application.Core.scripting.item;
using Application.Core.tools.RandomUtils;
using Application.Shared.Constants.Map;
using Application.Utility;
using Application.Utility.Extensions;
using client.inventory;
using client.inventory.manipulator;
using server.life;
using System.Collections.Concurrent;
using System.Drawing;

namespace Application.Plugin.TreasureMap
{
    internal class ItemScript : ItemScriptBase
    {
        public ItemScript(IChannelClient c, client.inventory.Item item, int npcId) : base(c, item, npcId)
        {
        }
        ConcurrentDictionary<Item, (int Map, Point Position)> _mapValue = [];
        static int[] Maps = [
            MapId.HENESYS, MapId.HENESYS_PARK,
            MapId.KERNING_CITY,
            MapId.PERION,
            MapId.ELLINIA,
            MapId.LITH_HARBOUR,
            MapId.SLEEPYWOOD,
            ];
        public async Task UseTreasureMap()
        {
            if (c.CurrentServer.Id != 2)
            {
                await SayOK($"只能在#r频道2#k使用藏宝图");
                return;
            }

            if (!_mapValue.ContainsKey(_item))
            {
                var map = Randomizer.Select(Maps);

                var mapObj = await getMap(map);
                var p = mapObj.getMapArea().GetRandomPoint();
                var pos = mapObj.getGroundBelow(p);

                _mapValue[_item] = (map, pos);
            }

            var v = _mapValue[_item];
            if (getMapId() != v.Map)
            {
                await SayOK($"在 #r#m{v.Map}##k 的某处...");
                return;
            }

            var nowPos = getPlayer().getPosition();
            var direction = GetDirection(nowPos.X, nowPos.Y, v.Position.X, v.Position.Y, true);
            if (!string.IsNullOrEmpty(direction))
            {
                await SayOK($"在你的 #b{direction}#k");
                return;
            }

            await GetTreasureMapReward();
        }

        async Task GetTreasureMapReward()
        {
            List<LotteryMachinItem<TreasureMapRewardType>> allItems = [
                new LotteryMachinItem<TreasureMapRewardType>(TreasureMapRewardType.Mob, 40),
                new LotteryMachinItem<TreasureMapRewardType>(TreasureMapRewardType.Meso, 20),
                new LotteryMachinItem<TreasureMapRewardType>(TreasureMapRewardType.Equip, 15),
                new LotteryMachinItem<TreasureMapRewardType>(TreasureMapRewardType.Damage, 10),
                new LotteryMachinItem<TreasureMapRewardType>(TreasureMapRewardType.None, 5),
                new LotteryMachinItem<TreasureMapRewardType>(TreasureMapRewardType.Skill, 5),
                new LotteryMachinItem<TreasureMapRewardType>(TreasureMapRewardType.Scroll, 5),
                ];
            var result = new LotteryMachine<TreasureMapRewardType>(allItems).GetRandomItem();
            switch (result)
            {
                case TreasureMapRewardType.None:
                    await SayOK("什么也没有...");
                    break;
                case TreasureMapRewardType.Mob:
                    var mobTemplate = LifeFactory.Instance.GetMonsterTrust(5250001);
                    await getMap().spawnMonsterOnGroundBelow(mobTemplate, getPlayer().getPosition());
                    await getMap().spawnMonsterOnGroundBelow(mobTemplate, getPlayer().getPosition());
                    break;
                case TreasureMapRewardType.Meso:
                    await gainMeso(Randomizer.rand(20000, 50000));
                    break;
                case TreasureMapRewardType.Equip:
                    break;
                case TreasureMapRewardType.Damage:
                    await getPlayer().ChangeHP(-(int)(getPlayer().HP * 0.2));
                    break;
                case TreasureMapRewardType.Skill:
                    break;
                case TreasureMapRewardType.Scroll:
                    break;
                default:
                    break;
            }
            await InventoryManipulator.removeFromSlot(c, _item.getInventoryType(), _item.getPosition(), _item.getQuantity(), true);
        }

        private static string? GetDirection(double ax, double ay, double bx, double by, bool yDown)
        {
            double dx = bx - ax;
            double dy = by - ay;

            if (Math.Abs(dx) <= 50 && Math.Abs(dy) <= 50)
                return null;

            // 如果 Y 轴向下（屏幕坐标），反转 dy
            if (yDown)
                dy = -dy;

            // 计算角度（弧度转度），范围 [-180, 180]
            double angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;
            if (angle < 0)
                angle += 360.0;

            // 八个方向角度区间（中心点：0=右，45=右上，90=上，135=左上，180=左，225=左下，270=下，315=右下）
            if (angle >= 337.5 || angle < 22.5)
                return "右方";
            if (angle >= 22.5 && angle < 67.5)
                return "右上方";
            if (angle >= 67.5 && angle < 112.5)
                return "上方";
            if (angle >= 112.5 && angle < 157.5)
                return "左上方";
            if (angle >= 157.5 && angle < 202.5)
                return "左方";
            if (angle >= 202.5 && angle < 247.5)
                return "左下方";
            if (angle >= 247.5 && angle < 292.5)
                return "下方";
            // 292.5 ~ 337.5
            return "右下方";
        }
    }

    public enum TreasureMapRewardType
    {
        None,
        Mob,
        Meso,
        Equip,
        Damage,
        Skill,
        Scroll
    }
}
