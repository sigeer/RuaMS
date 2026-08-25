namespace Application.Templates.Item.Cash
{
    [GenerateTag]
    public sealed class ItemGuardTemplate : ItemTemplateBase
    {
        public ItemGuardTemplate(int templateId) : base(templateId)
        {
        }

        [WZPath("info/protectTime")]
        public int ProtectTime { get; set; }
    }
}
