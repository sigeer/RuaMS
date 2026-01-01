using Application.Core.Channel.DataProviders;

namespace Application.Core.Game.Commands.Gm3;

public class ReloadDropsCommand : CommandBase
{
    public ReloadDropsCommand() : base(3, "reloaddrops")
    {
        Description = "Reload all drop data.";
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        MonsterInformationProvider.getInstance().clearDrops();
        player.dropMessage(5, "Reloaded Drops");
        return Task.CompletedTask;
    }
}
