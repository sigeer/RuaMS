namespace Application.Core.Channel.Invitation
{
    public class InviteChannelHandlerRegistry : IDisposable
    {
        private readonly Dictionary<string, InviteChannelHandler> _handlers = new();

        /// <summary>注册一个 InviteHandler</summary>
        public void Register(InviteChannelHandler handler)
        {
            if (_handlers.ContainsKey(handler.Type))
                throw new InvalidOperationException($"Handler for invite type '{handler.Type}' already registered.");

            _handlers[handler.Type] = handler;
        }

        public void Register(IEnumerable<InviteChannelHandler> handlers)
        {
            foreach (var handler in handlers)
            {
                Register(handler);
            }
        }

        /// <summary>获取对应类型的 Handler，如果不存在返回 null</summary>
        public InviteChannelHandler? GetHandler(string inviteType)
        {
            return _handlers.GetValueOrDefault(inviteType);
        }

        /// <summary>获取所有已注册的邀请类型</summary>
        public IReadOnlyCollection<string> GetRegisteredTypes() => _handlers.Keys.ToList();

        public void Dispose()
        {
            _handlers.Clear();
        }
    }
}
