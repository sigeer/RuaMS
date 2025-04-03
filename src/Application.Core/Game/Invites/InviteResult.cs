using net.server.coordinator.world;

namespace Application.Core.Game.Invites
{
    public enum InviteResultType
    {
        ACCEPTED,
        DENIED,
        NOT_FOUND
    }

    public class InviteResult
    {
        public InviteResult(InviteResultType result, InviteRequest? request)
        {
            Result = result;
            Request = request;
        }

        public InviteResultType Result { get; set; }
        /// <summary>
        /// Result = NOT_FOUND时为null
        /// </summary>
        public InviteRequest? Request { get; set; }
    }
}
