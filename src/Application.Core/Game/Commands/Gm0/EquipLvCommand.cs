using Application.Core.Managers;

namespace Application.Core.Game.Commands.Gm0;

public class EquipLvCommand : CommandBase
{
    public EquipLvCommand() : base(0, "equiplv")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramValues)
    {
        await CharacterManager.ShowAllEquipFeatures(c.OnlinedCharacter);
    }
}
