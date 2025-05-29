using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Shared.Items
{
    public class GiftDto
    {
        public int Id { get; set; }

        public int To { get; set; }

        public string From { get; set; } = null!;

        public string Message { get; set; } = null!;

        public int Sn { get; set; }

        public long RingId { get; set; } = -1;
    }
}
