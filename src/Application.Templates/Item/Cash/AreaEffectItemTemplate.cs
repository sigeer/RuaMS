using System.Drawing;

namespace Application.Templates.Item.Cash
{
    /// <summary>
    /// 528
    /// </summary>
    [GenerateTag]
    public class AreaEffectItemTemplate : CashItemTemplate
    {
        public AreaEffectItemTemplate(int templateId) : base(templateId)
        {
        }

        [WZPath("info/time")]
        public override int Time { get; set; }

        [WZPath("info/rb")]
        public Point RB { get; set; }

        [WZPath("info/lt")]
        public Point LT { get; set; }
    }
}
