using Application.Shared.Invitations;
using Application.Utility;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Collections.Concurrent;

namespace Application.Core.Login.Models.Invitations
{
    public class InviteType : EnumClass, IDisposable
    {
        const long Expired = 3 * 60 * 1000;
        //BUDDY, (not needed)
        public static InviteType FAMILY = new InviteType(InviteTypeEnum.FAMILY);
        public static InviteType FAMILY_SUMMON = new InviteType(InviteTypeEnum.FAMILY_SUMMON);
        public static InviteType MESSENGER = new InviteType(InviteTypeEnum.MESSENGER);
        public static InviteType TRADE = new InviteType(InviteTypeEnum.TRADE);
        public static InviteType PARTY = new InviteType(InviteTypeEnum.PARTY);
        public static InviteType GUILD = new InviteType(InviteTypeEnum.GUILD);
        public static InviteType ALLIANCE = new InviteType(InviteTypeEnum.ALLIANCE);
        public InviteTypeEnum Value { get; }
        private InviteType(InviteTypeEnum enmuValue)
        {
            Value = enmuValue;
        }
        /// <summary>
        /// Key: 受邀者
        /// </summary>
        protected ConcurrentDictionary<int, InviteRequest> _data = new ConcurrentDictionary<int, InviteRequest>();

        public bool HasRequest(int targetCId) => _data.ContainsKey(targetCId);

        public void RemoveRequest(int targetCId)
        {
            _data.TryRemove(targetCId, out var d);
        }

        public void CheckExpired(long now)
        {
            _data = new ConcurrentDictionary<int, InviteRequest>(
                _data.Where(kv => now - kv.Value.CreationTime > Expired)
            );
        }

        public bool CreateInvite(InviteRequest request)
        {
            if (!_data.TryAdd(request.ToPlayerId, request))
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
        public InviteResult AnswerInvite(int responseId, bool answer)
        {
            InviteResultType result = InviteResultType.NOT_FOUND;

            if (_data.TryRemove(responseId, out var d))
            {
                result = answer ? InviteResultType.ACCEPTED : InviteResultType.DENIED;
                return new InviteResult(result, d);
            }

            return new InviteResult(result, null);
        }

        public void Dispose()
        {
            _data.Clear();
        }
    }
}
