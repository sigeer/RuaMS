namespace Application.Utility.Tickables
{
    public interface ITickable
    {
        void OnTick(long now);
        bool IsTickableCancelled { get; set; }
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

    public abstract class DelayedTickable: IDelayedTickable
    {
        public DelayedTickable(long next)
        {
            Next = next;
        }

        public long Next { get; }


        public bool IsTickableCancelled { get; set; }

        public void OnTick(long now)
        {
            if (IsTickableCancelled)
                return;

            if (Next <= now)
            {
                Handle(now);

                IsTickableCancelled = true;
            }
        }

        protected abstract void Handle(long now);
    }
}
