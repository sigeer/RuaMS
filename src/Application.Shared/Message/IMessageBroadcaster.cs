using Application.Shared.Constants;
using Application.Shared.MapObjects.Players;

namespace Application.Shared.Message
{
    public interface IMessageBroadcaster<TClient> where TClient : IClientMessenger
    {
        List<TClient> GetItems();
    }

    public static class IMessageBroadcasterExtensions
    {
        public static async Task TypedMessage<TClient>(this IMessageBroadcaster<TClient> c, NoticeType type, string? messageKey, params object?[] param) where TClient : IClientMessenger
        {
            foreach (var item in c.GetItems())
            {
                await item.TypedMessage(type, messageKey, param);
            }
        }

        public static Task Pink<TClient>(this IMessageBroadcaster<TClient> c, string? key, params object?[] param) where TClient : IClientMessenger 
            => c.TypedMessage(NoticeType.Pink, key, param);
        public static Task Notice<TClient>(this IMessageBroadcaster<TClient> c, string? key, params object?[] param) where TClient : IClientMessenger 
            => c.TypedMessage(NoticeType.Notice, key, param);
        public static Task Popup<TClient>(this IMessageBroadcaster<TClient> c, string? key, params object?[] param) where TClient : IClientMessenger 
            => c.TypedMessage(NoticeType.Popup, key, param);
        public static Task TopScrolling<TClient>(this IMessageBroadcaster<TClient> c, string? key, params object?[] param) where TClient : IClientMessenger 
            => c.TypedMessage(NoticeType.TopScrolling, key, param);
        public static Task LightBlue<TClient>(this IMessageBroadcaster<TClient> c, string? key, params object?[] param) where TClient : IClientMessenger 
            => c.TypedMessage(NoticeType.LightBlue, key, param);
        public static Task Yellow<TClient>(this IMessageBroadcaster<TClient> c, string? key, params object?[] param) where TClient : IClientMessenger
            => c.TypedMessage(NoticeType.Yellow, key, param);
        public static Task EarnTitle<TClient>(this IMessageBroadcaster<TClient> c, string? key, params object?[] param) where TClient : IClientMessenger 
            => c.TypedMessage(NoticeType.EarnTitle, key, param);

        public static async Task Dialog<TClient>(this IMessageBroadcaster<TClient> c, string? key, int npc, params object?[] param) where TClient : IClientMessenger
        {
            foreach (var item in c.GetItems())
            {
                await item.Dialog(key, npc, param);
            }
        }

        #region MyRegion
        public static async Task TypedMessage<TClient>(this IMessageBroadcaster<TClient> c, string category, NoticeType type, string? messageKey, params object?[] param) 
            where TClient : IClientMessenger
        {
            foreach (var item in c.GetItems())
            {
                await item.TypedMessage(type, item.GetMessageByCategory(category, messageKey, param));
            }
        }

        public static Task Pink<TClient>(this IMessageBroadcaster<TClient> c, string category, string? key, params object?[] param) where TClient : IClientMessenger
            => c.TypedMessage(category, NoticeType.Pink, key, param);
        public static Task Notice<TClient>(this IMessageBroadcaster<TClient> c, string category, string? key, params object?[] param) where TClient : IClientMessenger
            => c.TypedMessage(category, NoticeType.Notice, key, param);
        public static Task Popup<TClient>(this IMessageBroadcaster<TClient> c, string category, string? key, params object?[] param) where TClient : IClientMessenger
            => c.TypedMessage(category, NoticeType.Popup, key, param);
        public static Task TopScrolling<TClient>(this IMessageBroadcaster<TClient> c, string category, string? key, params object?[] param) where TClient : IClientMessenger
            => c.TypedMessage(category, NoticeType.TopScrolling, key, param);
        public static Task LightBlue<TClient>(this IMessageBroadcaster<TClient> c, string category, string? key, params object?[] param) where TClient : IClientMessenger
            => c.TypedMessage(category, NoticeType.LightBlue, key, param);
        public static Task Yellow<TClient>(this IMessageBroadcaster<TClient> c, string category, string? key, params object?[] param) where TClient : IClientMessenger
            => c.TypedMessage(category, NoticeType.Yellow, key, param);
        public static Task EarnTitle<TClient>(this IMessageBroadcaster<TClient> c, string category, string? key, params object?[] param) where TClient : IClientMessenger
            => c.TypedMessage(category, NoticeType.EarnTitle, key, param);

        public static async Task Dialog<TClient>(this IMessageBroadcaster<TClient> c, string category, string? key, int npc, params object?[] param) where TClient : IClientMessenger
        {
            foreach (var item in c.GetItems())
            {
                await item.Dialog(item.GetMessageByCategory(category, key, param), npc);
            }
        }
        #endregion


        public static async Task TypedMessage<TClient>(this IMessageBroadcaster<TClient> c, NoticeType type, Func<TClient, string?> action) where TClient : IClientMessenger
        {
            foreach (var item in c.GetItems())
            {
                await item.TypedMessage(type, action(item));
            }
        }

        public static Task Pink<TClient>(this IMessageBroadcaster<TClient> c, Func<TClient, string?> action) where TClient : IClientMessenger
            => c.TypedMessage(NoticeType.Pink, action);
        public static Task Notice<TClient>(this IMessageBroadcaster<TClient> c, Func<TClient, string?> action) where TClient : IClientMessenger
            => c.TypedMessage(NoticeType.Notice, action);
        public static Task Popup<TClient>(this IMessageBroadcaster<TClient> c, Func<TClient, string?> action) where TClient : IClientMessenger
            => c.TypedMessage(NoticeType.Popup, action);
        public static Task TopScrolling<TClient>(this IMessageBroadcaster<TClient> c, Func<TClient, string?> action) where TClient : IClientMessenger
            => c.TypedMessage(NoticeType.TopScrolling, action);
        public static Task LightBlue<TClient>(this IMessageBroadcaster<TClient> c, Func<TClient, string?> action) where TClient : IClientMessenger
            => c.TypedMessage(NoticeType.LightBlue, action);
        public static Task Yellow<TClient>(this IMessageBroadcaster<TClient> c, Func<TClient, string?> action) where TClient : IClientMessenger
            => c.TypedMessage(NoticeType.Yellow, action);
        public static Task EarnTitle<TClient>(this IMessageBroadcaster<TClient> c, Func<TClient, string?> action) where TClient : IClientMessenger
            => c.TypedMessage(NoticeType.EarnTitle, action);

        public static async Task Dialog<TClient>(this IMessageBroadcaster<TClient> c, Func<TClient, string?> action, int npc) where TClient : IClientMessenger
        {
            foreach (var item in c.GetItems())
            {
                await item.Dialog(action(item), npc);
            }
        }
    }
}
