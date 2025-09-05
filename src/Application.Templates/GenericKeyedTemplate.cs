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
        protected Dictionary<int, TValue> _templates;

        public GenericKeyedTemplate(int templateId) : base(templateId)
        {
            _templates = new Dictionary<int, TValue>();
        }

        public int Count => _templates.Count;

        public TValue this[int key] =>
            _templates.ContainsKey(key)
                ? _templates[key]
                : default;

        public void Add(TValue value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));

            var existing = this[value.TemplateId];

            if (existing == null) // trying to insert VIP shields twice for some reason??
            {
                _templates.Add(value.TemplateId, value);
            }
        }

        public Dictionary<int, TValue>.Enumerator GetEnumerator() => _templates.GetEnumerator();

        protected virtual int GetKeyForItem(TValue value) => value.TemplateId;
    }
}