namespace Application.Templates.Item.Etc
{
    [GenerateTag]
    public class EtcItemTemplate : ItemTemplateBase
    {
        [WZPath("info/lv")]
        public int lv { get; set; }
        [WZPath("info/exp")]
        public int Exp { get; set; }

        [WZPath("info/pickUpBlock")]
        public bool PickupBlock { get; set; }

        public EtcItemTemplate(int templateId) : base(templateId)
        {
        }
    }
}