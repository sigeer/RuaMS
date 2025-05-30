using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.Login.Models
{
    public class BuddyModel
    {
        public int CharacterId { get; set; }
        public string CharacterName { get; set; }
        public sbyte Pending { get; set; }
        public string Group { get; set; }
    }
}
