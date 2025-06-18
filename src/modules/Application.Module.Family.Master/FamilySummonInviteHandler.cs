using Application.Core.Login;
using Application.Core.Login.Models.Invitations;
using Application.Module.Family.Common;
using Application.Shared.Invitations;
using Dto;

namespace Application.Module.Family.Master
{
    internal class FamilySummonInviteHandler : InviteMasterHandler
    {
        FamilyManager _familyManager;

        public FamilySummonInviteHandler(MasterServer server, FamilyManager familyManager) : base(server, Constants.InviteType_FamilySummon)
        {
            _familyManager = familyManager;
        }

        protected override void OnInvitationAccepted(InviteRequest request)
        {
            _familyManager.UseEntitlement(new UseEntitlementRequest
            {
                MatserId = request.FromPlayerId,
                TargetPlayerId = request.ToPlayerId,
                EntitlementId = FamilyEntitlement.SUMMON_FAMILY.Value
            });
        }

        protected override void OnInvitationDeclined(InviteRequest request)
        {
            _familyManager.Refund(request.FromPlayerId);
        }

        public override void OnInvitationExpired(InviteRequest request)
        {
            _familyManager.Refund(request.FromPlayerId);
        }

        public override void HandleInvitationCreated(CreateInviteRequest request)
        {
            InviteResponseCode responseCode = InviteResponseCode.Success;
            var fromPlayer = _server.CharacterManager.FindPlayerById(request.FromId)!;
            var toPlayer = _server.CharacterManager.FindPlayerByName(request.ToName);
            if (toPlayer == null || toPlayer.Channel <= 0)
            {
                responseCode = InviteResponseCode.InviteesNotFound;
            }

            // cost检测
            BroadcastResult(responseCode, fromPlayer.Character.Id, fromPlayer, toPlayer, request.ToName);
        }
    }
}
