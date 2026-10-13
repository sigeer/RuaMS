using System.ComponentModel;

namespace Application.Shared.MapObjects
{
    public enum VisionType : byte
    {
        /// <summary>
        /// 不可见
        /// </summary>
        [Description("不可见")]
        Invisible,
        /// <summary>
        /// 视野外
        /// </summary>
        [Description("视野外")]
        OutofVision,
        /// <summary>
        /// 视野内
        /// </summary>
        [Description("视野内")]
        InVision
    }
}
