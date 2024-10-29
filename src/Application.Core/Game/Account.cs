using server;

namespace Application.Core.Game
{
    public class Account : IAccount
    {
        public int Id { get; }
        public string Name { get; }
        public IPlayer? OnlinedCharacter { get; }

        public List<IPlayer> AccountCharacterList { get; }

        public Storage Storage { get; }
        public int World { get; }

        public Account(string accountName)
        {
            Name = accountName;
        }

        public void Login()
        {

        }

        public void LoginSuccess()
        {
        }
    }
}
