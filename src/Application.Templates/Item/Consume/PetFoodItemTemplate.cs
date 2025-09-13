namespace Application.Templates.Item.Consume
{
    /// <summary>
    /// 212
    /// </summary>
    [GenerateTag]
    public sealed class PetFoodItemTemplate : ConsumeItemTemplate
    {
        public PetFoodItemTemplate(int templateId) : base(templateId)
        {
            Pet = Array.Empty<int>();
        }

        [WZPath("spec/inc")]
        public int PetfoodInc { get; set; }
        [WZPath("spec/-")]
        public int[] Pet { get; set; }
    }
}
