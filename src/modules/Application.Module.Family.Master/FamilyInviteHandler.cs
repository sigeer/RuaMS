using Application.Core.Login;
using Application.Core.Login.Models.Invitations;
using Application.Module.Family.Common;
using Application.Shared.Invitations;
using Application.Utility.Configs;
using Dto;

namespace Application.Module.Family.Master
{
    internal class FamilyInviteHandler : InviteMasterHandler
    {
        readonly FamilyManager _familyManager;
        public FamilyInviteHandler(MasterServer server, FamilyManager familyManager) : base(server, Constants.InviteType_Family)
        {
            _familyManager = familyManager;
        }

        protected override void OnInvitationAccepted(InviteRequest request)
        {
            _familyManager.AcceptInvite(request.ToPlayerId, request.FromPlayerId);
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

            else if (toPlayer.Character.Level <= 10)
            {

            }

            else if (Math.Abs(toPlayer.Character.Level - fromPlayer.Character.Level) > 20)
            {

            }

            else if (_familyManager.GetFamilyCharacter(toPlayer.Character.Id)?.Familyid == _familyManager.GetFamilyCharacter(fromPlayer.Character.Id)?.Familyid)
            {

            }

            BroadcastResult(responseCode, fromPlayer.Character.Party, fromPlayer, toPlayer, request.ToName);
        }
    }
}
