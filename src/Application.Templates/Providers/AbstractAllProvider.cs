namespace Application.Templates.Providers
{
    /// <summary>
    /// 一个文件映射所有template
    /// </summary>
    /// <typeparam name="TTemplate"></typeparam>
    public abstract class AbstractAllProvider<TTemplate> : AbstractProvider<TTemplate> where TTemplate : AbstractTemplate
    {
        protected string _file;
        protected AbstractAllProvider(TemplateOptions options, string file) : base(options)
        {
            _file = Path.Combine(ProviderName, file);
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
