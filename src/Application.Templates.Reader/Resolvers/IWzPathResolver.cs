using System.Globalization;

namespace Application.Templates.Reader
{
    public interface IWzPathResolver
    {
        /// <summary>WZ 数据根目录，所有 Provider 共用。</summary>
        string BaseDir { get; }


        /// <summary>
        /// 当前type下所有文件
        /// </summary>
        /// <param name="type"></param>
        /// <returns>相对路径</returns>
        string[] ResolveGroup(ProviderType type);

        /// <summary>
        /// 通过 type, id 找到对应文件
        /// </summary>
        /// <param name="type"></param>
        /// <param name="templateId"></param>
        /// <returns>相对路径</returns>
        string ResolveItem(ProviderType type, int templateId);
    }
}
