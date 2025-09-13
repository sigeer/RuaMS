namespace Application.Templates
{
    public interface ILinkTemplate<TTemplate> where TTemplate : AbstractTemplate
    {
        /// <summary>
        /// 浅克隆，不复制info节的内容
        /// </summary>
        /// <param name="sourceTemplate"></param>
        void CloneLink(TTemplate sourceTemplate);
    }
}
