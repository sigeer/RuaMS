using Application.Shared.Invitations;

namespace Application.Core.Login.Models.Invitations
{
    public class InviteRequest
    {
        public InviteRequest(long creationTime, int from, string fromName, int to, string toName, int key, string targetName)
        {
            CreationTime = creationTime;
            FromPlayerId = from;
            FromPlayerName = fromName;
            ToPlayerId = to;
            ToPlayerName = toName;
            Key = key;
            TargetName = targetName;
        }
        public long CreationTime { get;}
        public int FromPlayerId { get; }
        public string FromPlayerName { get; }
        public int ToPlayerId { get; }
        public string ToPlayerName { get; }
        /// <summary>
        /// 
        /// </summary>
        public int Key { get; }
        public  string TargetName { get; }
    }
}
