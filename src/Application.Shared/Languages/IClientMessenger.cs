namespace Application.Shared.Languages
{
    /// <summary>
    /// 使用ClientMessage.resx的内容
    /// </summary>
    public interface IClientMessenger
    {
        void TypedMessage(int type, string messageKey, params string[] param);
        /// <summary>
        /// type = 0
        /// <para>蓝色：[Notice] xxxx</para>
        /// </summary>
        /// <param name="key"></param>
        void Notice(string key, params string[] param);
        /// <summary>
        /// type = 1 弹窗
        /// </summary>
        /// <param name="key"></param>
        void Popup(string key, params string[] param);
        /// <summary>
        /// 对话框
        /// </summary>
        /// <param name="key"></param>
        void Dialog(string key, params string[] param);
        /// <summary>
        /// type = 5 聊天框红色
        /// </summary>
        /// <param name="key"></param>
        void Pink(string key, params string[] param);
        /// <summary>
        /// type = 6 聊天框蓝色
        /// </summary>
        /// <param name="key"></param>
        void LightBlue(string key, params string[] param);
        /// <summary>
        /// type = 4 顶部滚动
        /// </summary>
        /// <param name="key"></param>
        void TopScrolling(string key, params string[] param);
        /// <summary>
        /// 聊天框黄色
        /// </summary>
        /// <param name="key"></param>
        void Yellow(string key, params string[] param);
    }
}
