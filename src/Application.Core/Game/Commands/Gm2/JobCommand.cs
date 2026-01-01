using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm2;

public class JobCommand : CommandBase
{
    public JobCommand() : base(2, "job")
    {
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length == 1)
        {
            int jobid = int.Parse(paramsValue[0]);
            if (jobid < 0 || jobid >= 2200)
            {
                player.YellowMessageI18N(nameof(ClientMessage.JobNotFound), jobid.ToString());
                return Task.CompletedTask;
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
                    player.YellowMessageI18N(nameof(ClientMessage.JobNotFound), jobid.ToString());
                    return Task.CompletedTask;
                }

                victim.changeJob(JobFactory.GetById(jobid));
                player.equipChanged();
            }
            else
            {
                player.YellowMessageI18N(nameof(ClientMessage.PlayerNotFoundInChannel), paramsValue[0]);
            }
        }
        else
        {
            player.YellowMessageI18N(nameof(ClientMessage.JobCommand_Syntax));
        }
        return Task.CompletedTask;
    }
}
