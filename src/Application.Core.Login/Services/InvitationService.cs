using Application.Core.Login.Models.Invitations;
using Application.Core.Login.ServerData;
using Application.Utility.Exceptions;
using Dto;
using System.Threading.Tasks;

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

        public async Task AddInvitation(InvitationProto.CreateInviteRequest request)
        {
            var handler = _inviteRegistry.GetHandler(request.Type);
            if (handler == null)
                throw new BusinessException($"不支持的邀请类型：{request.Type}");

            await handler.HandleInvitationCreated(request);
        }

        public async Task AnswerInvitation(InvitationProto.AnswerInviteRequest request)
        {
            var handler = _inviteRegistry.GetHandler(request.Type);
            if (handler == null)
                throw new BusinessException($"不支持的邀请类型：{request.Type}");

            await handler.HandleInvitationAnswered(request);
        }
    }
}
