using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Shared.Constants
{
    public enum JoinChatRoomResult
    {
        Success,

        NotFound,
        AlreadyInChatRoom,
        CapacityFull
    }
}
