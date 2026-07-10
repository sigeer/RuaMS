namespace Application.Templates.Map
{
    public sealed class MapPortalTemplate
    {
        [WZPath("~/tn")]
        public string? sTargetName { get; set; }

        [WZPath("~/pt")]
        public int nPortalType { get; set; }
        [WZPath("~/tm")]
        public int nTargetMap { get; set; }

        [WZPath("~/pn")]
        public string? sPortalName { get; set; }
        [WZPath("~/x")]
        public int nX { get; set; }
        [WZPath("~/y")]
        public int nY { get; set; }
        [WZPath("~/script")]
        public string? Script { get; set; }

        [WZPath("~/$name")]
        public int nIndex { get; set; }
    }
}