namespace Application.Templates
{
    /// <summary>
    /// Generic keyed collection for AbstractTemplate objects.
    /// Intended use cases are for AbstractProvider collections that
    ///		require nested keyed collections for speedy access.
    /// </summary>
    /// <typeparam name="TKey">Collection key</typeparam>
    /// <typeparam name="TValue">Collection value of type AbstractTemplate</typeparam>
    public class GenericKeyedTemplate<TValue> : AbstractTemplate where TValue : AbstractTemplate
    {
        protected Dictionary<int, TValue> _categoryData;

        public GenericKeyedTemplate(int templateId) : base(templateId)
        {
            _categoryData = new Dictionary<int, TValue>();
        }

        public int Count => _categoryData.Count;

        public TValue this[int key] =>
            _categoryData.ContainsKey(key)
                ? _categoryData[key]
                : default;

        public void Add(TValue value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));

            var existing = this[value.TemplateId];

            if (existing == null) // trying to insert VIP shields twice for some reason??
            {
                _categoryData.Add(value.TemplateId, value);
            }
        }

        public Dictionary<int, TValue>.Enumerator GetEnumerator() => _categoryData.GetEnumerator();
        public Dictionary<int, TValue>.ValueCollection Values => _categoryData.Values;

        protected virtual int GetKeyForItem(TValue value) => value.TemplateId;
    }
}