using Application.Core.Game.Relation;

namespace Application.Core.Model
{
    public class GiftModel
    {
        public int Id { get; set; }

        public int To { get; set; }

        public string From { get; set; } = null!;

        public string Message { get; set; } = null!;

        public int Sn { get; set; }
        public Ring? Ring { get; set; }
    }
}
