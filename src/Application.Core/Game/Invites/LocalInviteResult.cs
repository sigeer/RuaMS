using Application.Shared.Invitations;

namespace Application.Core.Game.Invites
{
    public class LocalInviteResult
    {
        public LocalInviteResult(InviteResultType result, LocalInviteRequest? request)
        {
            Result = result;
            Request = request;
        }

        public InviteResultType Result { get; set; }
        /// <summary>
        /// Result = NOT_FOUND时为null
        /// </summary>
        public LocalInviteRequest? Request { get; set; }
    }
}
