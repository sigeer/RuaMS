namespace Application.Templates.Map
{
    public sealed class MapPortalTemplate : AbstractTemplate
    {
        public string? sTargetName { get; set; }

        public int nPortalType { get; set; }

        public int nTargetMap { get; set; }


        public string? sPortalName { get; set; }
        public int nX { get; set; }

        public int nY { get; set; }

        public string? Script { get; set; }


        public int nIndex { get; set; }

        public MapPortalTemplate(int nTemplateID)
            : base(nTemplateID)
        {
            nIndex = Convert.ToInt32(nTemplateID);
        }

        public override string ToString()
        {
            return $"{TemplateId} @ ( {nX}, {nY} ) -> {nTargetMap}";
        }
    }
}