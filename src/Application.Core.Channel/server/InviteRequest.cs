using Application.Core.Game.Maps;

namespace Application.Core.Request.Invites
{
    public class InviteRequest
    {
        public InviteRequest(Player from, Player to)
        {
            CreationTime = from.Client.CurrentServer.getCurrentTime();
            From = from;
            To = to;
        }

        public long CreationTime { get; set; }
        public Player From { get; }
        public Player To { get; }
    }

    public class GuildInviteRequest : InviteRequest
    {
        public GuildInviteRequest(Player from, Player to) : base(from, to)
        {
            GuildId = from.GuildId;
        }

        public int GuildId { get; set; }
    }

    public class AllianceInviteRequest : InviteRequest
    {
        public AllianceInviteRequest(Player from, Player to) : base(from, to)
        {
            AllianceId = from.AllianceModel!.AllianceId;
        }

        public int AllianceId { get; set; }
    }

    public class TeamInviteRequest : InviteRequest
    {
        public TeamInviteRequest(Player from, Player to) : base(from, to)
        {
            TeamId = from.TeamModel!.getId();
        }

        public int TeamId { get; set; }
    }

    public class ChatInviteRequest : InviteRequest
    {
        public ChatInviteRequest(Player from, Player to, int messngerId) : base(from, to)
        {
            ChatRoomId = messngerId;
        }

        public int ChatRoomId { get; set; }
    }

    public class FamilySummonInviteRequest : InviteRequest
    {
        public FamilySummonInviteRequest(Player from, Player to) : base(from, to)
        {
            Map = from.getMap();
        }

        public IMap Map { get; set; }
    }
}
