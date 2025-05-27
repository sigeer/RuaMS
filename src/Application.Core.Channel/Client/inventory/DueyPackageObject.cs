using client.inventory;

namespace Application.Core.Channel.Client.inventory
{
    public class DueyPackageObject
    {
        public int PackageId { get; set; }

        public int ReceiverId { get; set; }

        public string SenderName { get; set; } = null!;

        public int Mesos { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public string? Message { get; set; }

        public bool Checked { get; set; } = true;

        public bool Type { get; set; } = false;
        public Item? Item { get; set; }

        public virtual ICollection<Dueyitem> Dueyitems { get; set; } = new List<Dueyitem>();

        public long sentTimeInMilliseconds()
        {
            return TimeStamp.AddMonths(1).ToUnixTimeMilliseconds();
        }

        public bool isDeliveringTime()
        {
            return TimeStamp >= DateTimeOffset.UtcNow;
        }

        public void UpdateSentTime()
        {
            DateTimeOffset cal = TimeStamp;

            if (Type)
            {
                if (DateTimeOffset.UtcNow - TimeStamp < TimeSpan.FromDays(1))
                {
                    // thanks inhyuk for noticing quick delivery packages unavailable to retrieve from the get-go
                    cal.AddDays(-1);
                }
            }

            this.TimeStamp = cal;
        }
    }
}
