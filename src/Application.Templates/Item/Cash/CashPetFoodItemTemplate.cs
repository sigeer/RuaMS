namespace Application.Templates.Item.Cash
{
    /// <summary>
    /// 524
    /// </summary>
    [GenerateTag]
    public class CashPetFoodItemTemplate : CashItemTemplate
    {
        public CashPetFoodItemTemplate(int templateId) : base(templateId)
        {
            Pet = Array.Empty<int>();
        }

        [WZPath("spec/inc")]
        public int PetfoodInc { get; set; }
        [WZPath("spec/-")]
        public int[] Pet { get; set; }
    }
}
