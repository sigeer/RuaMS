using Application.Core.Channel.DataProviders;
using Application.Core.Client;
using Application.Core.Gameplay.Plugins;
using Application.Core.scripting.item;
using Application.Core.tools.RandomUtils;
using Application.Shared.Constants.Inventory;
using Application.Templates.Character;
using Application.Templates.Reader;
using Application.Utility;
using Application.Utility.Extensions;
using client.inventory.manipulator;
using server.life;
using System.Drawing;

namespace Application.Plugin.TreasureMap
{
    internal class ItemScript : ItemScriptBase
    {
        public ItemScript(IChannelClient c, client.inventory.Item item, int npcId) : base(c, item, npcId)
        {
        }

        [ScriptName("compassUse_cash")]
        public async Task UseTreasureMap()
        {
            if (c.CurrentServer.Id != Settings.ActiveChannel)
            {
                await SayOK($"只能在#r频道{Settings.ActiveChannel}#k使用藏宝图");
                return;
            }

            if (string.IsNullOrEmpty(_item.Properties))
            {
                var map = Randomizer.Select(Settings.Maps);

                var mapObj = await getMap(map);
                var p = mapObj.getMapArea().GetRandomPoint();
                var pos = mapObj.getGroundBelow(p);

                _item.Properties = KeyValueStringParser.Build([
                    new KeyValuePair<string, string>("Map", map.ToString()),
                    new KeyValuePair<string, string>("PosX", pos.X.ToString()),
                    new KeyValuePair<string, string>("PosY", pos.Y.ToString())]);
            }

            var v = KeyValueStringParser.Parse(_item.Properties);
            var nMap = int.Parse(v["Map"]);
            var nPos = new Point(int.Parse(v["PosX"]), int.Parse(v["PosY"]));
            if (getMapId() != nMap)
            {
                await SayOK($"在 #r#m{nMap}##k 的某处...");
                return;
            }

            var nowPos = getPlayer().getPosition();
            var direction = GetDirection(nowPos.X, nowPos.Y, nPos.X, nPos.Y, true);
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
                new LotteryMachinItem<TreasureMapRewardType>(TreasureMapRewardType.Meso, 25),
                new LotteryMachinItem<TreasureMapRewardType>(TreasureMapRewardType.Equip, 20),
                new LotteryMachinItem<TreasureMapRewardType>(TreasureMapRewardType.Damage, 5),
                new LotteryMachinItem<TreasureMapRewardType>(TreasureMapRewardType.None, 5),
                new LotteryMachinItem<TreasureMapRewardType>(TreasureMapRewardType.Skill, 2),
                new LotteryMachinItem<TreasureMapRewardType>(TreasureMapRewardType.Scroll, 3),
                ];
            var result = new LotteryMachine<TreasureMapRewardType>(allItems).GetRandomItem();
            switch (result)
            {
                case TreasureMapRewardType.Mob:
                    var mobId = new LotteryMachine<int>([new(5250001, 95), new(8150000, 5)]).GetRandomItem();
                    var mobTemplate = LifeFactory.Instance.GetMonsterTrust(mobId);
                    var count = mobId == 5250001 ? Randomizer.NextInt(2, 8) : 2;
                    for (int i = 0; i < count; i++)
                    {
                        await getMap().spawnMonsterOnGroundBelow(mobTemplate, getPlayer().getPosition());
                    }
                    await getMap().LightBlue($"{getPlayer().Name} 挖出了一群 #o{mobId}#");
                    break;
                case TreasureMapRewardType.Meso:
                    await getMap().spawnMesoDrop(Randomizer.rand(20000, 50000), getPlayer().getPosition(), getPlayer(), getPlayer(), true, Shared.MapObjects.DropType.OnlyOwner);
                    break;
                case TreasureMapRewardType.Equip:
                    // 10 ~ 60 级各种类型的普通装备
                    var equip = ItemInformationProvider.getInstance().GenerateVirtualItemById(Randomizer.Select(equipItemIds), 1, true);
                    await getMap().spawnItemDrop(getPlayer(), getPlayer(), equip, getPlayer().getPosition(), false, true);
                    break;
                case TreasureMapRewardType.Damage:
                    await getPlayer().ChangeHP(-(int)(getPlayer().HP * 0.2));
                    break;
                //case TreasureMapRewardType.Skill:
                //    // 可以根据不同地图掉落不同
                //    break;
                //case TreasureMapRewardType.Scroll:
                //    break;
                default:
                    await SayOK("这里什么也没有...");
                    break;
            }
            await InventoryManipulator.removeFromSlot(c, _item.getInventoryType(), _item.getPosition(), _item.getQuantity(), true);
        }

        static int[] equipItemIds = ProviderSource.Instance.GetProvider(ProviderType.Equip).LoadAll().OfType<EquipTemplate>()
            .Where(x =>
                !x.Cash
                && x.ReqLevel >= 10 && x.ReqLevel <= 70
                && !x.TradeBlock && !x.Only && !x.Quest
                && x.Price > 1
                && !exceptSlots.Contains(EquipSlot.getFromTextSlot(x.Islot))
                ).Select(x => x.TemplateId).ToArray();

        static EquipSlot[] exceptSlots = new EquipSlot[] {
            EquipSlot.EYE_ACCESSORY,
            EquipSlot.FACE_ACCESSORY,
            EquipSlot.BELT,
            EquipSlot.SADDLE,
            EquipSlot.MEDAL,
            EquipSlot.TAMED_MOB,
            EquipSlot.PENDANT,
            EquipSlot.RING,
            EquipSlot.Hair,
            EquipSlot.Face
        };

        static LotteryMachine<int> ScrollPool = new LotteryMachine<int>([
            new (2040804, 2),
            new (2040805, 1)
            ]);

        private static string? GetDirection(double ax, double ay, double bx, double by, bool yDown)
        {
            double dx = bx - ax;
            double dy = by - ay;
            double distance = dx * dx + dy * dy;

            // 如果重合，特殊处理
            if (distance < 450)
                return null;

            // 距离修饰
            string distModifier;
            if (distance < Settings.NEAR_THRESHOLD)
                distModifier = "很近了";
            else
                distModifier = "较远";

            // 方向计算（与之前相同）
            if (yDown)
                dy = -dy;

            double angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;
            if (angle < 0)
                angle += 360.0;

            string direction;
            if (angle >= 337.5 || angle < 22.5) direction = "右方";
            else if (angle >= 22.5 && angle < 67.5) direction = "右上方";
            else if (angle >= 67.5 && angle < 112.5) direction = "上方";
            else if (angle >= 112.5 && angle < 157.5) direction = "左上方";
            else if (angle >= 157.5 && angle < 202.5) direction = "左方";
            else if (angle >= 202.5 && angle < 247.5) direction = "左下方";
            else if (angle >= 247.5 && angle < 292.5) direction = "下方";
            else direction = "右下方";

            // 组合：距离修饰 + 方向
            return $"{distModifier}，{direction}！";
        }
    }
}
