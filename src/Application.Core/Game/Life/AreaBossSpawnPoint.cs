using Application.Core.Game.Maps;
using Application.Core.Server.life;

namespace Application.Core.Game.Life;

public class AreaBossSpawnPoint : SpawnPoint
{
    List<RandomPoint> _points;
    string _spawnMessage;
    private readonly string _messageCategory;
    public string Name { get; }
    public AreaBossSpawnPoint(
        string name,
        IMap map,
        int mobId,
        List<RandomPoint> pos,
        int mobTime, int mobInterval, string messageCategory, string spawnMessage) : base(map, mobId,
            Point.Empty, 0, 0, 0, 0, 0, false, -1,
            mobTime, mobInterval)
    {
        Name = name;
        _points = pos;
        _messageCategory = messageCategory;
        _spawnMessage = spawnMessage;
    }


    protected override Point GetPosition() => Randomizer.Select(_points).GetPoint();

    protected override void SubscribeMonster(Monster mob)
    {
        mob.OnSpawned += async (sender, obj) =>
        {
            await _map.TypedMessage(NoticeType.LightBlue, chr => chr.Client.CurrentCulture.GetMessageByCategory(_messageCategory, _spawnMessage));
        };
    }
}
