using Application.Core.Login.Models;

namespace Application.Module.Duey.Master.Models
{
    public class DueyPackageModel
    {
        public int Id { get; set; }
        /// <summary>
        /// 仅在读写数据库时使用，其他时候用Id代替（频道获取的packageid都是id）
        /// </summary>
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
        public int _frozen;

        /// <summary>
        /// 尝试冻结（原子操作）
        /// </summary>
        public bool TryFreeze()
        {
            return Interlocked.CompareExchange(ref _frozen, 1, 0) == 0;
        }

        /// <summary>
        /// 尝试解冻（原子操作）
        /// </summary>
        public bool TryUnfreeze()
        {
            return Interlocked.CompareExchange(ref _frozen, 0, 1) == 1;
        }

        /// <summary>
        /// 强制解冻（不关心原状态）
        /// </summary>
        public void ForceUnfreeze()
        {
            Interlocked.Exchange(ref _frozen, 0);
        }
    }
}
