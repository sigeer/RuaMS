using Dto;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.Login.Servers
{
    internal class BuddyGrpcService : ServiceProto.BuddyService.BuddyServiceBase
    {
        readonly MasterServer _server;

        public BuddyGrpcService(MasterServer server)
        {
            _server = server;
        }

        public override Task<AddBuddyResponse> AddBuddyById(AddBuddyByIdRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.BuddyManager.AddBuddyById(request));
        }

        public override Task<AddBuddyResponse> AddBuddyByName(AddBuddyRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.BuddyManager.AddBuddyByName(request));
        }

        public override Task<DeleteBuddyResponse> DeleteBuddy(DeleteBuddyRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.BuddyManager.DeleteBuddy(request));
        }

        public override Task<GetLocationResponse> GetLocation(GetLocationRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.BuddyManager.GetLocation(request));
        }

        public override Task<Empty> SendBuddfyNotice(SendBuddyNoticeMessageDto request, ServerCallContext context)
        {
            _server.BuddyManager.BroadcastNoticeMessage(request);
            return Task.FromResult(new Empty());
        }

        public override Task<SendWhisperMessageResponse> SendWhisper(SendWhisperMessageRequest request, ServerCallContext context)
        {
            return Task.FromResult(_server.BuddyManager.SendWhisper(request));
        }
    }
}
