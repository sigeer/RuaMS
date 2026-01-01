using Application.Core.Channel.DataProviders;
using server.quest;

namespace Application.Core.Game.Commands.Gm6;

public class ClearQuestCommand : CommandBase
{
    public ClearQuestCommand() : base(6, "clearquest")
    {
        Description = "Clear cache of a quest.";
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.dropMessage(5, "Please include a quest ID.");
            return Task.CompletedTask;
        }
        QuestFactory.Instance.clearCache(int.Parse(paramsValue[0]));
        player.dropMessage(5, "Quest Cache for quest " + paramsValue[0] + " cleared.");
        return Task.CompletedTask;

    }
}
