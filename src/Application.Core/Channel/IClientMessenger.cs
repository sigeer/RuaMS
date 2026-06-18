namespace Application.Core.Channel
{
    /// <summary>
    /// 使用ClientMessage.resx的内容
    /// </summary>
    public interface IClientMessenger
    {
        Task TypedMessage(int type, string messageKey, params string[] param);
        /// <summary>
        /// type = 0
        /// <para>蓝色：[Notice] xxxx</para>
        /// </summary>
        /// <param name="key"></param>
        Task Notice(string key, params string[] param);
        /// <summary>
        /// type = 1 弹窗
        /// </summary>
        /// <param name="key"></param>
        Task Popup(string key, params string[] param);
        /// <summary>
        /// type = 4 顶部滚动
        /// </summary>
        /// <param name="key"></param>
        Task TopScrolling(string key, params string[] param);

        /// <summary>
        /// type = 5 聊天框红色
        /// </summary>
        /// <param name="key"></param>
        Task Pink(string key, params string[] param);
        /// <summary>
        /// type = 6 聊天框蓝色
        /// </summary>
        /// <param name="key"></param>
        Task LightBlue(string key, params string[] param);

        /// <summary>
        /// 聊天框黄色
        /// </summary>
        /// <param name="key"></param>
        Task Yellow(string key, params string[] param);
        /// <summary>
        /// 界面中心
        /// </summary>
        /// <param name="key"></param>
        /// <param name="param"></param>
        Task EarnTitle(string key, params string[] param);
        /// <summary>
        /// 对话框
        /// </summary>
        /// <param name="key"></param>
        Task Dialog(string key, params string[] param);

        /// <summary>
        /// type = 6 聊天框蓝色
        /// </summary>
        /// <param name="action"></param>
        Task LightBlue(Func<ClientCulture, string> action);
    }
}
