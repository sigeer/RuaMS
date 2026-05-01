using Application.Templates.Map;

namespace Application.Templates.Npc
{
    [GenerateTag]
    public sealed class NpcTemplate : AbstractTemplate, ILinkTemplate<NpcTemplate>
    {
        [WZPath("info/link")]
        public int LinkId { get; set; }

        [WZPath("info/MapleTV")]
        public bool MapleTV { get; set; }

        [WZPath("info/script/0/script")]
        public string? Script { get; set; }


        [WZPath("info/trunkPut")]
        public int? TrunkPut { get; set; }
        [WZPath("info/trunkGet")]
        public int? TrunkGet { get; set; }
        [WZPath("info/guildRank")]
        public bool GuildRank { get; set; }
        [WZPath("info/parcel")]
        public bool Parcel { get; set; }
        public NpcTemplate(int templateId)
            : base(templateId)
        {
        }
        public void CloneLink(NpcTemplate sourceTemplate)
        {
            sourceTemplate.Script = Script;
            sourceTemplate.TrunkPut = TrunkPut;
            sourceTemplate.TrunkGet = TrunkGet;
            sourceTemplate.MapleTV = MapleTV;
            sourceTemplate.Parcel = Parcel;
            sourceTemplate.GuildRank = GuildRank;
        }
    }
}
