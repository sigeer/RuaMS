using Application.Core.Login;
using Application.Core.Login.Models.Invitations;
using Dto;

namespace Application.Module.Family.Master
{
    internal class FamilyInviteHandler : InviteMasterHandler
    {
        readonly FamilyManager _familyManager;
        public FamilyInviteHandler(MasterServer server, FamilyManager familyManager) : base(server, "Family")
        {
            _familyManager = familyManager;
        }

        public override void AcceptInvitation(InviteRequest request)
        {
            _familyManager.AcceptInvite(request.ToPlayerId, request.FromPlayerId);
        }

        public override void HandleInvitationCreated(CreateInviteRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
