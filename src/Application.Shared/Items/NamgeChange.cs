using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Shared.Items
{
    public enum ChangeNameResponseCode
    {
        Success = 0,

        InvalidName,
        CharacterNotFound,
        Level
    }
}
