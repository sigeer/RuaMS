using System.Runtime.CompilerServices;

namespace Application.Utility.Compatible.Atomics
{
    public class AtomicEnum<TEnum> where TEnum : Enum
    {
        private int _state;
        private readonly bool IsUnderlyingInt = Enum.GetUnderlyingType(typeof(TEnum)) == typeof(int);
        public AtomicEnum(TEnum initialState)
        {
            _state = Convert.ToInt32(initialState);
        }

        public TEnum Value
        {
            get
            {
                if (IsUnderlyingInt)
                {
                    // 无装箱转换（只在基础类型为 int 时）
                    return Unsafe.As<int, TEnum>(ref _state);
                }
                else
                {
                    // 安全 fallback
                    return (TEnum)Enum.ToObject(typeof(TEnum), _state);
                }
            }
        }

        public void Set(TEnum newState)
        {
            Interlocked.Exchange(ref _state, Convert.ToInt32(newState));
        }

        public bool CompareAndSet(TEnum expected, TEnum newState)
        {
            int expectedInt = Convert.ToInt32(expected);
            int newInt = Convert.ToInt32(newState);
            int original = Interlocked.CompareExchange(ref _state, newInt, expectedInt);
            return original == expectedInt;
        }

        public bool Is(TEnum state)
        {
            return Value.Equals(state);
        }

        public override string ToString() => Value.ToString();
    }
}
