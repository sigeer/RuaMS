namespace Application.Core.Channel.Message
{
    public static class MessageDispatcher
    {
        private static readonly Dictionary<string, Action<object>> _handlers = new();

        public static void Register<T>(string eventName, Action<T> handler)
        {
            if (!_handlers.TryAdd(eventName, msg => handler((T)msg)))
                throw new BusinessFatalException($"重复的事件！{eventName}");
        }

        public static void Dispatch(string eventName, object message)
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
