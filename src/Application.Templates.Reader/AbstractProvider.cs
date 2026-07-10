using System.Collections.Concurrent;

namespace Application.Templates.Reader
{
    public abstract class AbstractProvider<TTemplate> : IProvider<TTemplate>, IDisposable where TTemplate : AbstractTemplate
    {
        protected readonly ConcurrentDictionary<int, TTemplate> _templates;
        public abstract ProviderType Type { get; }
        protected readonly bool _useCache;
        protected IWzPathResolver _resolver;

        private int _refCount = 0;
        private bool _disposed = false;
        bool _hasAllLoaded = false;
        Lock _loadAllLock = new Lock();
        internal protected AbstractProvider(IWzPathResolver resolver, bool useCache = true)
        {
            _templates = new();
            _useCache = useCache;
            _resolver = resolver;
        }

        public string GetBaseDir() => _resolver.BaseDir;

        public virtual IEnumerable<TTemplate> LoadAll()
        {
            if (_hasAllLoaded)
                return Values;

            _loadAllLock.Enter();
            try
            {
                if (_hasAllLoaded)
                    return Values;

                return LoadAllInternal();
            }
            finally
            {
                if (_useCache)
                    _hasAllLoaded = true;

                _loadAllLock.Exit();
            }
        }

        protected abstract IEnumerable<TTemplate> LoadAllInternal();
        IEnumerable<AbstractTemplate> IProvider.LoadAll() => LoadAll();

        public virtual TCTemplate? GetRequiredItem<TCTemplate>(int templateId) where TCTemplate : AbstractTemplate => (GetItem(templateId) as TCTemplate);

        AbstractTemplate? IProvider.GetItem(int templateId) => GetItem(templateId);

        public virtual TTemplate? GetItem(int templateId)
        {
            if (_useCache)
            {
                if (_templates.TryGetValue(templateId, out var data))
                    return data;

                if (_hasAllLoaded)
                    return null;
            }
            return GetItemInternal(templateId);
        }

        protected abstract TTemplate? GetItemInternal(int templateId);

        public bool Contains(int key) => _templates.ContainsKey(key);
        public bool Contains(TTemplate item) => _templates.ContainsKey(GetKeyForItem(item));
        protected void InsertItem(TTemplate item)
        {
            if (_useCache)
                _templates[GetKeyForItem(item)] = item;
        }

        protected void InsertItem(int key, TTemplate item)
        {
            if (_useCache)
                _templates[key] = item;
        }

        protected IEnumerable<TTemplate> Values => _templates.Values;

        protected virtual int GetKeyForItem(TTemplate item) => item.TemplateId;

        public virtual TTemplate? this[int key]
        {
            get
            {
                if (key < 0) throw new ArgumentOutOfRangeException(nameof(key));
                return GetItem(key);
            }
        }

        public void AddRef()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);
            Interlocked.Increment(ref _refCount);
        }

        public void Release()
        {
            if (Interlocked.Decrement(ref _refCount) == 0)
                Dispose();
        }

        public virtual void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _hasAllLoaded = false;
                _templates.Clear();
            }
        }

    }
}
