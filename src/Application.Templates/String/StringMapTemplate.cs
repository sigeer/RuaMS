namespace Application.Templates.String
{
    public sealed class StringMapTemplate : StringTemplateBase
    {
        public StringMapTemplate(int templateId, string continent) : base(templateId)
        {
            StreetName = WzDefaults.WZ_NoName;
            MapName = WzDefaults.WZ_NoName;
            Continent = continent;
        }

        public override string Name { get => MapName; set => MapName = value; }

        [WZPath("streetName")]
        public string StreetName { get; set; }

        [WZPath("mapName")]
        public string MapName { get; set; }
        public string Continent { get; set; }
    }
}
