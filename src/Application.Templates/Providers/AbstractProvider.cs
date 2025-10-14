using Application.Templates.Exceptions;
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
        /// <summary>
        /// 单文件img时有效
        /// </summary>
        public virtual string[]? SingleImgFile { get; }

        protected readonly TemplateOptions _options;
        /// <summary>
        /// wz根目录
        /// </summary>
        protected string _dataBaseDir;
        /// <summary>
        /// .img.xml文件对wz根目录的相对路径
        /// </summary>
        protected string[] _files;
        protected WzFileProvider _fileProvider;

        private int _refCount = 0;
        private bool _disposed = false;
        bool _hasAllLoaded = false;
        Lock _loadAllLock = new Lock();

        protected AbstractProvider(TemplateOptions options)
        {
            _templates = new();

            _options = options;
            _dataBaseDir = ProviderFactory.GetEffectDir(options.DataDir);

            _fileProvider = new WzFileProvider(_dataBaseDir);

            if (SingleImgFile == null)
            {
                _files = Directory.GetFiles(Path.Combine(_dataBaseDir, ProviderName), "*.xml", SearchOption.AllDirectories)
                    .Select(x => Path.GetRelativePath(_dataBaseDir, x))
                    .ToArray();
            }
            else
            {
                _files = SingleImgFile.Select(x => Path.Combine(ProviderName, x)).ToArray();
            }
        }

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
                _hasAllLoaded = true;
                _loadAllLock.Exit();
            }
        }
        /// <summary>
        /// 读取所有img
        /// </summary>
        protected virtual IEnumerable<AbstractTemplate> LoadAllInternal()
        {
            List<AbstractTemplate> all = new List<AbstractTemplate>();
            foreach (var file in _files)
            {
                all.AddRange(GetDataFromImg(file));
            }
            return all;
        }

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
        /// 通过templateId获取相应img文件路径（相对路径）
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns>img文件路径（相对路径）</returns>
        protected virtual string? GetImgPathByTemplateId(int templateId)
        {
            if (SingleImgFile == null || SingleImgFile.Length == 0)
            {
                return null;
            }
            return _files[0];

        }

        /// <summary>
        /// 通过img获取template，并直接写入缓存
        /// <para>可能存在一个img解析出多个template</para>
        /// </summary>
        /// <param name="path">img路径（对wz的相对路径）</param>
        /// <returns></returns>
        protected abstract IEnumerable<AbstractTemplate> GetDataFromImg(string? path);

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
