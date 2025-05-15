using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Shared.Characters
{
    public class SkillDto
    {
        public int SkillId { get; set; }
        public int Level { get; set; }
        public int MasterLevel { get; set; }
        public long Expiration { get; set; }
    }
}
