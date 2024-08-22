

using client;
using client.inventory;
using client.inventory.manipulator;
using constants.id;
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

    public RockPaperScissor(Client c, byte mode)
    {
        c.sendPacket(PacketCreator.rpsMode((byte)(9 + mode)));
        if (mode == 0)
        {
            c.getPlayer().gainMeso(-1000, true, true, true);
        }
    }

    public bool answer(Client c, int answer)
    {
        if (ableAnswer && !win && answer >= 0 && answer <= 2)
        {
            int response = Randomizer.nextInt(3);
            if (response == answer)
            {
                c.sendPacket(PacketCreator.rpsSelection((byte)response, (sbyte)round));
                // dont do anything. they can still answer once a draw
            }
            else if ((answer == 0 && response == 2) || (answer == 1 && response == 0) || (answer == 2 && response == 1))
            { // they win
                c.sendPacket(PacketCreator.rpsSelection((byte)response, (sbyte)(round + 1)));
                ableAnswer = false;
                win = true;
            }
            else
            { // they lose
                c.sendPacket(PacketCreator.rpsSelection((byte)response, -1));
                ableAnswer = false;
            }
            return true;
        }
        reward(c);
        return false;
    }

    public bool timeOut(Client c)
    {
        if (ableAnswer && !win)
        {
            ableAnswer = false;
            c.sendPacket(PacketCreator.rpsMode(0x0A));
            return true;
        }
        reward(c);
        return false;
    }

    public bool nextRound(Client c)
    {
        if (win)
        {
            round++;
            if (round < 10)
            {
                win = false;
                ableAnswer = true;
                c.sendPacket(PacketCreator.rpsMode(0x0C));
                return true;
            }
            else
            {
                round = 10;
            }
        }
        reward(c);
        return false;
    }

    public void reward(Client c)
    {
        if (win)
        {
            InventoryManipulator.addFromDrop(c, new Item(ItemId.RPS_CERTIFICATE_BASE + round, 0, 1), true);
        }
        c.getPlayer().setRPS(null);
    }

    public void dispose(Client c)
    {
        reward(c);
        c.sendPacket(PacketCreator.rpsMode(0x0D));
    }
}
