using Application.Core.Channel.Commands;
using net.server.services.task.channel;
using tools;

namespace Application.Core.Game.Maps.Mists;

public abstract class Mist : AbstractMapObject
{
    protected Rectangle mistPosition;
    protected bool _isMobMist, _isPoisonMist, _isRecoveryMist;
    protected int skillDelay;

    public bool IsFake { get; set; }

    public Mist(IMap map, Rectangle mistPosition, int skillDelay)
    {
        setMap(map);
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

    public override void sendSpawnData(IChannelClient client)
    {
        client.sendPacket(makeSpawnData());
    }

    public override void sendDestroyData(IChannelClient client)
    {
        client.sendPacket(makeDestroyData());
    }

    public int getSkillDelay()
    {
        return skillDelay;
    }

    public override void Enter(IMap map, Action<Player> chrAction)
    {
        base.Enter(map, chrAction);

        MobMistService service = map.getChannelServer().MobMistService;
        service.registerMobMistCancelAction(map.Id, new MapMistRemoveCommand(this), getSkillDelay());
    }
}
