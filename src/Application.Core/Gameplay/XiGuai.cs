using Application.Core.Game.Life;
using Application.Core.Game.Maps;

namespace Application.Core.Game.Gameplay
{
    /// <summary>
    /// 离开地图后终止，或者手动终止
    /// </summary>
    public class XiGuai
    {
        public XiGuai(IMap mapModel, Player controller)
        {
            MapModel = mapModel;
            Controller = controller;
            Position = Controller.getPosition();
        }

        public IMap MapModel { get; set; }
        public Player Controller { get; set; }
        public Point Position { get; private set; }

        public void RestPosition()
        {
            Position = Controller.getPosition();
        }

        public void Start()
        {
            MapModel.ProcessMonster(monster =>
            {
                ApplyMonster(monster);
            });
            Controller.message("开启吸怪");
        }

        public void ApplyMonster(Monster monster)
        {
            if (monster.isBoss() || monster.getStats().isFriendly())
            {
                return;
            }

            Task.Delay(TimeSpan.FromSeconds(1)).ContinueWith((t) =>
            {
                monster.resetMobPosition(Position);
            });
        }
    }
}
