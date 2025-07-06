namespace Application.Core.Channel.Infrastructures
{
    /// <summary>
    /// 仅限单例类实现该接口
    /// </summary>
    public interface IStaticService
    {
        /// <summary>
        /// 应该仅在启动时调用一次
        /// </summary>
        /// <param name="sp"></param>
        public void Register(IServiceProvider sp);
    }
}
