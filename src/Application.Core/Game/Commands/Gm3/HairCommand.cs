using Application.Core.Channel.DataProviders;

namespace Application.Core.Game.Commands.Gm3;

public class HairCommand : CommandBase
{
    public HairCommand() : base(3, "hair")
    {
        Description = "Change hair of a player.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !hair [<playername>] <hairid>");
            return;
        }

        IPlayer? targetPlayer = c.OnlinedCharacter;
        string? hairStr;
        if (paramsValue.Length == 1)
        {
            hairStr = paramsValue[0];
        }
        else
        {
            hairStr = paramsValue[1];
            targetPlayer = c.getChannelServer().getPlayerStorage().getCharacterByName(paramsValue[0]);
        }

        if (!int.TryParse(hairStr, out var hairId))
        {
            player.yellowMessage("Syntax: !hair [<playername>] <hairid>");
            return;
        }

        if (!ItemConstants.isHair(hairId) || !ItemInformationProvider.getInstance().HasTemplate(hairId))
        {
            player.yellowMessage("Hair id '" + hairId + "' does not exist.");
            return;
        }

        if (targetPlayer == null)
        {
            player.yellowMessage("Player '" + paramsValue[0] + "' has not been found on this channel.");
            return;
        }

        targetPlayer.setHair(hairId);
        targetPlayer.updateSingleStat(Stat.HAIR, hairId);
        targetPlayer.equipChanged();
    }
}
