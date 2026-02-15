using System.Collections.Concurrent;

namespace Application.Templates.Providers
{
    public abstract class AbstractProvider<TTemplate> : IProvider, IDisposable where TTemplate : AbstractTemplate
    {
        protected readonly ConcurrentDictionary<int, TTemplate> _templates;
        /// <summary>
        /// 隶属wz
        /// </summary>
        public abstract string ProviderName { get; }


        protected readonly ProviderOption _options;
        /// <summary>
        /// wz根目录
        /// </summary>
        protected string? _dataBaseDir;
        protected WzFileProvider _fileProvider;

        private int _refCount = 0;
        private bool _disposed = false;
        bool _hasAllLoaded = false;
        Lock _loadAllLock = new Lock();

        internal protected AbstractProvider(ProviderOption options)
        {
            _templates = new();

            _options = options;
            _dataBaseDir = options.DataDir;

            _fileProvider = new WzFileProvider(GetBaseDir());
        }

        public string GetBaseDir() => _dataBaseDir ?? throw new ArgumentNullException(nameof(_dataBaseDir));
        /// <summary>
        /// 读取所有img，有缓存
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<AbstractTemplate> LoadAll()
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
                if (_options.UseCache)
                {
                    _hasAllLoaded = true;
                }

                _loadAllLock.Exit();
            }
        }
        /// <summary>
        /// 读取所有img
        /// </summary>
        protected abstract IEnumerable<AbstractTemplate> LoadAllInternal();

        public virtual TCTemplate? GetRequiredItem<TCTemplate>(int templateId) where TCTemplate : TTemplate => GetItem(templateId) as TCTemplate;

        public virtual TTemplate? GetItem(int templateId)
        {
            if (_options.UseCache)
            {
                if (_templates.TryGetValue(templateId, out var data))
                    return data;

                if (_hasAllLoaded)
                    return null;
            }
            return GetItemInternal(templateId);
        }

        /// <summary>
        /// 获取一条数据
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        protected abstract TTemplate? GetItemInternal(int templateId);

        public bool Contains(int key) => _templates.ContainsKey(key);
        public bool Contains(TTemplate item) => _templates.ContainsKey(GetKeyForItem(item));
        protected void InsertItem(TTemplate item)
        {
            if (_options.UseCache)
                _templates[GetKeyForItem(item)] = item;
        }

        protected void InsertItem(int key, TTemplate item)
        {
            if (_options.UseCache)
                _templates[key] = item;
        }

        protected IEnumerable<AbstractTemplate> Values => _templates.Values;

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
            {
                Dispose();
            }
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
