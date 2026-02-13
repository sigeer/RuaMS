using System.Collections;

namespace Application.Utility.Collections
{
    /// <summary>
    /// 超出容量时，移除最早的数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BoundedCollection<T> : IEnumerable<T>
    {
        private readonly LinkedList<T> _list;
        private readonly int _capacity;

        /// <summary>
        /// 获取集合中实际包含的元素数。
        /// </summary>
        public int Count => _list.Count;

        /// <summary>
        /// 获取集合的容量上限。
        /// </summary>
        public int Capacity => _capacity;

        public event EventHandler<T>? OnOverWrite;

        /// <summary>
        /// 初始化一个有界集合。
        /// </summary>
        /// <param name="capacity">容量，必须大于 0。</param>
        /// <exception cref="ArgumentOutOfRangeException">容量小于等于 0。</exception>
        public BoundedCollection(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "容量必须大于 0。");

            _capacity = capacity;
            _list = new LinkedList<T>();
        }

        /// <summary>
        /// 添加一个元素到集合末尾。
        /// 如果添加后超出容量，则自动移除最早添加的元素。
        /// </summary>
        /// <param name="item">要添加的元素。</param>
        public void Add(T item)
        {
            _list.AddLast(item);
            if (_list.Count > _capacity)
            {
                var first = RemoveFirst();
                if (first != null)
                {
                    OnOverWrite?.Invoke(this, first);
                }
            }
        }

        /// <summary>
        /// 移除集合中第一个与指定值匹配的元素。
        /// </summary>
        /// <param name="item">要移除的元素。</param>
        /// <returns>如果成功移除返回 true，否则 false。</returns>
        public bool Remove(T item)
        {
            return _list.Remove(item);
        }

        /// <summary>
        /// 移除所有与指定谓词匹配的元素。
        /// </summary>
        /// <param name="match">条件谓词。</param>
        /// <returns>移除的元素个数。</returns>
        public int RemoveAll(Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));

            var nodesToRemove = new List<LinkedListNode<T>>();
            var node = _list.First;
            while (node != null)
            {
                if (match(node.Value))
                    nodesToRemove.Add(node);
                node = node.Next;
            }

            foreach (var n in nodesToRemove)
                _list.Remove(n);

            return nodesToRemove.Count;
        }

        /// <summary>
        /// 移除并返回最早添加的元素。
        /// </summary>
        /// <exception cref="InvalidOperationException">集合为空。</exception>
        public T? RemoveFirst()
        {
            if (_list.Count == 0)
                return default;

            var value = _list.First!.Value;
            _list.RemoveFirst();
            return value;
        }

        /// <summary>
        /// 移除最晚添加的元素。
        /// </summary>
        /// <returns>如果成功移除返回 true，否则 false（集合为空）。</returns>
        public bool RemoveLast()
        {
            if (_list.Count == 0)
                return false;
            _list.RemoveLast();
            return true;
        }

        /// <summary>
        /// 清空集合。
        /// </summary>
        public void Clear()
        {
            _list.Clear();
        }

        /// <summary>
        /// 判断集合是否包含指定元素。
        /// </summary>
        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        /// <summary>
        /// 返回枚举器，按元素添加顺序（从早到晚）。
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
