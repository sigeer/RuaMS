namespace Application.Templates.Item
{
    /// <summary>
    /// 用于和装备类型物品区分
    /// </summary>
    public class ItemTemplateBase : AbstractItemTemplate
    {
        public ItemTemplateBase(int templateId) : base(templateId)
        {
        }

        [WZPath("info/mcType")]
        public int MCType { get; set; }
        [WZPath("info/pquest")]
        public bool PartyQuest { get; set; }

        [WZPath("info/max")]
        public int Max { get; set; }
    }
}
