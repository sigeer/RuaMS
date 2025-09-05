namespace Application.Templates.Providers
{
    public abstract class AbstractProvider<TTemplate> : IProvider where TTemplate : AbstractTemplate
    {
        protected readonly Dictionary<int, TTemplate> _templates;
        public abstract ProviderType ProviderName { get; }
        /// <summary>
        /// 已经加载了全部数据，LoadAll后调用
        /// </summary>
        protected bool _hasLoadAll { get; set; }
        /// <summary>
        /// 当加载所有数据时，必定启用缓存
        /// </summary>
        protected bool _useCache { get; set; }
        protected readonly TemplateOptions _options;

        protected AbstractProvider(TemplateOptions options)
        {
            _templates = new Dictionary<int, TTemplate>();

            _options = options;
            _useCache = _options.UseCache;
        }

        public virtual void LoadAll()
        {
            _useCache = true;
            LoadAllInternal();
            _hasLoadAll = true;
        }
        /// <summary>
        /// 一次性加载所有资源
        /// </summary>
        protected abstract void LoadAllInternal();

        public virtual TTemplate? GetItem(int templateId)
        {
            if (_templates.TryGetValue(templateId, out var data))
                return data;

            if (_hasLoadAll)
                return null;
            else
                GetDataFromImg(GetImgPathByTemplateId(templateId));

            return _templates.GetValueOrDefault(templateId);
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
        /// 通过img获取template，并直接写入缓存，CanLoadSingle为false时不会调用
        /// <para>可能存在一个img解析出多个template</para>
        /// </summary>
        /// <param name="path">img路径（对于xml是xml路径，对于wz/nx，则是文件内路径）</param>
        /// <returns></returns>
        protected virtual void GetDataFromImg(string path)
        {

        }

        protected virtual string GetPath()
        {
            return _options.GetRootDir(ProviderName);
        }


        public bool Contains(int key) => _templates.ContainsKey(key);
        public bool Contains(TTemplate item) => _templates.ContainsKey(GetKeyForItem(item));
        protected void InsertItem(TTemplate item)
        {
            if (_useCache)
                _templates[GetKeyForItem(item)] = item;
        }

        protected virtual int GetKeyForItem(TTemplate item) => item.TemplateId;

        public Dictionary<int, TTemplate>.Enumerator GetEnumerator() => _templates.GetEnumerator();
        public Dictionary<int, TTemplate>.ValueCollection Values => _templates.Values;
        public Dictionary<int, TTemplate>.KeyCollection Keys => _templates.Keys;
        public virtual TTemplate this[int key]
        {
            get
            {
                if (key < 0) throw new ArgumentOutOfRangeException(nameof(key));
                return GetItem(key);
            }
        }
    }
}
