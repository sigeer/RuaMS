using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Shared.Invitations
{
    public enum InviteResponseCode
    {
        Success,
        MANAGING_INVITE,

        InviteesNotFound,

        ChatRoom_AlreadInRoom,
        ChatRoom_CapacityFull,

        Team_AlreadyInTeam,
        Team_CapacityFull,
        Team_BeginnerLimit,

        Guild_AlreadInGuild,

        Alliance_AlreadyInAlliance,
        Alliance_GuildNotFound,
        Alliance_GuildLeaderNotFound,
        Alliance_CapacityFull,
    }
}
