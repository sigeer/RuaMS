using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm3;

public class CheckDmgCommand : CommandBase
{
    public CheckDmgCommand() : base(3, "checkdmg")
    {
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        var victim = c.getChannelServer().getPlayerStorage().getCharacterByName(paramsValue[0]);
        if (victim != null && victim.IsOnlined)
        {
            int maxBase = victim.calculateMaxBaseDamage(victim.getTotalWatk());
            var watkBuff = victim.getBuffedValue(BuffStat.WATK) ?? 0;
            var matkBuff = victim.getBuffedValue(BuffStat.MATK) ?? 0;
            int blessing = victim.getSkillLevel(10000000 * player.getJobType() + 12);

            player.dropMessage(5, player.GetMessageByKey(nameof(ClientMessage.CheckDmgCommand_Message1),
                 victim.getTotalStr().ToString(),
                 victim.getTotalDex().ToString(),
                 victim.getTotalInt().ToString(),
                 victim.getTotalLuk().ToString()));
            player.dropMessage(5, player.GetMessageByKey(nameof(ClientMessage.CheckDmgCommand_Message2), victim.getTotalWatk().ToString(), victim.getTotalMagic().ToString()));
            player.dropMessage(5, player.GetMessageByKey(nameof(ClientMessage.CheckDmgCommand_Message3), watkBuff.ToString(), matkBuff.ToString(), blessing.ToString()));
            player.dropMessage(5, player.GetMessageByKey(nameof(ClientMessage.CheckDmgCommand_Message4), victim.Name, maxBase.ToString()));
        }
        else
        {
            player.YellowMessageI18N(nameof(ClientMessage.PlayerNotFoundInChannel), paramsValue[0]);
        }
    }
}
