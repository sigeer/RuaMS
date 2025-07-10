using Application.Core.Login.Models;
using Application.Core.Login.Shared;

namespace Application.Module.Duey.Master.Models
{
    public class DueyPackageModel: ITrackableEntityKey<int>
    {
        public int Id { get; set; }

        public int ReceiverId { get; set; }

        public int SenderId { get; set; }
        public string SenderName { get; set; } = null!;

        public int Mesos { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public string? Message { get; set; }

        public bool Checked { get; set; } = true;

        public bool Type { get; set; } = false;
        public ItemModel? Item { get; set; }

        public void UpdateSentTime()
        {
            if (Type)
            {
                TimeStamp = TimeStamp.AddDays(-1);
            }
        }


        /// <summary>
        /// 取货读取时冻结
        /// </summary>
        public int _frozen;

        /// <summary>
        /// 尝试冻结（原子操作），避免重复领取
        /// </summary>
        public bool TryFreeze()
        {
            return Interlocked.CompareExchange(ref _frozen, 1, 0) == 0;
        }

        public bool IsFrozen => _frozen == 1;
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
