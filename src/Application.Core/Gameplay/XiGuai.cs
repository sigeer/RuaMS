using Application.Core.Game.Life;
using Application.Core.Game.Maps;

namespace Application.Core.Game.Gameplay
{
    /// <summary>
    /// 离开地图后终止，或者手动终止
    /// </summary>
    public class XiGuai
    {
        public XiGuai(IMap mapModel, IPlayer controller)
        {
            MapModel = mapModel;
            Controller = controller;
            Position = Controller.getPosition();
        }

        public IMap MapModel { get; set; }
        public IPlayer Controller { get; set; }
        public Point Position { get; }

        public void Start()
        {
            var monsterList = MapModel.getAllMonsters();
            foreach (var monster in monsterList)
            {
                ApplyMonster(monster);
            }
            Controller.message("开启吸怪");
        }

        public void ApplyMonster(Monster monster)
        {
            if (monster.isBoss())
            {
                return;
            }

            monster.resetMobPosition(Position);
        }
    }
}
