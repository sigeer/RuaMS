using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Shared.Characters
{
    public class PetDto
    {
        public int Petid { get; set; }

        public string? Name { get; set; }

        public int Level { get; set; }

        public int Closeness { get; set; }

        public int Fullness { get; set; }

        public bool Summoned { get; set; }

        public int Flag { get; set; }
    }
}
