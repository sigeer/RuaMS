using Application.Templates.Map;
using System.Drawing;

namespace Application.Plugin.FakeCharacter;

// ================================================================
//  移动动作类型 —— Walk/Climb/Jump
// ================================================================

public enum MoveActionType { Walk, Climb, Jump }

public readonly struct MoveAction
{
    public MoveActionType Type { get; }
    public Point To { get; }

    public MoveAction(MoveActionType type, Point to)
    {
        Type = type;
        To = to;
    }
}

// ================================================================
//  地图导航图 —— 基于落脚点链（Foothold）和梯子（LadderRope）
//  构建可步行图，用 BFS 寻路，返回 Walk/Climb/Jump 动作序列
// ================================================================

/// <summary>
/// 地图导航图 —— 基于落脚点链和梯子构建可步行图，用 BFS 寻路。
///
/// 1. 将 Foothold 按 Next/Prev 连接关系聚合成"平台"（Platform）
/// 2. 每根梯子连接两个平台（顶端和底端）
/// 3. 检测平台之间的跳跃连接（跳下、平跳）
/// 4. BFS 从起点平台集搜索到终点平台集的最短路径
/// 5. 路径由 Walk（沿平台行走）、Climb（沿梯子攀爬）、Jump（跳跃）三种动作组成
///
/// 静态缓存：每张地图只构建一次导航图。
/// </summary>
public class MapNavigator
{
    /// <summary>一个可行走的平台 —— 由 Next/Prev 相连的 Foothold 线段组成</summary>
    public class Platform
    {
        public int Id;
        public List<MapFootholdTemplate> Footholds = new();
        public int MinX = int.MaxValue;
        public int MaxX = int.MinValue;

        public double? GetYAtX(int x)
        {
            foreach (var fh in Footholds)
            {
                int fhMinX = Math.Min(fh.X1, fh.X2);
                int fhMaxX = Math.Max(fh.X1, fh.X2);
                if (x >= fhMinX && x <= fhMaxX)
                    return CalculateFootholdY(fh, x);
            }
            return null;
        }

        public bool IsOnPlatform(Point p, int tolerance = 30)
        {
            if (p.X < MinX - tolerance || p.X > MaxX + tolerance) return false;
            var y = GetYAtX(p.X);
            return y.HasValue && Math.Abs(p.Y - y.Value) < tolerance;
        }
    }

    /// <summary>梯子连接信息 —— 记录一根梯子连接哪两个平台（-1 = 悬空端）</summary>
    public class LadderLink
    {
        public int LadderIndex;
        public MapLadderRopeTemplate Ladder = null!;
        public int PlatformBottom = -1;  // 底端平台索引，-1 = 悬空
        public int PlatformTop = -1;     // 顶端平台索引，-1 = 悬空
        public Point BottomPoint;
        public Point TopPoint;
    }

    /// <summary>跳跃连接 —— 记录两个平台之间可跳跃通过（无梯子）</summary>
    private class JumpConnection
    {
        public int FromPlatform;
        public int ToPlatform;
        public Point FromPoint;           // 在 FromPlatform 上的起跳点
        public Point ToPoint;             // 在 ToPlatform 上的落地点（或在梯子上的抓取点）
        public int? HangingLadderLinkIdx; // 跳→抓梯子时，指向要爬的 LadderLink
    }

    /// <summary>节点信息 —— BFS 的起终点描述</summary>
    private record NodeInfo(List<int> PlatformIndices, MapLadderRopeTemplate? OnLadder, int LadderLinkIdx);

    // ================================================================
    //  字段
    // ================================================================

    private readonly List<Platform> _platforms = new();
    private readonly List<LadderLink> _ladderLinks = new();
    private readonly List<JumpConnection> _jumpEdges = new();
    private readonly List<List<(int nextPlat, int linkIdx)>> _adj = new();

    private MapNavigator() { }

    // ================================================================
    //  静态缓存 —— 每张地图只构建一次
    // ================================================================

    private static readonly Dictionary<int, MapNavigator> _cache = new();
    private static readonly object _cacheLock = new();

    public static MapNavigator GetOrCreate(MapTemplate template, int mapId)
    {
        if (_cache.TryGetValue(mapId, out var nav))
            return nav;

        lock (_cacheLock)
        {
            if (!_cache.TryGetValue(mapId, out nav))
            {
                nav = Build(template);
                _cache[mapId] = nav;
            }
        }
        return nav;
    }

    // ================================================================
    //  Build —— 从 MapTemplate 构建导航图
    // ================================================================

    public static MapNavigator Build(MapTemplate template)
    {
        var nav = new MapNavigator();
        var fhById = template.Footholds.ToDictionary(f => f.Index);

        // Step 1: 将 Foothold 按 Next/Prev 连通性聚合成 Platform
        var visited = new HashSet<int>();

        foreach (var fh in template.Footholds)
        {
            if (visited.Contains(fh.Index)) continue;

            var platform = new Platform { Id = nav._platforms.Count };
            var stack = new Stack<int>();
            stack.Push(fh.Index);

            while (stack.Count > 0)
            {
                int curId = stack.Pop();
                if (!visited.Add(curId)) continue;
                if (!fhById.TryGetValue(curId, out var curFh)) continue;

                // Skip vertical walls — not walkable surfaces,
                // but still traverse through to find connecting segments
                if (curFh.X1 == curFh.X2 && curFh.Y1 != curFh.Y2)
                {
                    if (curFh.Next > 0 && !visited.Contains(curFh.Next)) stack.Push(curFh.Next);
                    if (curFh.Prev > 0 && !visited.Contains(curFh.Prev)) stack.Push(curFh.Prev);
                    continue;
                }

                platform.Footholds.Add(curFh);
                platform.MinX = Math.Min(platform.MinX, Math.Min(curFh.X1, curFh.X2));
                platform.MaxX = Math.Max(platform.MaxX, Math.Max(curFh.X1, curFh.X2));

                if (curFh.Next > 0 && !visited.Contains(curFh.Next)) stack.Push(curFh.Next);
                if (curFh.Prev > 0 && !visited.Contains(curFh.Prev)) stack.Push(curFh.Prev);
            }

            if (platform.Footholds.Count > 0)
            {
                nav._platforms.Add(platform);
            }
        }

        // Init adjacency
        nav._adj.AddRange(Enumerable.Range(0, nav._platforms.Count).Select(_ => new List<(int, int)>()));

        // Step 2: 为每根梯子找到上下端连接的 Platform
        // 允许单端悬空梯子（如从高处垂下的绳子）
        for (int li = 0; li < template.LadderRopes.Length; li++)
        {
            var ladder = template.LadderRopes[li];
            int topY = Math.Min(ladder.Y1, ladder.Y2);
            int bottomY = Math.Max(ladder.Y1, ladder.Y2);
            Point topPos = new(ladder.X, topY);
            Point bottomPos = new(ladder.X, bottomY);

            int? topPlatform = FindPlatformAt(nav._platforms, topPos);
            int? bottomPlatform = FindPlatformAt(nav._platforms, bottomPos);

            if (!topPlatform.HasValue && !bottomPlatform.HasValue)
                continue; // 两端都不着地，忽略

            var link = new LadderLink
            {
                LadderIndex = li,
                Ladder = ladder,
                PlatformBottom = bottomPlatform ?? -1,
                PlatformTop = topPlatform ?? -1,
                BottomPoint = bottomPos,
                TopPoint = topPos,
            };
            nav._ladderLinks.Add(link);
            int linkIdx = nav._ladderLinks.Count - 1;

            if (topPlatform.HasValue && bottomPlatform.HasValue && topPlatform != bottomPlatform)
            {
                // 标准梯子：两端连接不同平台
                nav._adj[topPlatform.Value].Add((bottomPlatform.Value, linkIdx));
                nav._adj[bottomPlatform.Value].Add((topPlatform.Value, linkIdx));
            }
            else if (topPlatform.HasValue && !bottomPlatform.HasValue)
            {
                // 悬空梯子：顶端在平台上，底端悬空
                // 检测下方平台能否跳起抓住梯子
                var topPlat = nav._platforms[topPlatform.Value];
                for (int pi = 0; pi < nav._platforms.Count; pi++)
                {
                    if (pi == topPlatform.Value) continue;
                    var jumpUp = DetectJumpUpToLadder(nav._platforms[pi], ladder, topPlat, link, linkIdx);
                    if (jumpUp != null)
                    {
                        nav._jumpEdges.Add(jumpUp.Value.conn);
                        int jumpIdx = nav._jumpEdges.Count - 1;
                        nav._adj[jumpUp.Value.fromPlat].Add((jumpUp.Value.toPlat, -(jumpIdx + 1)));
                    }
                }
            }
            // 注意：底端在平台、顶端悬空（从上往下跳的梯子）——暂不处理
        }

        // Step 3: 检测平台之间的跳跃连接
        var platforms = nav._platforms;
        for (int i = 0; i < platforms.Count; i++)
        {
            for (int j = 0; j < platforms.Count; j++)
            {
                if (i == j) continue;

                var conn = DetectJumpConnection(platforms[i], platforms[j]);
                if (conn != null)
                {
                    nav._jumpEdges.Add(conn);
                    int jumpIdx = nav._jumpEdges.Count - 1;
                    // linkIdx < 0 编码跳跃连接：-1 = jumpEdges[0], -2 = jumpEdges[1], ...
                    nav._adj[conn.FromPlatform].Add((conn.ToPlatform, -(jumpIdx + 1)));
                }
            }
        }

        return nav;
    }

    private static int? FindPlatformAt(List<Platform> platforms, Point p)
    {
        for (int i = 0; i < platforms.Count; i++)
            if (platforms[i].IsOnPlatform(p, tolerance: 40))
                return i;
        return null;
    }

    private static double CalculateFootholdY(MapFootholdTemplate fh, int x)
    {
        if (fh.X1 == fh.X2) return Math.Min(fh.Y1, fh.Y2);
        if (fh.Y1 == fh.Y2) return fh.Y1;
        return fh.Y1 + (x - fh.X1) * (double)(fh.Y1 - fh.Y2) / (fh.X1 - fh.X2);
    }

    // ================================================================
    //  跳跃连接检测
    // ================================================================

    /// <summary>
    /// 检测 platform A 是否能通过跳跃到达 platform B。
    ///
    /// 两种可跳跃情况：
    ///   跳下：X 范围重叠，A 在 B 上方 40~800px
    ///   平跳：水平间隙 &lt;250px，垂直差 &lt;80px
    /// </summary>
    private static JumpConnection? DetectJumpConnection(Platform a, Platform b)
    {
        int overlapMin = Math.Max(a.MinX, b.MinX);
        int overlapMax = Math.Min(a.MaxX, b.MaxX);
        {
            int midX = (overlapMin + overlapMax) / 2;
            double? yA = a.GetYAtX(midX);
            double? yB = b.GetYAtX(midX);

            if (yA.HasValue && yB.HasValue)
            {
                double gap = yB.Value - yA.Value; // Y 增大 = 向下
                if (gap > 40 && gap < 800)
                {
                    return new JumpConnection
                    {
                        FromPlatform = a.Id,
                        ToPlatform = b.Id,
                        FromPoint = new Point(midX, (int)yA.Value),
                        ToPoint = new Point(midX, (int)yB.Value),
                    };
                }
            }
        }

        // --- 平跳：水平接近，Y 相近 ---
        // b 在 a 右侧
        int gapX = b.MinX - a.MaxX;
        if (gapX > 0 && gapX < 250)
        {
            double? yEdge = a.GetYAtX(a.MaxX);
            double? yTarget = b.GetYAtX(b.MinX);
            if (yEdge.HasValue && yTarget.HasValue && Math.Abs(yTarget.Value - yEdge.Value) < 80)
            {
                return new JumpConnection
                {
                    FromPlatform = a.Id,
                    ToPlatform = b.Id,
                    FromPoint = new Point(a.MaxX, (int)yEdge.Value),
                    ToPoint = new Point(b.MinX, (int)yTarget.Value),
                };
            }
        }

        // b 在 a 左侧
        gapX = a.MinX - b.MaxX;
        if (gapX > 0 && gapX < 250)
        {
            double? yEdge = a.GetYAtX(a.MinX);
            double? yTarget = b.GetYAtX(b.MaxX);
            if (yEdge.HasValue && yTarget.HasValue && Math.Abs(yTarget.Value - yEdge.Value) < 80)
            {
                return new JumpConnection
                {
                    FromPlatform = a.Id,
                    ToPlatform = b.Id,
                    FromPoint = new Point(a.MinX, (int)yEdge.Value),
                    ToPoint = new Point(b.MaxX, (int)yTarget.Value),
                };
            }
        }

        return null;
    }

    // ================================================================
    //  跳起抓梯子检测
    // ================================================================

    /// <summary>
    /// 检测下方平台能否跳起抓住悬空梯子。
    /// 条件：梯子底端悬空，平台在梯子下方 200px 以内，水平 300px 以内。
    /// </summary>
    private static (int fromPlat, int toPlat, JumpConnection conn)? DetectJumpUpToLadder(
        Platform lowerPlat, MapLadderRopeTemplate ladder, Platform topPlat, LadderLink link, int linkIdx)
    {
        int bottomY = Math.Max(ladder.Y1, ladder.Y2);
        double? lowerY = lowerPlat.GetYAtX(ladder.X);

        if (!lowerY.HasValue)
        {
            int dl = Math.Abs(ladder.X - lowerPlat.MinX);
            int dr = Math.Abs(ladder.X - lowerPlat.MaxX);
            lowerY = lowerPlat.GetYAtX(dl <= dr ? lowerPlat.MinX : lowerPlat.MaxX);
        }

        if (!lowerY.HasValue) return null;

        double dx = Math.Abs(ladder.X - (lowerPlat.MinX + lowerPlat.MaxX) / 2);
        double dy = bottomY - lowerY.Value;

        if (dy > 0 && dy < 200 && dx < 300)
        {
            var conn = new JumpConnection
            {
                FromPlatform = lowerPlat.Id,
                ToPlatform = topPlat.Id,
                FromPoint = new Point(ladder.X, (int)lowerY.Value),
                ToPoint = new Point(ladder.X, bottomY),
                HangingLadderLinkIdx = linkIdx,
            };
            return (lowerPlat.Id, topPlat.Id, conn);
        }
        return null;
    }

    // ================================================================
    //  FindPath —— BFS 寻路，返回 Walk/Climb/Jump 动作序列
    // ================================================================

    /// <summary>
    /// BFS 寻路，返回 Walk/Climb/Jump 动作序列。
    /// 返回 null 表示无路径（调用方应使用 TeleportTo 兜底）。
    /// </summary>
    public List<MoveAction>? FindPath(Point from, Point to)
    {
        // 1. 获取起点和终点所在的平台/梯子信息
        var startInfo = GetNodeInfo(from);
        var endInfo = GetNodeInfo(to);
        if (startInfo == null || endInfo == null) return null;

        // Quick check: both on same platform — just walk
        if (startInfo.PlatformIndices.Count == 1 && endInfo.PlatformIndices.Count == 1
            && startInfo.PlatformIndices[0] == endInfo.PlatformIndices[0])
        {
            return new List<MoveAction> { new(MoveActionType.Walk, to) };
        }

        // 2. BFS on platform graph
        var parent = new Dictionary<int, (int parentPlat, int linkIdx)>();
        var queue = new Queue<int>();

        foreach (var sp in startInfo.PlatformIndices)
        {
            parent[sp] = (-1, -1);
            queue.Enqueue(sp);
        }

        int? foundEnd = null;
        HashSet<int> endSet = new(endInfo.PlatformIndices);

        while (queue.Count > 0 && foundEnd == null)
        {
            int cur = queue.Dequeue();
            foreach (var (nextPlat, linkIdx) in _adj[cur])
            {
                if (!parent.ContainsKey(nextPlat))
                {
                    parent[nextPlat] = (cur, linkIdx);
                    if (endSet.Contains(nextPlat))
                    {
                        foundEnd = nextPlat;
                        break;
                    }
                    queue.Enqueue(nextPlat);
                }
            }
        }

        if (foundEnd == null) return null;

        // 3. Reconstruct platform path: endPlat -> ... -> startPlat
        var platPath = new List<int>();
        int p = foundEnd.Value;
        while (p != -1)
        {
            platPath.Add(p);
            var (pp, _) = parent[p];
            p = pp;
        }
        platPath.Reverse();

        // 4. Build action list
        var actions = new List<MoveAction>();
        Point currentPos = from;

        // If start is on a ladder, climb to the first platform in the path
        if (startInfo.OnLadder != null && startInfo.LadderLinkIdx >= 0)
        {
            var startLink = _ladderLinks[startInfo.LadderLinkIdx];
            bool goTop = startLink.PlatformTop == platPath[0];
            Point ladderEnd = goTop ? startLink.TopPoint : startLink.BottomPoint;

            actions.Add(new(MoveActionType.Climb, ladderEnd));
            currentPos = ladderEnd;
        }

        // Walk/climb/jump through each adjacent platform pair
        for (int i = 0; i < platPath.Count - 1; i++)
        {
            int platA = platPath[i];
            int platB = platPath[i + 1];

            int linkIdx = int.MinValue;
            foreach (var (nextPlat, li) in _adj[platA])
            {
                if (nextPlat == platB) { linkIdx = li; break; }
            }
            if (linkIdx == int.MinValue) continue;

            if (linkIdx >= 0)
            {
                // Ladder connection
                var link = _ladderLinks[linkIdx];
                Point fromPt = link.PlatformTop == platA ? link.TopPoint : link.BottomPoint;
                Point toPt = link.PlatformTop == platB ? link.TopPoint : link.BottomPoint;

                if (Distance(currentPos, fromPt) > 5)
                    actions.Add(new(MoveActionType.Walk, fromPt));
                actions.Add(new(MoveActionType.Climb, toPt));
                currentPos = toPt;
            }
            else
            {
                // Jump connection
                var jump = _jumpEdges[-linkIdx - 1];
                if (Distance(currentPos, jump.FromPoint) > 5)
                    actions.Add(new(MoveActionType.Walk, jump.FromPoint));
                actions.Add(new(MoveActionType.Jump, jump.ToPoint));
                currentPos = jump.ToPoint;

                // Jump-to-ladder: 跳起抓住悬空梯子后，爬上去到达平台
                if (jump.HangingLadderLinkIdx.HasValue)
                {
                    var ladderLink = _ladderLinks[jump.HangingLadderLinkIdx.Value];
                    bool climbToTop = ladderLink.PlatformTop == platB;
                    Point climbTarget = climbToTop ? ladderLink.TopPoint : ladderLink.BottomPoint;
                    if (Distance(currentPos, climbTarget) > 5)
                        actions.Add(new(MoveActionType.Climb, climbTarget));
                    currentPos = climbTarget;
                }
            }
        }
        // Final segment to target
        if (endInfo.OnLadder != null && endInfo.LadderLinkIdx >= 0)
        {
            actions.Add(new(MoveActionType.Climb, to));
        }
        else
        {
            if (Distance(currentPos, to) > 5)
                actions.Add(new(MoveActionType.Walk, to));
        }

        return actions;
    }

    // ================================================================
    //  节点信息查询
    // ================================================================

    private NodeInfo? GetNodeInfo(Point p)
    {
        // Check ladders
        for (int li = 0; li < _ladderLinks.Count; li++)
        {
            var link = _ladderLinks[li];
            if (link.Ladder.Contains(p))
            {
                var indices = new List<int> { link.PlatformTop, link.PlatformBottom };
                return new NodeInfo(indices, link.Ladder, li);
            }
        }

        // Check platforms
        for (int i = 0; i < _platforms.Count; i++)
        {
            if (_platforms[i].IsOnPlatform(p, tolerance: 30))
            {
                return new NodeInfo(new List<int> { i }, null, -1);
            }
        }

        return null;
    }

    // ================================================================
    //  工具方法
    // ================================================================

    private static double Distance(Point a, Point b)
    {
        int dx = a.X - b.X;
        int dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
