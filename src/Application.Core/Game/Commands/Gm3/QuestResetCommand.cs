using server.quest;

namespace Application.Core.Game.Commands.Gm3;

public class QuestResetCommand : CommandBase
{
    public QuestResetCommand() : base(3, "resetquest")
    {
        Description = "Reset a completed quest.";
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;

        if (paramsValue.Length < 1)
        {
            await player.Yellow("Syntax: !resetquest <questid>");
            return;
        }

        int questid_ = int.Parse(paramsValue[0]);

        if (player.getQuestStatus(questid_) != 0)
        {
            Quest quest = Quest.getInstance(questid_);
            if (quest != null)
            {
                await quest.reset(player);
                await player.Pink("QUEST " + questid_ + " reseted.");
            }
            else
            {    // should not occur
                await player.Pink("QUESTID " + questid_ + " is invalid.");
            }
        }
    }
}
