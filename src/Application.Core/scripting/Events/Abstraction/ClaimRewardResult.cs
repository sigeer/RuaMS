using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.scripting.Events.Abstraction
{
    public enum ClaimRewardResult : byte
    {
        Success,

        BagFull,
        Claimed
    }
}
