namespace Application.Templates
{
    /// <summary>
    /// Generic keyed collection for AbstractTemplate objects.
    /// Intended use cases are for AbstractProvider collections that
    ///		require nested keyed collections for speedy access.
    /// </summary>
    /// <typeparam name="TKey">Collection key</typeparam>
    /// <typeparam name="TValue">Collection value of type AbstractTemplate</typeparam>
    public abstract class GenericKeyedTemplate<TValue> : AbstractTemplate where TValue : AbstractTemplate
    {
        protected Dictionary<int, TValue> _categoryData;
        bool _hasLoadedAll = false;


        public GenericKeyedTemplate(int templateId) : base(templateId)
        {
            _categoryData = new Dictionary<int, TValue>();
        }

        public int Count => _categoryData.Count;

        public TValue? this[int key] => _categoryData.GetValueOrDefault(key);

        public void Add(TValue value)
        {
            ArgumentNullException.ThrowIfNull(value);
            _categoryData.TryAdd(value.TemplateId, value);
        }

        public Dictionary<int, TValue>.KeyCollection Keys => _categoryData.Keys;
        public Dictionary<int, TValue>.ValueCollection Values => _categoryData.Values;



        public IEnumerable<TValue> LoadAll()
        {
            if (_hasLoadedAll)
                return Values;

            try
            {
                LoadAllInternal();
                return Values;
            }
            finally
            {
                _hasLoadedAll = true;
            }
        }

        protected abstract void LoadAllInternal();
    }
}