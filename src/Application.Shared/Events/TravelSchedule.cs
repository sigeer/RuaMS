using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Shared.Events
{
    public class TravelScheduleItem
    {
        public int World { get; set; }
        public int Channel { get; set; }
        public TraveType Type { get; set; }
        public string TypeName { get; set; }
        public DateTimeOffset? NextTime { get; set; }
    }
}
