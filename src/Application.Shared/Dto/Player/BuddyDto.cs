using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Shared.Characters
{
    public class BuddyDto
    {
        public int CharacterId { get; set; }
        public string CharacterName { get; set; }
        public sbyte Pending { get; set; }
        public string Group { get; set; }
    }
}
