namespace Application.Utility.Tickables
{
    public interface ITickable
    {
        void OnTick(long now);

        TickableStatus Status { get; }
    }
    public interface IDelayedTickable: ITickable
    {
        long Next { get; }

    }

    public interface ILoopTickable: IDelayedTickable
    {
        long Period { get; }
    }

    public interface ITickableTree : ITickable
    {
        public List<ITickable> SubTickables { get; }
    }

    public interface ILifedTickable: ITickable
    {
        long ExpiredAt { get; }
    }

    public enum TickableStatus
    {
        /// <summary>
        /// 有效
        /// </summary>
        Active,
        /// <summary>
        /// 仅停止
        /// </summary>
        InActive,
        /// <summary>
        /// 待移除
        /// </summary>
        Remove
    }
    public abstract class DelayedTickable: IDelayedTickable
    {
        public DelayedTickable(long next)
        {
            Next = next;
        }

        public long Next { get; }

        public TickableStatus Status { get; private set; }

        public void OnTick(long now)
        {
            if (Status != TickableStatus.Active)
                return;

            if (Next <= now)
            {
                Handle(now);

                Status = TickableStatus.Remove;
            }
        }

        protected abstract void Handle(long now);
    }
}
