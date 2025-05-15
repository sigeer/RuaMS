using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Shared.Characters
{
    public class PetIgnoreDto
    {
        public int PetId { get; set; }
        public int[] ExcludedItems { get; set; }
    }
}
