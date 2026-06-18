using Application.Utility.Tickables;
using tools;

namespace Application.Core.Game.Maps;


public class Kite : AbstractMapObject, ILifedTickable
{
    private Point pos;
    public int OwnerId { get; }
    public string OwnerName { get; }

    public long ExpiredAt { get; }
    public TickableStatus Status { get; protected set; }

    private string text;
    private int ft;
    private int itemid;

    public Kite(Player owner, string text, int itemId) : base(owner.MapModel, owner.getPosition())
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


    public override async Task sendDestroyData(IChannelClient client)
    {
        await client.SendPacket(makeDestroyData());
    }

    public override async Task sendSpawnData(IChannelClient client)
    {
        await client.SendPacket(makeSpawnData());
    }

    public Packet makeSpawnData()
    {
        return PacketCreator.spawnKite(getObjectId(), itemid, OwnerName, text, pos, ft);
    }

    public Packet makeDestroyData()
    {
        return PacketCreator.removeKite(getObjectId(), 0);
    }

    public Task OnTick(long now)
    {
        if (!this.IsAvailable())
        {
            return Task.CompletedTask;
        }

        if (ExpiredAt <= now)
        {
            Status = TickableStatus.Remove;
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }

    public override bool IsVisibleForPlayer(Player chr)
    {
        return base.IsVisibleForPlayerWithoutRange(chr);
    }
}