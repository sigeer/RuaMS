using Application.Shared.Constants;

namespace Application.Plugin.Events
{
    internal record AreaBossOption(string Name, int MapId, int BossId, int Period, string Message, List<RandomPoint> Point);
}
