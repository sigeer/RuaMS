using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Module.BBS.Master.Models
{
    public class BBSReplyModel
    {
        public int Id { get; set; }
        public int Threadid { get; set; }

        public int Postercid { get; set; }

        public long Timestamp { get; set; }

        public string Content { get; set; } = null!;
    }
}
