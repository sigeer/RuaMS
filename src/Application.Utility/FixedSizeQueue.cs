namespace Application.Utility
{
    public class FixedSizeQueue<T>
    {
        private readonly Queue<T> _queue = new();
        private readonly int _maxSize;

        public FixedSizeQueue(int maxSize)
        {
            _maxSize = maxSize;
        }

        public void Enqueue(T item)
        {
            while (_queue.Count >= _maxSize)
            {
                _queue.Dequeue(); // 移除最早添加的
            }
            _queue.Enqueue(item);
        }

        public List<T> ToList()
        {
            return _queue.ToList();
        }


        public T Dequeue() => _queue.Dequeue();
        public int Count => _queue.Count;
    }

}
