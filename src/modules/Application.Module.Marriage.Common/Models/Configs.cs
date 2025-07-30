using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Module.Marriage.Common.Models
{
    public class Configs
    {
        //Wedding Configuration
        public int WEDDING_RESERVATION_DELAY { get; set; } = 3;
        public int WEDDING_RESERVATION_TIMEOUT { get; set; } = 10;
        public int WEDDING_RESERVATION_INTERVAL { get; set; } = 60;
        public int WEDDING_BLESS_EXP { get; set; } = 30000;
        public int WEDDING_GIFT_LIMIT { get; set; } = 1;
        public bool WEDDING_BLESSER_SHOWFX { get; set; } = true;
    }
}
