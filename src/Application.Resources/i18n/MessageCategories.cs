namespace Application.Resources.Messages
{
    /// <summary>
    /// 宿主自带的通用消息类别名。值与 <c>Application.Resources/i18n</c> 下的目录名一致。
    /// <para>
    /// 插件文案不需要在这里登记：类别名只是字符串，插件把 <c>i18n/{Category}/{Culture}.json</c>
    /// 放进自己的插件目录即可，例如 <c>Application.Plugin.Script</c> 的 <c>ScriptMessage</c>。
    /// </para>
    /// </summary>
    public static class MessageCategories
    {
        /// <summary>宿主通用消息，位于 <c>Application.Resources/i18n/ClientMessage</c>。</summary>
        public const string ClientMessage = "ClientMessage";
    }
}
