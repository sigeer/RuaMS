using Application.Shared.Invitations;
using System.Collections.Concurrent;

namespace Application.Core.Game.Invites
{
    public class InviteType : EnumClass, IDisposable
    {
        const long Expired = 3 * 60 * 1000;
        //BUDDY, (not needed)
        public static InviteType TRADE = new InviteType(InviteTypeEnum.TRADE);
        public InviteTypeEnum Value { get; }
        private InviteType(InviteTypeEnum enmuValue)
        {
            Value = enmuValue;
        }
        /// <summary>
        /// Key: 受邀者
        /// </summary>
        protected ConcurrentDictionary<int, LocalInviteRequest> _data = new ConcurrentDictionary<int, LocalInviteRequest>();

        public bool HasRequest(int targetCId) => _data.ContainsKey(targetCId);

        public void RemoveRequest(int targetCId)
        {
            _data.TryRemove(targetCId, out var d);
        }

        public void CheckExpired(long now)
        {
            foreach (var kv in _data)
            {
                if (kv.Value.CreationTime < now - Expired)
                {
                    _data.TryRemove(kv.Key, out _);
                }
            }
        }

        public bool CreateInvite(LocalInviteRequest request)
        {
            if (!_data.TryAdd(request.To.Id, request))
                return false;

            return true;
        }

        /// <summary>
        /// 回复邀请
        /// </summary>
        /// <param name="type">邀请类型</param>
        /// <param name="responseId">回复请求者（受邀者）</param>
        /// <param name="answer">同意/拒绝</param>
        /// <returns></returns>
        public LocalInviteResult AnswerInvite(int responseId, int checkKey, bool answer)
        {
            InviteResultType result = InviteResultType.NOT_FOUND;

            if (_data.TryRemove(responseId, out var d) && (checkKey == -1 || checkKey == d.From.Id))
            {
                result = answer ? InviteResultType.ACCEPTED : InviteResultType.DENIED;
                return new LocalInviteResult(result, d);
            }

            return new LocalInviteResult(result, null);
        }

        public void Dispose()
        {
            _data.Clear();
        }
    }
}
