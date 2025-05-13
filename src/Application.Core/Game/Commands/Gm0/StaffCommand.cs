using constants.id;

namespace Application.Core.Game.Commands.Gm0;

public class StaffCommand : CommandBase
{
    public StaffCommand() : base(0, "credits")
    {
        Description = "Show credits. These people made the server possible.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        c.OpenNpc(NpcId.HERACLE, "credits");
    }
}
