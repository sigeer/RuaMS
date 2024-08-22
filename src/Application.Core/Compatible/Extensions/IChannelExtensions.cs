using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;

namespace Application.Core.Compatible.Extensions
{
    public static class IChannelExtensions
    {
        public static IAttribute<TValue> GetAttribute<TValue>(this IChannel channel, string item) where TValue : class
        {
            return channel.GetAttribute(AttributeKey<TValue>.ValueOf(item));
        }
        public static TValue? GetAttributeValue<TValue>(this IChannel channel, string item) where TValue : class
        {
            return channel.GetAttribute(AttributeKey<TValue>.ValueOf(item)).Get();
        }

        public static void SetAttributeValue<TValue>(this IChannel channel, string item, TValue value) where TValue : class
        {
            channel.GetAttribute(AttributeKey<TValue>.ValueOf(item)).Set(value);
        }

        public static TValue GetAndRemoveAttributeValue<TValue>(this IChannel channel, string item) where TValue : class
        {
            return channel.GetAttribute(AttributeKey<TValue>.ValueOf(item)).GetAndRemove();
        }

        public static void RemoveAttribute<TValue>(this IChannel channel, string item) where TValue : class
        {
            channel.GetAttribute(AttributeKey<TValue>.ValueOf(item)).Remove();
        }
        public static bool ContainsAttribute<TValue>(this IChannel channel, string item) where TValue : class
        {
            return channel.GetAttribute(AttributeKey<TValue>.ValueOf(item)).Get() != null;
        }
    }
}
