using Application.Core.Login.Models;

namespace Application.Module.Duey.Master.Models
{
    public class DueyPackageModel
    {
        public int Id { get; set; }
        public int PackageId { get; set; }

        public int ReceiverId { get; set; }

        public int SenderId { get; set; }

        public int Mesos { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public string? Message { get; set; }

        public bool Checked { get; set; } = true;

        public bool Type { get; set; } = false;
        public ItemModel? Item { get; set; }
        /// <summary>
        /// 取货读取时冻结
        /// </summary>
        public bool Fronzen { get; set; }
    }
}
