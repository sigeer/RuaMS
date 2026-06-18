using server.maps;

namespace Application.Core.Channel.Commands
{
    internal class ReactorRespawnCommand : IWorldChannelAsyncCommand
    {
        public string Name => nameof(ReactorRespawnCommand);
        Reactor _reactor;
        bool _fromDestroyed;

        public ReactorRespawnCommand(Reactor reactor, bool fromDestroyed)
        {
            _reactor = reactor;
            _fromDestroyed = fromDestroyed;
        }

        public async Task Execute(WorldChannel ctx)
        {
            await _reactor.respawn(_fromDestroyed);
        }
    }
}
