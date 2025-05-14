using net.server.coordinator.session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.Datas
{
    public class CharacterValueObject
    {
        public CharacterEntity CharacterEntity { get; set; }
        public CharacterLoginInfo SessionInfo { get; set; }
        public AccountEntity AccountEntity { get; }

    }

    public class CharacterLoginInfo
    {
        public bool IsAccountOnlined { get; }
        public bool IsPlayerOnlined { get; }
        public Hwid Hwid { get; }
    }
}
