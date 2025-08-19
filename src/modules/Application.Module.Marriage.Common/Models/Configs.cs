using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Module.Marriage.Common.Models
{
    public class Configs
    {
        /// <summary>
        /// Minimum idle slots before processing a wedding reservation.
        /// </summary>
        public int WEDDING_RESERVATION_DELAY { get; set; } = 3;
        /// <summary>
        /// Limit time in minutes for the couple to show up before cancelling the wedding reservation.
        /// </summary>
        public int WEDDING_RESERVATION_TIMEOUT { get; set; } = 10;
        /// <summary>
        /// Time between wedding starts in minutes.
        /// </summary>
        public int WEDDING_RESERVATION_INTERVAL { get; set; } = 60;
        /// <summary>
        /// Exp gained per bless count.
        /// </summary>
        public int WEDDING_BLESS_EXP { get; set; } = 30000;
        /// <summary>
        /// Max number of gifts per person to same wishlist on marriage instances.
        /// </summary>
        public int WEDDING_GIFT_LIMIT { get; set; } = 1;
        /// <summary>
        /// Pops bubble sprite effect on players blessing the couple. Setting this false shows the blessing effect on the couple instead.
        /// </summary>
        public bool WEDDING_BLESSER_SHOWFX { get; set; } = true;
    }
}
