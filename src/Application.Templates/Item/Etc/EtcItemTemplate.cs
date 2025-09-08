namespace Application.Templates.Item.Etc
{
    public class EtcItemTemplate : AbstractItemTemplate
    {
        [WZPath("info/lv")]
        public int lv { get; set; }
        public int Exp { get; set; }

        [WZPath("info/pickUpBlock")]
        public bool PickupBlock { get; set; }
        public EtcItemTemplate(int templateId) : base(templateId)
        {
        }
    }
}