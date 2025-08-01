namespace Application.Core.Game.Commands.Gm2;

public class JobCommand : CommandBase
{
    public JobCommand() : base(2, "job")
    {
        Description = "Change job of a player.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length == 1)
        {
            int jobid = int.Parse(paramsValue[0]);
            if (jobid < 0 || jobid >= 2200)
            {
                player.message("Jobid " + jobid + " is not available.");
                return;
            }

            player.changeJob(JobFactory.GetById(jobid));
            player.equipChanged();
        }
        else if (paramsValue.Length == 2)
        {
            var victim = c.getChannelServer().getPlayerStorage().getCharacterByName(paramsValue[0]);

            if (victim != null && victim.IsOnlined)
            {
                int jobid = int.Parse(paramsValue[1]);
                if (jobid < 0 || jobid >= 2200)
                {
                    player.message("Jobid " + jobid + " is not available.");
                    return;
                }

                victim.changeJob(JobFactory.GetById(jobid));
                player.equipChanged();
            }
            else
            {
                player.message("Player '" + paramsValue[0] + "' could not be found on this channel.");
            }
        }
        else
        {
            player.message("Syntax: !job <job id> <opt: IGN of another person>");
        }
    }
}
