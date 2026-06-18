namespace Application.Core.Game.Commands.Gm0;

public class StaffCommand : CommandBase
{
    public StaffCommand() : base(0, "credits")
    {
        Description = "Show credits. These people made the server possible.";
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        await c.OnlinedCharacter.OpenNpc(NpcId.HERACLE, "credits");
    }
}
