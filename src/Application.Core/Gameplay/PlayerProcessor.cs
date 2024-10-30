namespace Application.Core.Gameplay
{
    public abstract class PlayerProcessor<TMessage>
    {
        protected readonly IPlayer _player;

        protected PlayerProcessor(IPlayer player)
        {
            _player = player;
        }

        protected abstract void Process(TMessage message);

        protected virtual bool Before(TMessage message)
        {
            return true;
        }
        protected virtual void After(TMessage message)
        {
            return;
        }
        public virtual void Handle(TMessage message)
        {
            if (!Before(message))
                return;

            Process(message);
            After(message);
        }

    }
}
