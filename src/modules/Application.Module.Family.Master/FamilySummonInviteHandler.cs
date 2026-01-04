using Application.Core.Login;
using Application.Core.Login.Models.Invitations;
using Application.Module.Family.Common;
using Application.Shared.Invitations;
using Dto;
using InvitationProto;

namespace Application.Module.Family.Master
{
    internal class FamilySummonInviteHandler : InviteMasterHandler
    {
        FamilyManager _familyManager;

        public FamilySummonInviteHandler(MasterServer server, FamilyManager familyManager) : base(server, Constants.InviteType_FamilySummon)
        {
            _familyManager = familyManager;
        }

        protected override async Task OnInvitationAccepted(InviteRequest request)
        {
            await _familyManager.UseEntitlement(new UseEntitlementRequest
            {
                MatserId = request.FromPlayerId,
                TargetPlayerId = request.ToPlayerId,
                EntitlementId = FamilyEntitlement.SUMMON_FAMILY.Value
            });
        }

        protected override async Task OnInvitationDeclined(InviteRequest request)
        {
            await _familyManager.Refund(request.FromPlayerId);
        }

        public override async Task OnInvitationExpired(InviteRequest request)
        {
            await _familyManager.Refund(request.FromPlayerId);
        }

        public override async Task HandleInvitationCreated(CreateInviteRequest request)
        {
            InviteResponseCode responseCode = InviteResponseCode.Success;
            var fromPlayer = _server.CharacterManager.FindPlayerById(request.FromId)!;
            var toPlayer = _server.CharacterManager.FindPlayerByName(request.ToName);
            if (toPlayer == null || toPlayer.Channel <= 0)
            {
                responseCode = InviteResponseCode.InviteesNotFound;
            }

            // cost检测
            await BroadcastResult(responseCode, fromPlayer.Character.Id, fromPlayer, toPlayer, request.ToName);
        }
    }
}
