namespace Application.Templates.Item.Consume
{
    /// <summary>
    /// 238
    /// </summary>
    public class MonsterCardItemTemplate : ConsumeItemTemplate
    {
        public MonsterCardItemTemplate(int templateId) : base(templateId)
        {
        }

        [WZPath("info/monsterBook")]
        public bool IsMonsterBook { get; set; }
        [WZPath("info/mob")]
        public int MobId { get; set; }
    }
}
