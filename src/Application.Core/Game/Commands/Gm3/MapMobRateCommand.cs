namespace Application.Core.Game.Commands.Gm3
{
    public class MapMobRateCommand : ParamsCommandBase
    {
        public MapMobRateCommand() : base(["<mobrate>"], 3, "mapmobrate")
        {
        }

        public override void Execute(IClient client, string[] values)
        {
            var curMap = client.OnlinedCharacter.getMap();
            var p = GetFloatParam("mobrate");
            curMap.MonsterRate = p;
            client.OnlinedCharacter.dropMessage($"当前地图的怪物倍率：x " + curMap.MonsterRate);
        }
    }
}
