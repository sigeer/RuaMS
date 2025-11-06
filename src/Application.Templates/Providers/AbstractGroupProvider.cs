using Microsoft.Extensions.Logging;

namespace Application.Templates.Providers
{
    /// <summary>
    /// 一个文件映射一个（或一组）template
    /// </summary>
    /// <typeparam name="TTemplate"></typeparam>
    public abstract class AbstractGroupProvider<TTemplate> : AbstractProvider<TTemplate> where TTemplate : AbstractTemplate
    {
        /// <summary>
        /// .img.xml文件对wz根目录的相对路径
        /// </summary>
        protected string[] _files;
        protected AbstractGroupProvider(ProviderOption options) : base(options)
        {
            _files = Directory.GetFiles(Path.Combine(GetBaseDir(), ProviderName), "*.xml", SearchOption.AllDirectories)
                .Select(x => Path.GetRelativePath(GetBaseDir(), x))
                .ToArray();
        }

        protected override IEnumerable<AbstractTemplate> LoadAllInternal()
        {
            List<AbstractTemplate> all = new List<AbstractTemplate>();
            try
            {
                foreach (var file in _files)
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
            return GetDataFromImg(GetImgPathByTemplateId(templateId)).FirstOrDefault(x => x.TemplateId == templateId) as TTemplate;
        }

        /// <summary>
        /// 通过img获取template，并写入缓存
        /// </summary>
        /// <param name="path">img路径（对wz的相对路径）</param>
        /// <returns></returns>
        protected abstract IEnumerable<AbstractTemplate> GetDataFromImg(string? path);

        /// <summary>
        /// 通过templateId获取相应img文件路径（相对路径）
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns>img文件路径（相对路径）</returns>
        protected abstract string? GetImgPathByTemplateId(int templateId);
    }
}
