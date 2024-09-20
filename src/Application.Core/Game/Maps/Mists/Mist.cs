using net.packet;
using server.maps;
using tools;

namespace Application.Core.Game.Maps.Mists;

public abstract class Mist : AbstractMapObject
{
    protected Rectangle mistPosition;
    protected bool _isMobMist, _isPoisonMist, _isRecoveryMist;
    protected int skillDelay;

    public Mist(Rectangle mistPosition, int skillDelay)
    {
        this.mistPosition = mistPosition;
        this.skillDelay = skillDelay;
    }


    public override MapObjectType getType()
    {
        return MapObjectType.MIST;
    }

    public override Point getPosition()
    {
        return mistPosition.Location;
    }


    public bool isMobMist()
    {
        return _isMobMist;
    }

    public bool isPoisonMist()
    {
        return _isPoisonMist;
    }

    public bool isRecoveryMist()
    {
        return _isRecoveryMist;
    }


    public Rectangle getBox()
    {
        return mistPosition;
    }

    public override void setPosition(Point position)
    {
        throw new NotImplementedException();
    }

    public Packet makeDestroyData()
    {
        return PacketCreator.removeMist(getObjectId());
    }

    public abstract Packet makeSpawnData();
    public abstract Packet makeFakeSpawnData(int level);

    public override void sendSpawnData(IClient client)
    {
        client.sendPacket(makeSpawnData());
    }

    public override void sendDestroyData(IClient client)
    {
        client.sendPacket(makeDestroyData());
    }

    public int getSkillDelay()
    {
        return skillDelay;
    }

}
