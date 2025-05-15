using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Shared.Sessions
{
    public enum AntiMulticlientResult
    {
        SUCCESS,
        REMOTE_LOGGEDIN,
        REMOTE_REACHED_LIMIT,
        REMOTE_PROCESSING,
        REMOTE_NO_MATCH,
        MANY_ACCOUNT_ATTEMPTS,
        COORDINATOR_ERROR
    }
}
