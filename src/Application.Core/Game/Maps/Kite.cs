using Application.Utility.Tickables;
using tools;

namespace Application.Core.Game.Maps;


public class Kite : AbstractMapObject, ILifedTickable
{
    private Point pos;
    public int OwnerId { get; }
    public string OwnerName { get; }

    public long ExpiredAt { get; }

    public bool IsTickableCancelled { get; set; }

    private string text;
    private int ft;
    private int itemid;

    public Kite(Player owner, string text, int itemId)
    {
        OwnerId = owner.Id;
        OwnerName = owner.Name;
        pos = owner.getPosition();
        ft = owner.getFh();
        this.text = text;
        itemid = itemId;

        ExpiredAt = owner.getChannelServer().Node.getCurrentTime() + YamlConfig.config.server.KITE_EXPIRE_TIME;
    }

    public override MapObjectType getType()
    {
        return MapObjectType.KITE;
    }

    public override Point getPosition()
    {
        return pos;
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
        return PacketCreator.spawnKite(getObjectId(), itemid, OwnerName, text, pos, ft);
    }

    public Packet makeDestroyData()
    {
        return PacketCreator.removeKite(getObjectId(), 0);
    }

    public void OnTick(long now)
    {
        if (IsTickableCancelled)
        {
            return;
        }

        if (ExpiredAt <= now)
        {
            IsTickableCancelled = true;
            return;
        }
    }
}