using Application.Shared.Items;

namespace Application.Shared.Duey
{
    public class DueyPackageDto
    {
        public int PackageId { get; set; }

        public int ReceiverId { get; set; }

        public string SenderName { get; set; } = null!;

        public int Mesos { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public string? Message { get; set; }

        public bool Checked { get; set; } = true;

        public bool Type { get; set; } = false;
        public ItemDto? Item { get; set; }
    }
}
