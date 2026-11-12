using scripting.npc;

namespace Application.Plugin.Script
{
    /// <summary>
    /// 插件自己定义的消息类别名。
    /// <para>
    /// 类别名是插件自己的概念：Core 只提供「按类别 + 键取文案」的通用入口
    /// （<see cref="NPCConversationManager.GetMessageByCategory"/>、<c>IClientCulture.GetMessageByCategory</c>），
    /// 不会去认识 <c>ScriptMessage</c> 这种插件内部的名字。
    /// </para>
    /// </summary>
    public static class ScriptMessageCategories
    {
        /// <summary>NPC / 事件脚本文案，位于插件目录的 <c>i18n/ScriptMessage</c>。</summary>
        public const string ScriptMessage = "ScriptMessage";
    }

    /// <summary>
    /// NPC 脚本里取插件文案的简写：固定使用 <see cref="ScriptMessageCategories.ScriptMessage"/> 类别。
    /// <para>
    /// 注意必须带接收者调用（<c>this.GetTalkMessage(...)</c>）：扩展方法不参与无接收者的成员查找。
    /// </para>
    /// </summary>
    public static class ScriptMessageExtensions
    {
        /// <summary>从插件的 <c>ScriptMessage</c> 类别取文案，按玩家当前语言格式化。</summary>
        public static string GetTalkMessage(this NPCConversationManager conversation, string key, params object[] param) =>
            conversation.getClient().CurrentCulture.GetMessageByCategory(ScriptMessageCategories.ScriptMessage, key, param);
    }
}
