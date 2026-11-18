using Application.Shared.Constants;

namespace Application.Shared.MapObjects.Players
{
    /// <summary>
    /// 使用ClientMessage.resx的内容
    /// </summary>
    public interface IClientMessenger
    {
        string? GetMessageByCategory(string category, string? key, params object?[] paramsValue);
        Task TypedMessage(NoticeType type, string? messageKey, params object?[] param);

        /// <summary>
        /// 对话框
        /// </summary>
        /// <param name="key"></param>
        Task Dialog(string? key, int npc, params object?[] param);
    }

    public static class IClientMessengerExtensions
    {
        public static Task Pink(this IClientMessenger c, string? key, params object?[] param) => c.TypedMessage(NoticeType.Pink, key, param);
        public static Task Notice(this IClientMessenger c, string? key, params object?[] param) => c.TypedMessage(NoticeType.Notice, key, param);
        public static Task Popup(this IClientMessenger c, string? key, params object?[] param) => c.TypedMessage(NoticeType.Popup, key, param);
        public static Task TopScrolling(this IClientMessenger c, string? key, params object?[] param) => c.TypedMessage(NoticeType.TopScrolling, key, param);
        public static Task LightBlue(this IClientMessenger c, string? key, params object?[] param) => c.TypedMessage(NoticeType.LightBlue, key, param);
        public static Task Yellow(this IClientMessenger c, string? key, params object?[] param) => c.TypedMessage(NoticeType.Yellow, key, param);
        public static Task EarnTitle(this IClientMessenger c, string? key, params object?[] param) => c.TypedMessage(NoticeType.EarnTitle, key, param);
    }
}
