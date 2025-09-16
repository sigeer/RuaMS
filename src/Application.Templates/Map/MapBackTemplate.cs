namespace Application.Templates.Map
{
    public class MapBackTemplate
    {
        [WZPath("back/-/$name")]
        public int Index { get; set; }
        public int Type { get; set; }
    }
}
