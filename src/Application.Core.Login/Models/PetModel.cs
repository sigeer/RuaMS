using Application.Core.Login.Shared;

namespace Application.Core.Login.Models
{

    public class PetModel
    {
        public long Petid { get; set; }

        public string? Name { get; set; }

        public int Level { get; set; }

        public int Closeness { get; set; }

        public int Fullness { get; set; }

        public bool Summoned { get; set; }

        public int Flag { get; set; }
    }
}
