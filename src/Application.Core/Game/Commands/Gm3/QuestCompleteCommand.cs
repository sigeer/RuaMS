using server.quest;

namespace Application.Core.Game.Commands.Gm3;

public class QuestCompleteCommand : CommandBase
{
    public QuestCompleteCommand() : base(3, "completequest")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;

        if (paramsValue.Length < 1)
        {
            await player.Yellow("Syntax: !completequest <questid>");
            return;
        }

        int questId = int.Parse(paramsValue[0]);

        if (player.getQuestStatus(questId) == 1)
        {
            Quest quest = Quest.getInstance(questId);
            if (quest != null && quest.getNpcRequirement(true) != -1)
            {
                await c.OnlinedCharacter.ForceCompleteQuest(questId, quest.getNpcRequirement(true));
            }
            else
            {
                await c.OnlinedCharacter.ForceCompleteQuest(questId);
            }

            await player.Pink("QUEST " + questId + " completed.");
        }
        else
        {
            await player.Pink("QUESTID " + questId + " not started or already completed.");
        }
    }
}
