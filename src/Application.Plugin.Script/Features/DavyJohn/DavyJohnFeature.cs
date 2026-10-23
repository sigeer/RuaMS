using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;

namespace Application.Plugin.Script.Features.DavyJohn
{
    /// <summary>
    /// 组队任务：海盗船（Davy John）。
    /// 原本分散在 Events/PartyQuest/PQ_Pirate.cs、Npc/PartyQuest/PQ_Pirate.cs、
    /// PortalScript.cs、ReactorActScript.cs、MapFirstEnterScript.cs 五处，
    /// 通过 eim 的属性键（"stage2"、"grindMode"、"openedBoxes" 等）跨文件通信。
    /// </summary>
    internal static class DavyJohnFeature
    {
        internal const string EventName = "PQ_Pirate";

        #region 地图
        internal const int Stage1Map = 925100000;
        internal const int Stage2Map = 925100100;
        internal const int Stage2PassageMap = 925100200;
        internal const int Stage2BossMap = 925100201;
        internal const int Stage2CheckMap = 925100202;
        internal const int Stage3PassageMap = 925100300;
        internal const int Stage3BossMap = 925100301;
        internal const int Stage3CheckMap = 925100302;
        internal const int Stage4Map = 925100400;
        internal const int BossMap = 925100500;
        internal const int ClearMapId = 925100600;
        internal const int ExitMapId = 925100700;
        internal const int RecruitMapId = 251010404;
        #endregion

        #region 道具 / 怪物
        /// <summary>
        /// 通关发放的 "Davy John's Letter" 之类纪念道具。
        /// </summary>
        internal const int ClearRewardItem = 4001158;

        internal const int SealedBossKey = 2511000;
        internal const int LastSealedBossKey = 2517999;

        internal const int GuardMobId = 9300109;
        internal const int GuardMobQuantity = 3;
        internal const int GruntMobId = 9300110;
        internal const int GruntMobQuantity = 5;

        internal const int WindflowerMobId1 = 9300124;
        internal const int WindflowerMobId2 = 9300125;
        internal const int WindflowerQuantity = 6;
        #endregion

        #region 属性键
        internal const string StackProperty = "level";
        internal const string Stage2Property = "stage2";
        internal const string Stage2AProperty = "stage2a";
        internal const string Stage3AProperty = "stage3a";
        internal const string GrindModeProperty = "grindMode";
        internal const string OpenedBoxesProperty = "openedBoxes";
        internal const string OpenedChestsProperty = "openedChests";
        internal const string SpawnedBossProperty = "spawnedBoss";
        #endregion

        #region 传送门

        /// <summary>
        /// 开启 "苦战模式" 时，必须激活全部宝箱反应堆才能推进。
        /// </summary>
        internal static bool PassedGrindMode(IMap map, AbstractEventInstanceManager eim)
        {
            if (eim.getIntProperty(GrindModeProperty) == 0)
            {
                return true;
            }

            return eim.activatedAllReactorsOnMap(map, SealedBossKey, LastSealedBossKey);
        }

        #endregion
    }
}
