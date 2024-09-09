using server;

namespace Application.Core.Game
{
    public interface IAccount
    {
        public int Id { get; }
        public string Name { get; }
        public IPlayer? OnlinedCharacter { get; }
        public List<IPlayer> AccountCharacterList { get; }
        public Storage Storage { get; }
    }
}
