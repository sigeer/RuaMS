namespace Application.Templates.Npc
{
    public sealed class NpcTemplate : AbstractTemplate
    {

        [WZPath("info/MapleTV")]
        public bool MapleTV { get; set; }

        [WZPath("info/script/0/script")]
        public string? Script { get; set; }


        [WZPath("trunkPut")]
        public int TrunkPut { get; set; }
        public NpcTemplate(int templateId)
            : base(templateId)
        {
            Script = "";
        }
    }
}
