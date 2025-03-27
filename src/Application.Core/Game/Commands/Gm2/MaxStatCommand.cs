using client;

namespace Application.Core.Game.Commands.Gm2;

public class MaxStatCommand : CommandBase
{
    public MaxStatCommand() : base(2, "maxstat")
    {
        Description = "Max out all character stats.";
    }

    public override void Execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        player.loseExp(player.getExp(), false, false);
        player.setLevel(NumericConfig.MaxLevel);
        player.resetPlayerRates();
        if (YamlConfig.config.server.USE_ADD_RATES_BY_LEVEL)
        {
            player.setPlayerRates();
        }
        player.setWorldRates();
        player.updateStrDexIntLuk(NumericConfig.MaxStat);
        player.setFame(NumericConfig.MaxFame);
        player.SetMaxHP(NumericConfig.MaxHP);
        player.SetMaxMP(NumericConfig.MaxMP);
        player.SendStats();
        //player.updateSingleStat(Stat.LEVEL, NumericConfig.MaxLevel);
        //player.updateSingleStat(Stat.FAME, NumericConfig.MaxFame);
        player.yellowMessage("Stats maxed out.");
    }
}
