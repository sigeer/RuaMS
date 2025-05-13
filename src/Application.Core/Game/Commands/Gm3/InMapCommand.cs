using Application.Core.scripting.npc;

namespace Application.Core.Game.Commands.Gm3;

public class InMapCommand : CommandBase
{
    public InMapCommand() : base(3, "inmap")
    {
        Description = "Show all players in the map.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;

        string st = string.Join("\r\n", player.getMap().getCharacters());
        TempConversation.Create(c)?.RegisterTalk(st);

    }
}
