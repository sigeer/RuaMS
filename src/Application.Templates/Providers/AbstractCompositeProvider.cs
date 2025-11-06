namespace Application.Templates.Providers
{
    /// <summary>
    /// 复合型：多个文件映射成一个（或多个）template
    /// </summary>
    /// <typeparam name="TTemplate"></typeparam>
    public abstract class AbstractCompositeProvider<TTemplate> : AbstractProvider<TTemplate> where TTemplate : AbstractTemplate
    {
        protected string[] _files;
        public AbstractCompositeProvider(ProviderOption options, string[] files) : base(options)
        {
            _files = files.Select(x => Path.Combine(ProviderName, x)).ToArray();
        }

        protected abstract IEnumerable<AbstractTemplate> GetDataFromImg();
        protected override IEnumerable<AbstractTemplate> LoadAllInternal()
        {
            return GetDataFromImg();
        }

        protected override TTemplate? GetItemInternal(int templateId)
        {
            return GetDataFromImg().FirstOrDefault(x => x.TemplateId == templateId) as TTemplate;
        }
    }
}
