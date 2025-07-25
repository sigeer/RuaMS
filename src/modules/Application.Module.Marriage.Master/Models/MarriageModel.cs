using Application.Core.Login.Shared;

namespace Application.Module.Marriage.Master.Models
{
    public class MarriageModel: ITrackableEntityKey<int>
    {
        public int Id { get; set; }

        public int Husbandid { get; set; }

        public int Wifeid { get; set; }

        /// <summary>
        /// 0. 订婚 1. 结婚 2. 离婚
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 订婚时间
        /// </summary>
        public DateTimeOffset Time0 { get; set; }
        /// <summary>
        /// 结婚时间
        /// </summary>
        public DateTimeOffset? Time1 { get; set; }
        /// <summary>
        /// 离婚时间
        /// </summary>
        public DateTimeOffset? Time2 { get; set; }
    }
}
