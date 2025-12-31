using Application.Shared.Invitations;
using Application.Shared.Message;
using Dto;
using System.Threading.Tasks;

namespace Application.Core.Login.Models.Invitations
{
    public class InviteMasterHandlerRegistry
    {
        private readonly Dictionary<string, InviteMasterHandler> _handlers = new();

        /// <summary>注册一个 InviteHandler</summary>
        public void Register(InviteMasterHandler handler)
        {
            if (_handlers.ContainsKey(handler.Type))
                throw new InvalidOperationException($"Handler for invite type '{handler.Type}' already registered.");

            _handlers[handler.Type] = handler;
        }

        public void Register(IEnumerable<InviteMasterHandler> handlers)
        {
            foreach (var handler in handlers)
            {
                Register(handler);
            }
        }


        /// <summary>获取对应类型的 Handler，如果不存在返回 null</summary>
        public InviteMasterHandler? GetHandler(string inviteType)
        {
            return _handlers.GetValueOrDefault(inviteType);
        }

        /// <summary>获取所有已注册的邀请类型</summary>
        public IReadOnlyCollection<string> GetRegisteredTypes() => _handlers.Keys.ToList();
    }
    public abstract class AbstractInviteType
    {
        public abstract string Name { get; }
    }

    public abstract class InviteMasterHandler
    {
        protected MasterServer _server;
        public string Type { get; }
        protected InviteMasterHandler(MasterServer server, string type)
        {
            _server = server;
            Type = type;
        }

        public abstract Task HandleInvitationCreated(InvitationProto.CreateInviteRequest request);
        public async Task HandleInvitationAnswered(InvitationProto.AnswerInviteRequest request)
        {
            var result = _server.InvitationManager.AnswerInvite(Type, request.MasterId, request.CheckKey, request.Ok);
            var response = new InvitationProto.AnswerInviteResponse
            {
                Result = (int)result.Result,
                Type = request.Type,
                SenderPlayerId = request.MasterId,
            };

            if (result.Result != InviteResultType.NOT_FOUND && result.Request != null)
            {
                if (result.Result == InviteResultType.ACCEPTED)
                {
                    await OnInvitationAccepted(result.Request);
                }
                else
                {
                    await OnInvitationDeclined(result.Request);
                }

                response.Code = (int)result.Result;
                response.Result = (int)result.Result;
                response.Key = result.Request.Key;
                response.ReceivePlayerId = result.Request.ToPlayerId;
                response.ReceivePlayerName = result.Request.ToPlayerName;
                response.Type = Type;
                response.SenderPlayerId = result.Request.FromPlayerId;
                response.SenderPlayerName = result.Request.FromPlayerName;
                response.TargetName = result.Request.TargetName;
            }
            await _server.Transport.SendMessageN(ChannelRecvCode.OnInvitationAnswered, response, [response.SenderPlayerId, response.ReceivePlayerId]);
        }

        protected virtual Task OnInvitationAccepted(InviteRequest request)
        {
            return Task.CompletedTask;
        }
        protected virtual Task OnInvitationDeclined(InviteRequest request)
        {
            return Task.CompletedTask;
        }
        /// <summary>
        /// 邀请过期时触发
        /// </summary>
        /// <param name="request"></param>
        public virtual Task OnInvitationExpired(InviteRequest request)
        {
            return Task.CompletedTask;
        }

        protected virtual async Task BroadcastResult(InviteResponseCode responseCode, int key, CharacterLiveObject fromPlayer, CharacterLiveObject? toPlayer, string targetName)
        {
            if (responseCode == InviteResponseCode.Success)
            {
                var request = new InviteRequest(
                    _server.getCurrentTime(),
                    fromPlayer.Character.Id, fromPlayer.Character.Name,
                    toPlayer!.Character.Id, toPlayer!.Character.Name,
                    key,
                    targetName);
                if (!_server.InvitationManager.TryAddInvitation(Type, toPlayer!.Character.Id, request))
                {
                    responseCode = InviteResponseCode.MANAGING_INVITE;
                }
            }

            var response = new InvitationProto.CreateInviteResponse
            {
                Code = (int)responseCode,
                SenderPlayerId = fromPlayer.Character.Id,
                SenderPlayerName = fromPlayer.Character.Name,
                Type = Type,
            };
            if (responseCode == InviteResponseCode.Success)
            {
                response.Key = key;
                response.ReceivePlayerId = toPlayer!.Character.Id;
                response.ReceivePlayerName = toPlayer.Character.Name;
            }
            await _server.Transport.SendMessageN(ChannelRecvCode.OnInvitationSent, response, [response.SenderPlayerId, response.ReceivePlayerId]);
        }


    }


}
