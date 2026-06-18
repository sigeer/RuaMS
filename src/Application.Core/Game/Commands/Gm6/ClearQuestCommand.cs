using Application.Core.Channel.DataProviders;

namespace Application.Core.Game.Commands.Gm6;

public class ClearQuestCommand : CommandBase
{
    public ClearQuestCommand() : base(6, "clearquest")
    {
        Description = "Clear cache of a quest.";
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            await player.Pink("Please include a quest ID.");
            return;
        }
        QuestFactory.Instance.clearCache(int.Parse(paramsValue[0]));
        await player.Pink("Quest Cache for quest " + paramsValue[0] + " cleared.");

    }
}
