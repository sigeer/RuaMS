using Application.Module.Marriage.Common.Models;

namespace Application.Module.Marriage.Channel.Models
{
    public class MarriageInfo
    {
        public int Id { get; set; }
        public int RingSourceId { get; set; }
        public int HusbandId { get; set; }
        public string HusbandName { get; set; }
        public int WifeId { get; set; }
        public string WifeName { get; set; }

        /// <summary>
        /// 0. 订婚 1. 结婚 2. 离婚
        /// </summary>
        public MarriageStatusEnum Status { get; set; }
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

        public int GetPartnerId(int chrId)
        {
            return HusbandId == chrId ? WifeId : HusbandId;
        }
    }
}
