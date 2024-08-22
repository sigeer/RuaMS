

using client;
using net.packet;
using tools;

namespace server.maps;



public class Kite : AbstractMapObject
{
    private Point pos;
    private Character owner;
    private string text;
    private int ft;
    private int itemid;

    public Kite(Character owner, string text, int itemId)
    {
        this.owner = owner;
        this.pos = owner.getPosition();
        this.ft = owner.getFh();
        this.text = text;
        this.itemid = itemId;
    }

    public override MapObjectType getType()
    {
        return MapObjectType.KITE;
    }

    public override Point getPosition()
    {
        return pos;
    }

    public Character getOwner()
    {
        return owner;
    }

    public override void sendDestroyData(Client client)
    {
        client.sendPacket(makeDestroyData());
    }

    public override void sendSpawnData(Client client)
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