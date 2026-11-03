using Application.Core.Client;
using Application.Core.scripting.Events.Abstraction;
using Application.Shared.Constants.Map;

namespace Application.Plugin.Script.Features.Dojo
{
    /// <summary>
    /// Dojo 功能内共用的常量。
    /// </summary>
    internal static class DojoFeature
    {
        // 必须先进行一次单人挑战
        internal const string PreEventName = "DojoPre";

        internal const string SoloEventName = "DojoSolo";
        internal const string PartyEventName = "DojoParty";

        internal const int RecruitMap = MapId.MU_LUNG_DOJO_HALL;
        internal const int ExitMap = MapId.DOJO_EXIT;

        /// <summary>最后一关通关后的结算地图。</summary>
        internal const int ClearMap = 925020003;

        /// <summary>每个道馆的最后一关（共 38 关）。</summary>
        internal const int MaxStage = 38;

        /// <summary>BOSS 死亡后变成的隐形怪，它的出现代表该关卡已通关。</summary>
        internal const int HiddenMob = 9300216;

        /// <summary>关卡怪物的起始 Id（9300183 + 实际关卡）。</summary>
        internal const int StageMobBaseId = 9300183;

        internal const int MapEffectItem = 5120024;

        public static string? HandleCreateInstanceResult(CreateInstanceResult r, IChannelClient c)
        {
            switch (r)
            {
                case CreateInstanceResult.Success:
                    return null;
                case CreateInstanceResult.RequiredParty:
                case CreateInstanceResult.RequiredLeader:
                    return "你以为你要去哪里？你甚至不是队伍的领袖！去告诉你的队伍领袖来找我谈话。";
                case CreateInstanceResult.Requirement:
                    return "你的队伍成员等级范围太广，无法进入。请确保你的所有队伍成员等级相差不超过#r30级#k。";
                case CreateInstanceResult.LobbyLimited:
                    return "所有道馆都已经被使用了。请稍等一会再尝试。";
                case CreateInstanceResult.Disposed:
                case CreateInstanceResult.Unknown:
                    return "你的队伍可能已经在使用道馆，或者你的队伍在道馆的允许时间还没有结束。请等待他们完成后再进入。";
                default:
                    return "未知错误";
            }
        }

        /// <summary>
        /// 地图所属的关卡（第 1 ~ 38 关）。大厅、出口等第 0 层地图返回 0。
        /// </summary>
        public static int GetStage(int mapId)
        {
            return (mapId / 100) % 100;
        }

        /// <summary>
        /// 休息关：每 6 关一次。只有道馆内的战斗地图才算，大厅（第 0 层）与道馆外的地图都不算。
        /// </summary>
        public static bool IsRestingSpot(int mapId)
        {
            var stage = GetStage(mapId);
            return stage > 0 && stage % 6 == 0 && MapId.isDojo(mapId);
        }

        /// <summary>
        /// 当前关卡的限时（毫秒）：前 36 关随层数递增，最后两关固定 15 分钟。
        /// </summary>
        public static long GetStageTime(int stage)
        {
            return (stage > 36 ? 15 : (stage / 6) + 5) * 60000L;
        }

        /// <summary>
        /// 关卡怪物的起始 Id（9300183 + 实际关卡）。休息关没有 BOSS。
        /// </summary>
        public static int GetStageMobId(int stage)
        {
            // 减去此前经历的休息关，得到怪物编号上的实际序号
            return StageMobBaseId + GetMobStage(stage);
        }

        public static int GetMobStage(int stage)
        {
            // 减去此前经历的休息关，得到怪物编号上的实际序号
            return stage - (stage / 6);
        }

        /// <summary>
        /// 该关卡的地图：从 <paramref name="baseMap"/> 起按 100 递进。
        /// </summary>
        public static int GetStageMapId(int baseMap, int stage)
        {
            return baseMap + stage * 100;
        }
    }
}
