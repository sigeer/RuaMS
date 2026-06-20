namespace Application.Core.Game.Commands.Gm3
{
    public class MobRateCommand : ParamsCommandBase
    {
        public MobRateCommand() : base(["<mobrate>"], 3, "mobrate")
        {
        }

        public override async Task Execute(IChannelClient client, string[] values)
        {
            var cs = client.OnlinedCharacter.getChannelServer();
            var p = GetFloatParam("mobrate");
            await client.CurrentServer.Node.Transport.SendWorldConfig(new Config.WorldConfig { MobRate = p });
            await client.OnlinedCharacter.Notice($"全局的怪物倍率：x {cs.WorldMobRate}。当前地图的怪物倍率：x {client.OnlinedCharacter.getMap().MonsterRate}。当前地图实际倍率：x {client.OnlinedCharacter.getMap().ActualMonsterRate}");
        }
    }
}
