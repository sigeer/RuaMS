using client.inventory;

namespace Application.Module.Duey.Channel.Models
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

        public long sentTimeInMilliseconds()
        {
            return TimeStamp.AddMonths(1).ToUnixTimeMilliseconds();
        }
    }
}
