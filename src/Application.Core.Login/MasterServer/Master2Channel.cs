using Application.Shared.Message;
using CreatorProto;
using ExpeditionProto;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Login
{
    public partial class MasterServer
    {
        #region 请求频道服务器以创建角色
        ConcurrentDictionary<int, TaskCompletionSource<CreateCharResponseDto>> createCharRequests = [];
        public async Task<CreateCharResponseDto> SendCreateCharacterRequest(CreateCharRequestDto request)
        {
            var tcs = new TaskCompletionSource<CreateCharResponseDto>();
            if (!createCharRequests.TryAdd(request.AccountId, tcs))
            {
                return new CreatorProto.CreateCharResponseDto { Code = -2 };
            }
            createCharRequests[request.AccountId] = tcs;

            var defaultServer = ChannelServerList.FirstOrDefault().Value;
            if (defaultServer == null)
                return new CreatorProto.CreateCharResponseDto { Code = -2 };

            await defaultServer.SendMessage((int)ChannelRecvCode.CreateCharacter, request);

            return await tcs.Task;
        }
        public void HandleCreateCharacterResponse(CreateCharResponseDto r)
        {
            if (createCharRequests.TryRemove(r.AccountId, out var res))
            {
                res.SetResult(r);
            }
        }
        #endregion
    }
}
