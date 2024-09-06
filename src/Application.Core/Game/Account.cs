using Application.Core.Game.TheWorld;
using net.server;
using server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.Game
{
    public class Account1 : IAccount
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
