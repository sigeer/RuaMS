using System.Drawing;

namespace Application.Templates.Item.Cash
{
    public class CashItemTemplate : AbstractItemTemplate
    {

        [WZPath("info/protectTime")]
        public int ProtectTime { get; set; }

        [WZPath("info/recoveryRate")]
        public int RecoveryRate { get; set; }

        [WZPath("info/life")]
        public int Life { get; set; }

        [WZPath("info/stateChangeItem")]
        public int StateChangeItem { get; set; }

        [WZPath("info/type")]
        public int WeatherType { get; set; }

        [WZPath("info/meso")]
        public int Meso { get; set; }
        [WZPath("info/rb")]
        public Point RB { get; set; }

        [WZPath("info/lt")]
        public Point LT { get; set; }

        public CashItemTemplate(int templateId)
            : base(templateId) { }
    }
}