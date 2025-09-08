namespace Application.Templates.Item.Consume
{
    /// <summary>
    /// 有普通消耗品（212），也有现金道具（524）
    /// </summary>
    [GenerateTag]
    public sealed class PetFoodItemTemplate : ConsumeItemTemplate
    {
        public PetFoodItemTemplate(int templateId) : base(templateId)
        {
            Pet = Array.Empty<int>();
        }

        /// <summary>
        /// 212 提升饱食度，524提升亲密度
        /// </summary>
        public int IncType { get; set; }
        [WZPath("spec/inc")]
        public int PetfoodInc { get; set; }
        [WZPath("spec/-")]
        public int[] Pet { get; set; }
    }
}
