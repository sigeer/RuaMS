namespace Application.Core.Game.Commands.Gm3
{
    public class MobRateCommand : ParamsCommandBase
    {
        public MobRateCommand() : base(["<mobrate>"], 3, "mobrate")
        {
        }

        public override void Execute(IClient client, string[] values)
        {
            var ws = client.OnlinedCharacter.getWorldServer();
            var p = GetFloatParam("mobrate");
            ws.MobRate = p;
            client.OnlinedCharacter.dropMessage($"全局的怪物倍率：x {ws.MobRate}。总倍率：x {ws.MobRate * client.OnlinedCharacter.getMap().MonsterRate}");
        }
    }
}
