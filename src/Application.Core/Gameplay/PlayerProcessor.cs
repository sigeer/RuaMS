namespace Application.Core.Gameplay
{
    public abstract class PlayerProcessor<TMessage>
    {
        protected readonly Player _player;

        protected PlayerProcessor(Player player)
        {
            _player = player;
        }

        protected abstract Task Process(TMessage message);

        protected virtual Task<bool> Before(TMessage message)
        {
            return Task.FromResult(true);
        }
        protected virtual Task After(TMessage message)
        {
            return Task.CompletedTask;
        }
        public virtual async Task Handle(TMessage message)
        {
            if (!await Before(message))
                return;

            await Process(message);
            await After(message);
        }

    }
}
