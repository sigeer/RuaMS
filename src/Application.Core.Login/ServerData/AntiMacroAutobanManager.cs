using System.Collections.Concurrent;

namespace Application.Core.Login.ServerData;

/// <summary>
/// Master 端测谎失败计数器。
/// 跟踪每个玩家的测谎失败次数，达到阈值时由 MasterServer.ProcessAntiMacroPenalty 触发封禁。
/// </summary>
public class AntiMacroAutobanManager
{
    private readonly ConcurrentDictionary<int, int> _points = new();

    public int AddPoint(int victimId)
    {
        return _points.AddOrUpdate(victimId, 1, (_, count) => count + 1);
    }

    public int GetPoints(int victimId) => _points.GetValueOrDefault(victimId);

    public void ClearPoints(int victimId) => _points.TryRemove(victimId, out _);
}
