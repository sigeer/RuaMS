namespace Application.Core.Game.Commands.Gm3
{
    public class KillMobCommand : ParamsCommandBase
    {
        public KillMobCommand() : base(["<oid>"], 3, "killmob")
        {
        }

        public override async Task Execute(IChannelClient client, string[] values)
        {
            var mobOId = GetIntParam("oid");
            var mob = client.OnlinedCharacter.MapModel.getMonsterByOid(mobOId);
            if (mob == null)
            {
                return;
            }

            await mob.DamageBy(client.OnlinedCharacter, int.MaxValue, 0);
        }
    }
}
