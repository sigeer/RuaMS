namespace Application.Core.Compatible.Atomics
{
    public class AtomicLong
    {

        private long _value;

        /// <summary>
        /// Creates a new <c>AtomicBoolean</c> instance with an initial value of <c>false</c>.
        /// </summary>
        public AtomicLong()
            : this(0)
        {

        }

        /// <summary>
        /// Creates a new <c>AtomicBoolean</c> instance with the initial value provided.
        /// </summary>
        public AtomicLong(int value)
        {
            _value = value;
        }

        /// <summary>
        /// This method returns the current value.
        /// </summary>
        /// <returns>
        /// The <c>bool</c> value to be accessed atomically.
        /// </returns>
        public long get()
        {
            return _value;
        }

        /// <summary>
        /// This method sets the current value atomically.
        /// </summary>
        /// <param name="value">
        /// The new value to set.
        /// </param>
        public void set(long value)
        {
            Interlocked.Exchange(ref _value, value);
        }

        /// <summary>
        /// This method atomically sets the value and returns the original value.
        /// </summary>
        /// <param name="value">
        /// The new value.
        /// </param>
        /// <returns>
        /// The value before setting to the new value.
        /// </returns>
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

        /// <summary>
        /// Atomically sets the value to the given updated value if the current value <c>==</c> the expected value.
        /// </summary>
        /// <param name="expected">
        /// The value to compare against.
        /// </param>
        /// <param name="result">
        /// The value to set if the value is equal to the <c>expected</c> value.
        /// </param>
        /// <returns>
        /// <c>true</c> if the comparison and set was successful. A <c>false</c> indicates the comparison failed.
        /// </returns>
        public bool CompareAndSet(bool expected, bool result)
        {
            int e = expected ? 1 : 0;
            int r = result ? 1 : 0;
            return Interlocked.CompareExchange(ref _value, r, e) == e;
        }

        /// <summary>
        /// This operator allows an implicit cast from <c>AtomicBoolean</c> to <c>int</c>.
        /// </summary>
        public static implicit operator long(AtomicLong value)
        {
            return value.get();
        }

    }
}
