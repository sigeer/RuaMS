using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Module.Marriage.Common.ErrorCodes
{
    public enum InviteErrorCode
    {
        Success = 0,
        GuestNotFound,
        MarriageNotFound,
        DuplicateInvitation,
        WeddingUnderway
    }
}
