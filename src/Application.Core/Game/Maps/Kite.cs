using tools;

namespace Application.Core.Game.Maps;


public class Kite : AbstractMapObject
{
    private Point pos;
    private Player owner;
    private string text;
    private int ft;
    private int itemid;

    public Kite(Player owner, string text, int itemId)
    {
        this.owner = owner;
        pos = owner.getPosition();
        ft = owner.getFh();
        this.text = text;
        itemid = itemId;
    }

    public override MapObjectType getType()
    {
        return MapObjectType.KITE;
    }

    public override Point getPosition()
    {
        return pos;
    }

    public Player getOwner()
    {
        return owner;
    }

    public override void sendDestroyData(IChannelClient client)
    {
        client.sendPacket(makeDestroyData());
    }

    public override void sendSpawnData(IChannelClient client)
    {
        client.sendPacket(makeSpawnData());
    }

    public Packet makeSpawnData()
    {
        return PacketCreator.spawnKite(getObjectId(), itemid, owner.getName(), text, pos, ft);
    }

    public Packet makeDestroyData()
    {
        return PacketCreator.removeKite(getObjectId(), 0);
    }
}