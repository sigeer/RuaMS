using Application.Core.Channel.DataProviders;
using client.inventory;
using client.inventory.manipulator;
using tools;

namespace server.minigame;

/**
 * @Author Arnah
 * @Website http://Vertisy.ca/
 * @since Aug 15, 2016
 */
public class RockPaperScissor
{
    private int round = 0;
    private bool ableAnswer = true;
    private bool win = false;

    byte mode;
    public RockPaperScissor(IChannelClient c, byte mode)
    {

        this.mode = mode;
    }
    public async Task Initialize(IChannelClient c)
    {
        await c.SendPacket(PacketCreator.rpsMode((byte)(9 + mode)));
        if (mode == 0)
        {
            await c.OnlinedCharacter.GainMeso(-1000, GainItemShow.ShowInChat, true);
        }
    }

    public async Task<bool> answer(IChannelClient c, int answer)
    {
        if (ableAnswer && !win && answer >= 0 && answer <= 2)
        {
            int response = Randomizer.nextInt(3);
            if (response == answer)
            {
                await c.SendPacket(PacketCreator.rpsSelection((byte)response, (sbyte)round));
                // dont do anything. they can still answer once a draw
            }
            else if ((answer == 0 && response == 2) || (answer == 1 && response == 0) || (answer == 2 && response == 1))
            { // they win
                await c.SendPacket(PacketCreator.rpsSelection((byte)response, (sbyte)(round + 1)));
                ableAnswer = false;
                win = true;
            }
            else
            { // they lose
                await c.SendPacket(PacketCreator.rpsSelection((byte)response, -1));
                ableAnswer = false;
            }
            return true;
        }
        await reward(c);
        return false;
    }

    public async Task<bool> timeOut(IChannelClient c)
    {
        if (ableAnswer && !win)
        {
            ableAnswer = false;
            await c.SendPacket(PacketCreator.rpsMode(0x0A));
            return true;
        }
        await reward(c);
        return false;
    }

    public async Task<bool> nextRound(IChannelClient c)
    {
        if (win)
        {
            round++;
            if (round < 10)
            {
                win = false;
                ableAnswer = true;
                await c.SendPacket(PacketCreator.rpsMode(0x0C));
                return true;
            }
            else
            {
                round = 10;
            }
        }
        await reward(c);
        return false;
    }

    public async Task reward(IChannelClient c)
    {
        if (win)
        {
            var item = ItemInformationProvider.getInstance().GenerateVirtualItemById(ItemId.RPS_CERTIFICATE_BASE + round, 1);
            await InventoryManipulator.addFromDrop(c, item, true);
        }
        c.OnlinedCharacter.setRPS(null);
    }

    public async Task dispose(IChannelClient c)
    {
        await reward(c);
        await c.SendPacket(PacketCreator.rpsMode(0x0D));
    }
}
