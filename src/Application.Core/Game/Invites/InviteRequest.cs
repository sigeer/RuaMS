namespace Application.Core.Game.Invites
{
    public class LocalInviteRequest
    {
        public LocalInviteRequest(Player from, Player to)
        {
            CreationTime = from.Client.CurrentServer.Node.getCurrentTime();
            From = from;
            To = to;
        }

        public long CreationTime { get; set; }
        public Player From { get; }
        public Player To { get; }
    }
}
