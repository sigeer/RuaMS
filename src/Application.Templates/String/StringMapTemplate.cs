namespace Application.Templates.String
{
    public sealed class StringMapTemplate : StringTemplateBase
    {
        public StringMapTemplate(int templateId) : base(templateId)
        {
            StreetName = WzDefaults.WZ_NoName;
            MapName = WzDefaults.WZ_NoName;
        }

        public override string Name { get => MapName; set => MapName = value; }

        [WZPath("streetName")]
        public string StreetName { get; set; }

        [WZPath("mapName")]
        public string MapName { get; set; }
    }
}
