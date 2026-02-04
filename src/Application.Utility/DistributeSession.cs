namespace Application.Utility
{
    public class DistributeSession<TGroupKey, TData>
    {
        private readonly HashSet<TGroupKey> _pending;
        private readonly List<TData> _chunks = new();

        public DistributeSession(IEnumerable<TGroupKey> pending)
        {
            _pending = pending.ToHashSet();
        }

        public bool CompleteChunk(DistributeSessionDataWrapper<TGroupKey, TData> chunk)
        {
            if (!_pending.Remove(chunk.GroupKey))
                return false; // 重复或非法

            _chunks.AddRange(chunk.Data);
            return _pending.Count == 0;
        }

        public List<TData> Chunks => _chunks;
    }

    public class DistributeSessionDataWrapper<TGroupKey, TData>
    {
        public DistributeSessionDataWrapper(TGroupKey groupKey, List<TData> data)
        {
            GroupKey = groupKey;
            Data = data;
        }

        public TGroupKey GroupKey { get; }
        public List<TData> Data { get; }
    }
}
