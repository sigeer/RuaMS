using Application.Templates;
using Microsoft.Extensions.Logging;

namespace Application.Templates.Reader
{
    /// <summary>
    /// 一个文件映射一个（或一组）template
    /// </summary>
    /// <typeparam name="TTemplate"></typeparam>
    public abstract class AbstractGroupProvider<TTemplate> : AbstractProvider<TTemplate> where TTemplate : AbstractTemplate
    {
        protected AbstractGroupProvider(IWzPathResolver resolver, bool useCache = true) : base(resolver, useCache)
        {
        }

        protected override IEnumerable<TTemplate> LoadAllInternal()
        {
            List<TTemplate> all = new List<TTemplate>();
            try
            {
                foreach (var file in _resolver.ResolveGroup(Type))
                {
                    all.AddRange(GetDataFromImg(file));
                }
            }
            catch (Exception ex)
            {
                LibLog.Logger.LogError(ex.ToString());
            }
            return all;
        }

        protected override TTemplate? GetItemInternal(int templateId)
        {
            var path = _resolver.ResolveItem(Type, templateId);
            return GetDataFromImg(path).FirstOrDefault(x => x.TemplateId == templateId) as TTemplate;
        }

        /// <summary>
        /// 通过img获取template，并写入缓存
        /// </summary>
        /// <param name="path">img路径（对wz的相对路径）</param>
        /// <returns></returns>
        protected abstract IEnumerable<TTemplate> GetDataFromImg(string? path);
    }
}
