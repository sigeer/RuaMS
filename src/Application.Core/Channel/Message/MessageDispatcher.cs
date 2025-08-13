
using BaseProto;
using Google.Protobuf;
using System.Reflection;

namespace Application.Core.Channel.Message
{
    public static class MessageDispatcher
    {
        private static readonly Dictionary<string, Action<IMessage>> _handlers = new();
        private static readonly Dictionary<string, MessageParser> _parsers = new();

        public static void Register<TMessage>(string eventName, Action<TMessage> handler)
            where TMessage : IMessage<TMessage>
        {
            if (!_handlers.TryAdd(eventName, msg => handler((TMessage)msg)))
                throw new BusinessFatalException($"重复的事件！{eventName}");

            if (!_parsers.ContainsKey(eventName))
            {
                var parser = (MessageParser)typeof(TMessage)
                    .GetProperty("Parser", BindingFlags.Public | BindingFlags.Static)!
                    .GetValue(null)!;
                _parsers[eventName] = parser;
            }
        }

        public static void Dispatch(MessageWrapper wrapper)
        {
            if (_handlers.TryGetValue(wrapper.Type, out var handler) &&
                _parsers.TryGetValue(wrapper.Type, out var parser))
            {
                var typedMsg = parser.ParseFrom(wrapper.Content);
                handler(typedMsg);
            }
            else
            {
                throw new InvalidOperationException($"未注册事件: {wrapper.Type}");
            }
        }

        public static void Dispatch(string eventName, IMessage message)
        {
            if (_handlers.TryGetValue(eventName, out var handler))
            {
                handler(message);
            }
            else
            {
                throw new BusinessFatalException($"No handler for message type {eventName}");
            }
        }
    }

}
