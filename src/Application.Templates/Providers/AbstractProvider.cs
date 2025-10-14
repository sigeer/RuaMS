using System.Collections.Concurrent;
using System.Linq;

namespace Application.Templates.Providers
{
    public abstract class AbstractProvider<TTemplate> : IProvider, IDisposable where TTemplate : AbstractTemplate
    {
        protected readonly ConcurrentDictionary<int, TTemplate> _templates;
        public abstract ProviderType ProviderName { get; }

        protected readonly TemplateOptions _options;
        protected string _dataBaseDir;

        private int _refCount = 0;
        private bool _disposed = false;
        bool _hasAllLoaded = false;
        Lock _loadAllLock = new Lock();

        protected AbstractProvider(TemplateOptions options)
        {
            _templates = new();

            _options = options;
            _dataBaseDir = ProviderFactory.GetEffectDir(options.DataDir);
        }


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
                _hasAllLoaded = true;
                _loadAllLock.Exit();
            }
        }
        /// <summary>
        /// 一次性加载所有资源
        /// </summary>
        protected abstract IEnumerable<AbstractTemplate> LoadAllInternal();

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

        protected virtual TTemplate? GetItemInternal(int templateId)
        {
            return GetDataFromImg(GetImgPathByTemplateId(templateId)).FirstOrDefault(x => x.TemplateId == templateId) as TTemplate;
        }

        public virtual TCTemplate? GetRequiredItem<TCTemplate>(int templateId) where TCTemplate : TTemplate => GetItem(templateId) as TCTemplate;
        /// <summary>
        /// 通过templateId获取文件路径（对于xml是xml路径，对于wz/nx，则是文件内路径）
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual string GetImgPathByTemplateId(int key)
        {
            return string.Empty;
        }

        /// <summary>
        /// 通过img获取template，并直接写入缓存
        /// <para>可能存在一个img解析出多个template</para>
        /// </summary>
        /// <param name="path">img路径（对于xml是xml路径，对于wz/nx，则是文件内路径）</param>
        /// <returns></returns>
        protected abstract IEnumerable<AbstractTemplate> GetDataFromImg(string path);

        protected string GetPath()
        {
            return Path.Combine(_dataBaseDir, TemplateOptions.GetDefaultPath(ProviderName));
        }


        public bool Contains(int key) => _templates.ContainsKey(key);
        public bool Contains(TTemplate item) => _templates.ContainsKey(GetKeyForItem(item));
        protected void InsertItem(TTemplate item)
        {
            if (_options.UseCache)
                _templates[GetKeyForItem(item)] = item;
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
