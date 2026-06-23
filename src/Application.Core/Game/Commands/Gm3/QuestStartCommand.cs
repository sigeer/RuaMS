using server.quest;

namespace Application.Core.Game.Commands.Gm3;

public class QuestStartCommand : CommandBase
{
    public QuestStartCommand() : base(3, "startquest")
    {
        Description = "Start a quest.";
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;

        if (paramsValue.Length < 1)
        {
            await player.Yellow("Syntax: !startquest <questid>");
            return;
        }

        int questid = int.Parse(paramsValue[0]);

        if (player.getQuestStatus(questid) == 0)
        {
            Quest quest = Quest.getInstance(questid);
            if (quest != null && quest.getNpcRequirement(false) != -1)
            {
                await c.OnlinedCharacter.getAbstractPlayerInteraction().forceStartQuest(questid, quest.getNpcRequirement(false));
            }
            else
            {
                await c.OnlinedCharacter.getAbstractPlayerInteraction().forceStartQuest(questid);
            }

            await player.dropMessage(5, "QUEST " + questid + " started.");
        }
        else
        {
            await player.dropMessage(5, "QUESTID " + questid + " already started/completed.");
        }
    }
}
