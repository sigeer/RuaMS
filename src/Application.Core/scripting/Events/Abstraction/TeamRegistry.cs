using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.scripting.Events.Abstraction
{
    public record TeamRegistry(int Team, List<Player> EligibleMembers);
}
