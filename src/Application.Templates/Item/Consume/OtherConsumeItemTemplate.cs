using Application.Templates.StatEffectProps;

namespace Application.Templates.Item.Consume
{
    [GenerateTag]
    public class OtherConsumeItemTemplate : ConsumeItemTemplate, IStatEffectIncMountFatigue, IStatEffectExp
    {
        public OtherConsumeItemTemplate(int templateId) : base(templateId)
        {
        }

        [WZPath("spec/incFatigue")]
        public int IncFatigue { get; set; }
        [WZPath("spec/expBuff")]
        public int ExpBuffRate { get; set; }
    }
}
