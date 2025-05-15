using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Shared.Characters
{
    public class RecentFameRecordDto
    {
        public int[] ChararacterIds { get; set; }
        public long LastUpdateTime { get; set; }
    }
}
