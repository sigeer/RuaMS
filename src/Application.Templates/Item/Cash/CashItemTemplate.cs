namespace Application.Templates.Item.Cash
{
    [GenerateTag]
    public class CashItemTemplate : ItemTemplateBase
    {


        [WZPath("info/protectTime")]
        public int ProtectTime { get; set; }

        public CashItemTemplate(int templateId)
            : base(templateId) { }
    }
}