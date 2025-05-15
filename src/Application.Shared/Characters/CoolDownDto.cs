using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Shared.Characters
{
    public class CoolDownDto
    {
        public int SkillId { get; set; }
        public long StartTime { get; set; }
        public long Length { get; set; }
    }
}
