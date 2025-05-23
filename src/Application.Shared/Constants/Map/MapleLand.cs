using System.Reflection;

namespace Application.Shared.Constants.Map
{
    public class MapleLand
    {
        public static string[] AllLands = typeof(MapleLand)
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.IsLiteral && x.IsStatic)
            .Select(x => x.GetValue(null)!.ToString()!).ToArray();

        /// <summary>
        /// 彩虹岛
        /// </summary>
        public const string Maple = "maple";
        /// <summary>
        /// 金银岛
        /// </summary>
        public const string Victoria = "victoria";
        /// <summary>
        /// 神秘岛、天空之城、阿里安特、神木村
        /// </summary>
        public const string Ossyria = "ossyria";
        /// <summary>
        /// 艾琳森林
        /// </summary>
        public const string Elin = "elin";
        /// <summary>
        /// 婚礼
        /// </summary>
        public const string WeddingGL = "weddingGL";
        /// <summary>
        /// 
        /// </summary>
        public const string MasteriaGL = "MasteriaGL";
        public const string HalloweenGL = "HalloweenGL";
        public const string JP = "jp";
        /// <summary>
        /// 活动地图、组队任务地图、自由市场
        /// </summary>
        public const string Etc = "etc";
        public const string Singapore = "singapore";
        public const string Event = "event";
        public const string Episode1GL = "Episode1GL";
        public const string Thai = "thai";
        public const string China = "china";
    }
}
