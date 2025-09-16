namespace Application.Templates.Map
{
    public sealed class MapPortalTemplate
    {
        [WZPath("portal/-/tn")]
        public string? sTargetName { get; set; }

        [WZPath("portal/-/pt")]
        public int nPortalType { get; set; }
        [WZPath("portal/-/tm")]
        public int nTargetMap { get; set; }

        [WZPath("portal/-/pn")]
        public string? sPortalName { get; set; }
        [WZPath("portal/-/x")]
        public int nX { get; set; }
        [WZPath("portal/-/y")]
        public int nY { get; set; }
        [WZPath("portal/-/script")]
        public string? Script { get; set; }

        [WZPath("portal/-/$name")]
        public int nIndex { get; set; }
    }
}