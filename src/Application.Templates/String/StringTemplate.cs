namespace Application.Templates.String
{
    public sealed class StringTemplate : AbstractTemplate
    {
        [WZPath("name")]
        public string Name { get; set; }

        [WZPath("desc")]
        public string Description { get; set; }

        [WZPath("streetName")]
        public string StreetName { get; set; }

        [WZPath("mapName")]
        public string MapName { get; set; }

        public StringTemplate(int templateId)
            : base(templateId)
        {
            Name = "";
            Description = "";
            StreetName = "";
            MapName = "";
        }
    }
}
