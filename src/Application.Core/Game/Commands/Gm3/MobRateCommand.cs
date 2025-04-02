namespace Application.Core.Game.Commands.Gm3
{
    public class MobRateCommand : ParamsCommandBase
    {
        public MobRateCommand() : base(["<mobrate>"], 3, "mobrate")
        {
            Description = "设置全局怪物倍率，范围：(0, 5]";
        }

        public override void Execute(IClient client, string[] values)
        {
            var ws = client.OnlinedCharacter.getWorldServer();
            var p = GetFloatParam("mobrate");
            ws.MobRate = p;
            client.OnlinedCharacter.dropMessage($"全局的怪物倍率：x {ws.MobRate}。当前地图的怪物倍率：x {client.OnlinedCharacter.getMap().MonsterRate}。当前地图实际倍率：x {client.OnlinedCharacter.getMap().ActualMonsterRate}");
        }
    }
}
