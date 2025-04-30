namespace Application.Utility.Compatible.Atomics
{
    public class AtomicLong
    {

        private long _value;

        public AtomicLong()
            : this(0)
        {

        }

        public AtomicLong(int value)
        {
            _value = value;
        }

        public long get()
        {
            return _value;
        }

        public void set(long value)
        {
            Interlocked.Exchange(ref _value, value);
        }

        public long getAndSet(long value)
        {
            return Interlocked.Exchange(ref _value, value);
        }

        public long incrementAndGet()
        {
            return Interlocked.Increment(ref _value);
        }

        public long decrementAndGet()
        {
            return Interlocked.Decrement(ref _value);
        }

        public long addAndGet(long value)
        {
            return Interlocked.Add(ref _value, value);
        }

        public long getAndIncrement()
        {
            var m = _value;
            Interlocked.Increment(ref _value);
            return m;
        }

        public bool CompareAndSet(bool expected, bool result)
        {
            int e = expected ? 1 : 0;
            int r = result ? 1 : 0;
            return Interlocked.CompareExchange(ref _value, r, e) == e;
        }

        public static implicit operator long(AtomicLong value)
        {
            return value.get();
        }

    }
}
