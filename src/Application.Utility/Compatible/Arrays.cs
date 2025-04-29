namespace Application.Utility.Compatible
{
    public static class Arrays
    {
        public static TValue[] copyOf<TValue>(TValue[] source, int length)
        {
            var newArr = new TValue[length];

            var effectLenght = length < source.Length ? length : source.Length;
            Array.ConstrainedCopy(source, 0, newArr, 0, effectLenght);
            return newArr;
        }

        public static List<TValue> asList<TValue>(params TValue[] values)
        {
            return values.ToList();
        }

        public static TValue[] copyOfRange<TValue>(TValue[] source, int startIndex, int length)
        {
            var newArr = new TValue[source.Length];
            Array.ConstrainedCopy(source, startIndex, newArr, 0, length - startIndex);
            return newArr;
        }

    }
}
