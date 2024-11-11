namespace Application.Core.Game.Commands.Gm3
{
    public class MobRateCommand : ParamsCommandBase
    {
        public MobRateCommand() : base(["<mobrate>"], 3, "mobrate")
        {
        }

        public override void Execute(IClient client, string[] values)
        {
            var curMap = client.OnlinedCharacter.getMap();
            var p = GetFloatParam("mobrate");
            curMap.MonsterRate = p;
            client.OnlinedCharacter.dropMessage(1, $"当前地图的怪物倍率：x " + curMap.MonsterRate);
        }
    }
}
