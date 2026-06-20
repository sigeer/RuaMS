using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm2;

public class JobCommand : CommandBase
{
    public JobCommand() : base(2, "job")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length == 1)
        {
            int jobid = int.Parse(paramsValue[0]);
            if (jobid < 0 || jobid >= 2200)
            {
                await player.Yellow(nameof(ClientMessage.JobNotFound), jobid.ToString());
                return;
            }

            await player.changeJob(JobFactory.GetById(jobid));
            await player.equipChanged();
        }
        else if (paramsValue.Length == 2)
        {
            var victim = c.getChannelServer().getPlayerStorage().getCharacterByName(paramsValue[0]);

            if (victim != null && victim.IsOnlined)
            {
                int jobid = int.Parse(paramsValue[1]);
                if (jobid < 0 || jobid >= 2200)
                {
                    await player.Yellow(nameof(ClientMessage.JobNotFound), jobid.ToString());
                    return;
                }

                await victim.changeJob(JobFactory.GetById(jobid));
                await player.equipChanged();
            }
            else
            {
                await player.Yellow(nameof(ClientMessage.PlayerNotFoundInChannel), paramsValue[0]);
            }
        }
        else
        {
            await player.Yellow(nameof(ClientMessage.JobCommand_Syntax));
        }
    }
}
