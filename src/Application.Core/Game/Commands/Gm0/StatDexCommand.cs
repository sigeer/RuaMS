namespace Application.Core.Game.Commands.Gm0;

public class StatDexCommand : CommandBase
{
    public StatDexCommand() : base(0, "dex")
    {
        Description = "Assign AP into DEX.";
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
                log.Warning(e.ToString());
                await player.Notice("That is not a valid number!");
                return;
            }
        }
        else
        {
            amount = Math.Min(remainingAp, YamlConfig.config.server.MAX_AP - player.getDex());
        }
        if (!await player.assignDex(Math.Max(amount, 0)))
        {
            await player.Notice("Please make sure your AP is not over " + YamlConfig.config.server.MAX_AP + " and you have enough to distribute.");
        }
    }
}
