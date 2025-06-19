namespace Application.Core.Game.Invites
{
    public class LocalInviteRequest
    {
        public LocalInviteRequest(IPlayer from, IPlayer to)
        {
            CreationTime = from.Client.CurrentServerContainer.getCurrentTime();
            From = from;
            To = to;
        }

        public long CreationTime { get; set; }
        public IPlayer From { get; }
        public IPlayer To { get; }
    }
}
