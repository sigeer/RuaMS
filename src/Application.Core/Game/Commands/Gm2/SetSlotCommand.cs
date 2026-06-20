using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm2;

public class SetSlotCommand : CommandBase
{
    public SetSlotCommand() : base(2, "setslot")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            await player.Yellow(nameof(ClientMessage.SetSlotCommand_Syntax));
            return;
        }

        int slots = (int.Parse(paramsValue[0]) / 4) * 4;
        for (int i = 1; i < 5; i++)
        {
            int curSlots = player.getSlots(i);
            if (slots <= -curSlots)
            {
                continue;
            }

            await player.gainSlots(i, slots - curSlots, true);
        }

        await player.Yellow(nameof(ClientMessage.Command_Done), player.getLastCommandMessage());
    }
}
