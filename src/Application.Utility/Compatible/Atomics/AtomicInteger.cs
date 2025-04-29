namespace Application.Utility.Compatible.Atomics
{
    public class AtomicInteger
    {

        private int _value;

        public AtomicInteger()
            : this(0)
        {

        }

        public AtomicInteger(int value)
        {
            _value = value;
        }

        public int get()
        {
            return _value;
        }

        public void set(int value)
        {
            Interlocked.Exchange(ref _value, value);
        }

        public int getAndSet(int value)
        {
            return Interlocked.Exchange(ref _value, value);
        }

        public int incrementAndGet()
        {
            return Interlocked.Increment(ref _value);
        }

        public int getAndIncrement()
        {
            var m = _value;
            Interlocked.Increment(ref _value);
            return m;
        }

        public int decrementAndGet()
        {
            return Interlocked.Decrement(ref _value);
        }

        public int addAndGet(int value)
        {
            return Interlocked.Add(ref _value, value);
        }

        public bool CompareAndSet(bool expected, bool result)
        {
            int e = expected ? 1 : 0;
            int r = result ? 1 : 0;
            return Interlocked.CompareExchange(ref _value, r, e) == e;
        }


        public static implicit operator int(AtomicInteger value)
        {
            return value.get();
        }

    }
}
