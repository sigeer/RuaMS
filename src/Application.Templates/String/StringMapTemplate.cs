namespace Application.Templates.String
{
    public sealed class StringMapTemplate : AbstractTemplate
    {
        public StringMapTemplate(int templateId) : base(templateId)
        {
            StreetName = Defaults.WZ_NoName;
            MapName = Defaults.WZ_NoName;
        }

        [WZPath("streetName")]
        public string StreetName { get; set; }

        [WZPath("mapName")]
        public string MapName { get; set; }
    }
}
