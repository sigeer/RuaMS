namespace XmlWzReader.WzEntity
{
    public class MapPortalWz
    {
        public MapPortalWz(int mapId, Data portalData)
        {
            MapId = mapId;
            Pt = DataTool.getInt(portalData.getChildByPath("pt"));

            Pn = DataTool.getString(portalData.getChildByPath("pn"));
            Tn = DataTool.getString(portalData.getChildByPath("tn"));
            Tm = (DataTool.getInt(portalData.getChildByPath("tm")));
            X = DataTool.getInt(portalData.getChildByPath("x"));
            Y = DataTool.getInt(portalData.getChildByPath("y"));
            PortalId = int.Parse(portalData.getName());
            Script = DataTool.getString("script", portalData);
        }
        public int Id { get; set; }
        public int MapId { get; set; }
        public int PortalId { get; set; }
        /// <summary>
        /// type
        /// </summary>
        public int Pt { get; set; }
        public string Pn { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Tm { get; set; }
        public string? Tn { get; set; }
        public string? Script { get; set; }
    }
}
