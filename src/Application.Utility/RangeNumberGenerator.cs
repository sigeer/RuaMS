namespace Application.Utility
{
    public class RangeNumberGenerator
    {
        int _step;
        long _value;
        long _flag;
        public RangeNumberGenerator(long value, int step)
        {
            _value = value;
            _step = step;

            _flag = _value / _step;
        }
        public long Min => _flag * _step;
        public long Max => (_flag + 1) * _step - 1;
        public override string ToString()
        {
            return $"{Min}-{Max}";
        }
    }
}
