using Application.Templates;

namespace Application.Templates.Reader
{
    /// <summary>
    /// 一个文件映射所有template
    /// </summary>
    /// <typeparam name="TTemplate"></typeparam>
    public abstract class AbstractAllProvider<TTemplate> : AbstractProvider<TTemplate> where TTemplate : AbstractTemplate
    {

        protected AbstractAllProvider(IWzPathResolver fileMapping, bool useCache = true) : base(fileMapping, useCache)
        {
        }


        protected abstract IEnumerable<TTemplate> GetDataFromImg();

        protected override IEnumerable<TTemplate> LoadAllInternal()
        {
            return GetDataFromImg();
        }

        protected override TTemplate? GetItemInternal(int templateId)
        {
            return GetDataFromImg().FirstOrDefault(x => x.TemplateId == templateId);
        }
    }
}
