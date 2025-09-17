namespace Application.Templates.Item.Consume
{
    [GenerateTag]
    public class ConsumeItemTemplate : ItemTemplateBase
    {

        [WZPath("info/monsterBook")]
        public bool MonsterBook { get; set; }

        [WZPath("spec/consumeOnPickup")]
        public bool ConsumeOnPickup { get; set; }
        [WZPath("specEx/consumeOnPickup")]
        public bool ConsumeOnPickupEx { get; set; }
        [WZPath("info/noCancelMouse")]
        public bool NoCancelMouse { get; set; }

        [WZPath("spec/expinc")]
        public int ExpInc { get; set; }

        [WZPath("info/type")]
        public int InfoType { get; set; }

        public ConsumeItemTemplate(int templateId) : base(templateId)
        {

        }
    }
}