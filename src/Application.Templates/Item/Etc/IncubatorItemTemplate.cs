namespace Application.Templates.Item.Etc
{
    [GenerateTag]
    public class IncubatorItemTemplate : EtcItemTemplate
    {
        public IncubatorItemTemplate(int templateId) : base(templateId)
        {
            ConsumeItems = Array.Empty<IncubatorConsumeItem>();
        }

        [WZPath("info/grade")]
        public int Grade { get; set; }
        [WZPath("info/hybrid")]
        public bool Hybrid { get; set; }

        [WZPath("info/questId")]
        public int QuestID { get; set; }
        [WZPath("info/uiData/$existed")]
        public bool HasUIData { get; set; }

        [WZPath("info/consumeItem/-")]
        public IncubatorConsumeItem[] ConsumeItems { get; set; }

    }

    public class IncubatorConsumeItem
    {
        [WZPath("info/consumeItem/-/0")]
        public int ItemId { get; set; }
        [WZPath("info/consumeItem/-/1")]
        public int Value { get; set; }
    }

}
