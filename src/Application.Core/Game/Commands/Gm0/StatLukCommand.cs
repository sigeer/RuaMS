namespace Application.Core.Game.Commands.Gm0;

public class StatLukCommand : CommandBase
{
    public StatLukCommand() : base(0, "luk")
    {
        Description = "Assign AP into LUK.";
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        int remainingAp = player.getRemainingAp();

        int amount;
        if (paramsValue.Length > 0)
        {
            try
            {
                amount = Math.Min(int.Parse(paramsValue[0]), remainingAp);
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                await player.Notice("That is not a valid number!");
                return;
            }
        }
        else
        {
            amount = Math.Min(remainingAp, YamlConfig.config.server.MAX_AP - player.getLuk());
        }
        if (!await player.assignLuk(Math.Max(amount, 0)))
        {
            await player.Notice("Please make sure your AP is not over " + YamlConfig.config.server.MAX_AP + " and you have enough to distribute.");
        }
    }
}
