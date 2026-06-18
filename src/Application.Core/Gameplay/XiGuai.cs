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

        public async Task Start()
        {
            await MapModel.ProcessMonster(async monster =>
            {
                await ApplyMonster(monster);
            });
            await Controller.Pink("开启吸怪");
        }

        public async Task ApplyMonster(Monster monster)
        {
            if (monster.isBoss() || monster.getStats().isFriendly())
            {
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
            await monster.resetMobPosition(Position);
        }
    }
}
