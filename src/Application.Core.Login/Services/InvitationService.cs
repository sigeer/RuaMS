using Application.Core.Login.Models.Invitations;
using Application.Core.Login.ServerData;
using Application.Utility.Exceptions;
using Dto;

namespace Application.Core.Login.Services
{
    public class InvitationService
    {
        readonly MasterServer _server;
        readonly InviteMasterHandlerRegistry _inviteRegistry;
        readonly IEnumerable<InviteMasterHandler> _allHandlers;

        public InvitationService(MasterServer server, InviteMasterHandlerRegistry inviteRegistry, IEnumerable<InviteMasterHandler> allHandlers)
        {
            _server = server;
            _inviteRegistry = inviteRegistry;
            _allHandlers = allHandlers;
        }

        public void Initialize()
        {
            _inviteRegistry.Register(_allHandlers);
        }

        public void AddInvitation(CreateInviteRequest request)
        {
            var handler = _inviteRegistry.GetHandler(request.Type);
            if (handler == null)
                throw new BusinessException($"不支持的邀请类型：{request.Type}");

            handler.HandleInvitationCreated(request);
        }

        public void AnswerInvitation(AnswerInviteRequest request)
        {
            var handler = _inviteRegistry.GetHandler(request.Type);
            if (handler == null)
                throw new BusinessException($"不支持的邀请类型：{request.Type}");

            handler.HandleInvitationAnswered(request);
        }

        public void RemovePlayerInvitation(int cid)
        {
            _server.InvitationManager.RemovePlayerInvitation(cid);
        }

    }
}
