namespace Application.Templates.Item.Cash
{
    /// <summary>
    /// 520
    /// </summary>
    [GenerateTag]
    public sealed class MesoBagItemTemplate : CashItemTemplate
    {
        public MesoBagItemTemplate(int templateId) : base(templateId)
        {

        }

        [WZPath("info/meso")]
        public int Meso { get; set; }
    }
}
