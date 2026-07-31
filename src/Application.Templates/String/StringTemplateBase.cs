namespace Application.Templates.String
{
    public class StringTemplateBase : AbstractTemplate
    {
        public StringTemplateBase(int templateId) : base(templateId)
        {
            Name = WzDefaults.WZ_NoName;
        }

        [WZPath("name")]
        public virtual string Name { get; set; }
    }
}
