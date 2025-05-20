using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Shared.Characters
{
    public class EventDto
    {
        public int Characterid { get; set; }

        public string Name { get; set; } = null!;

        public int Info { get; set; }
    }
}
