using Application.Shared.Invitations;

namespace Application.Core.Login.Models.Invitations
{


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
