namespace Application.Core.Compatible.Atomics
{
    public class AtomicInteger
    {

        private int _value;

        /// <summary>
        /// Creates a new <c>AtomicBoolean</c> instance with an initial value of <c>false</c>.
        /// </summary>
        public AtomicInteger()
            : this(0)
        {

        }

        /// <summary>
        /// Creates a new <c>AtomicBoolean</c> instance with the initial value provided.
        /// </summary>
        public AtomicInteger(int value)
        {
            _value = value;
        }

        /// <summary>
        /// This method returns the current value.
        /// </summary>
        /// <returns>
        /// The <c>bool</c> value to be accessed atomically.
        /// </returns>
        public int get()
        {
            return _value;
        }

        /// <summary>
        /// This method sets the current value atomically.
        /// </summary>
        /// <param name="value">
        /// The new value to set.
        /// </param>
        public void set(int value)
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
        public static implicit operator int(AtomicInteger value)
        {
            return value.get();
        }

    }
}
