namespace Application.Core.Game.Commands.Gm3
{
    public class MapMobRateCommand : ParamsCommandBase
    {
        public MapMobRateCommand() : base(["<mobrate>"], 3, "mapmobrate")
        {
            Description = "设置当前地图的怪物倍率";
        }

        public override void Execute(IClient client, string[] values)
        {
            var curMap = client.OnlinedCharacter.getMap();
            var p = GetFloatParam("mobrate");
            curMap.MonsterRate = p;
            client.OnlinedCharacter.dropMessage($"全局的怪物倍率：x {client.OnlinedCharacter.getWorldServer().MobRate}。当前地图的怪物倍率：x {curMap.MonsterRate}。当前地图实际倍率：x {curMap.ActualMonsterRate}");
        }
    }
}
