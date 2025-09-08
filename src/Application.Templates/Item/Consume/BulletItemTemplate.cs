namespace Application.Templates.Item.Consume
{
    [GenerateTag]
    public sealed class BulletItemTemplate : ConsumeItemTemplate
    {
        public BulletItemTemplate(int templateId) : base(templateId)
        {
        }
        [WZPath("info/incPAD")]
        public int IncPAD { get; set; }
        [WZPath("info/reqLevel")]
        public int ReqLevel { get; set; }
    }
}
