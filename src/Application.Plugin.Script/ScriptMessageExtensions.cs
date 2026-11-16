using Application.Core.Game.Players;
using Application.Core.scripting.Events.Instances;
using Application.Resources.Messages;
using Application.Shared.MapObjects.Players;
using scripting;
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
    /// 脚本里取插件文案的简写：固定使用 <see cref="ScriptMessageCategories.ScriptMessage"/> 类别。
    /// <para>
    /// 注意必须带接收者调用（<c>this.GetTalkMessage(...)</c>）：扩展方法不参与无接收者的成员查找。
    /// </para>
    /// <para>
    /// NPC、传送门、道具、地图、反应堆等交互类型都派生自 <c>AbstractPlayerInteraction</c>，
    /// 因此共用一个重载；需要显式指定语言时用 <see cref="IClientCulture"/> 重载。
    /// </para>
    /// </summary>
    public static class ScriptMessageExtensions
    {
        /// <summary>从插件的 <c>ScriptMessage</c> 类别取文案，按玩家当前语言格式化。</summary>
        public static string? GetTalkMessage(this AbstractPlayerInteraction interaction, string? key, params object?[] param) =>
            interaction.getClient().CurrentCulture.GetMessageByCategory(ScriptMessageCategories.ScriptMessage, key, param);

        /// <summary>按指定文化取文案（事件模板等没有交互上下文的地方用它）。</summary>
        public static string? GetTalkMessage(this IClientCulture culture, string? key, params object?[] param) =>
            culture.GetMessageByCategory(ScriptMessageCategories.ScriptMessage, key, param);

        public static string? GetTalkMessage(this Player chr, string? key, params object?[] param) =>
            GetTalkMessage(chr.Client.CurrentCulture, key, param);
    }
}
