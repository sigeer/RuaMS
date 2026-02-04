namespace Application.Templates.Npc
{
    [GenerateTag]
    public sealed class NpcTemplate : AbstractTemplate
    {

        [WZPath("info/MapleTV")]
        public bool MapleTV { get; set; }

        [WZPath("info/script/0/script")]
        public string? Script { get; set; }


        [WZPath("info/trunkPut")]
        public int? TrunkPut { get; set; }
        [WZPath("info/trunkGet")]
        public int? TrunkGet { get; set; }
        public NpcTemplate(int templateId)
            : base(templateId)
        {
        }
    }
}
