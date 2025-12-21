using Application.Core.Game.Maps;
using server.life;

namespace Application.Core.Game.Life;

public class AreaBossSpawnPoint : SpawnPoint
{
    List<RandomPoint> _points;
    string _spawnMessage;
    public string Name { get; }
    public AreaBossSpawnPoint(
        string name,
        IMap map,
        int mobId,
        List<RandomPoint> pos,
        int mobTime, int mobInterval, string spawnMessage) : base(map, mobId,
            Point.Empty, 0, 0, 0, 0, 0, false, -1,
            mobTime, mobInterval,
            SpawnPointTrigger.Cleared)
    {
        Name = name;
        _points = pos;
        _spawnMessage = spawnMessage;
    }

    protected override void SetMonsterPosition(Monster mob)
    {
        mob.setPosition(Randomizer.Select(_points).GetPoint());
    }

    protected override void SubscribeMonster(Monster mob)
    {
        mob.OnSpawned += (sender, obj) =>
        {
            _map.LightBlue(_spawnMessage);
        };
    }
}
