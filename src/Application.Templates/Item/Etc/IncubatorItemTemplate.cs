using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Templates.Item.Etc
{
    public class IncubatorItemTemplate: EtcItemTemplate
    {
        public IncubatorItemTemplate(int templateId) : base(templateId)
        {
            ConsumeItems = Array.Empty<IncubatorConsumeItem>();
        }

        [WZPath("info/grade")]
        public int Grade { get; set; }
        [WZPath("info/hybrid")]
        public bool Hybrid { get; set; }

        [WZPath("info/exp")]
        public int Exp { get; set; }

        [WZPath("info/questId")]
        public int QuestID { get; set; }

        [WZPath("info/consumeItem/x")]
        public IncubatorConsumeItem[] ConsumeItems { get; set; }
        public class IncubatorConsumeItem

        {
            public int ItemId { get; set; }
            public int Value { get; set; }
        }

    }
}
