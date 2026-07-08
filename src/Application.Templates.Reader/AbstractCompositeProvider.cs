namespace Application.Templates.Reader
{
    /// <summary>
    /// 复合型：多个文件映射成一个（或多个）template
    /// </summary>
    /// <typeparam name="TTemplate"></typeparam>
    public abstract class AbstractCompositeProvider<TTemplate> : AbstractProvider<TTemplate> where TTemplate : AbstractTemplate
    {
        public AbstractCompositeProvider(IWzPathResolver resolver, bool useCache = true) : base(resolver, useCache)
        {
        }

        protected abstract IEnumerable<TTemplate> GetDataFromImg();
        protected override IEnumerable<TTemplate> LoadAllInternal()
        {
            return GetDataFromImg();
        }

        protected override TTemplate? GetItemInternal(int templateId)
        {
            return GetDataFromImg().FirstOrDefault(x => x.TemplateId == templateId) as TTemplate;
        }
    }
}
