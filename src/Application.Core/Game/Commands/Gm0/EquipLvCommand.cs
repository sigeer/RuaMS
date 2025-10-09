using Application.Core.Managers;

namespace Application.Core.Game.Commands.Gm0;

public class EquipLvCommand : CommandBase
{
    public EquipLvCommand() : base(0, "equiplv")
    {
    }

    public override void Execute(IChannelClient c, string[] paramValues)
    {
        CharacterManager.ShowAllEquipFeatures(c.OnlinedCharacter);
    }
}
