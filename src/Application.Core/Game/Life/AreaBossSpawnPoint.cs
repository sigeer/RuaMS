using Application.Core.Game.Maps;
using server.life;

namespace Application.Core.Game.Life;

public class AreaBossSpawnPoint : SpawnPoint
{
    List<RandomPoint> _points;
    public string Name { get; }
    public AreaBossSpawnPoint(
        string name,
        IMap map,
        int mobId,
        List<RandomPoint> pos,
        int mobTime, int mobInterval) : base(map, mobId,
            Point.Empty, 0, 0, 0, 0, 0, false, -1,
            mobTime, mobInterval,
            SpawnPointTrigger.Cleared)
    {
        Name = name;
        _points = pos;
    }

    protected override void SetMonsterPosition(Monster mob)
    {
        mob.setPosition(Randomizer.Select(_points).GetPoint());
    }

}
