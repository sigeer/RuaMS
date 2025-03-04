namespace Application.Utility
{
    public class BidirectionalDictionary<T> where T : notnull
    {
        private readonly Dictionary<T, T> forward = new();
        private readonly Dictionary<T, T> reverse = new();

        public void Add(T key, T value)
        {
            forward[key] = value;
            reverse[value] = key;
        }

        public bool TryGetValue(T key, out T value)
        {
            return forward.TryGetValue(key, out value) || reverse.TryGetValue(key, out value);
        }

        public void Remove(T key)
        {
            forward.Remove(key);
            reverse.Remove(key);
        }
    }
}
