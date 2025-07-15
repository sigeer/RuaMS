using Application.Core.Game.Relation;

namespace Application.Core.Model
{
    public class GiftModel
    {
        public int Id { get; set; }

        public int To { get; set; }
        public string ToName { get; set; } = null!;
        public int From { get; set; }
        public string FromName { get; set; } = null!;

        public string Message { get; set; } = null!;

        public int Sn { get; set; }
        public RingSourceModel? Ring { get; set; }
    }
}
