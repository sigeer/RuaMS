using System.Collections.Concurrent;

namespace Application.Core.Game.Invites
{
    public class InviteType : EnumClass
    {
        const long Expired = 3 * 60 * 1000;
        //BUDDY, (not needed)
        public static InviteType FAMILY = new InviteType();
        public static InviteType FAMILY_SUMMON = new InviteType();
        public static InviteType MESSENGER = new InviteType();
        public static InviteType TRADE = new InviteType();
        public static InviteType PARTY = new InviteType();
        public static InviteType GUILD = new InviteType();
        public static InviteType ALLIANCE = new InviteType();
        private InviteType()
        {
        }
        /// <summary>
        /// Key: 受邀者
        /// </summary>
        protected ConcurrentDictionary<int, InviteRequest> _data = new ConcurrentDictionary<int, InviteRequest>();

        public bool HasRequest(int targetCId) => _data.ContainsKey(targetCId);

        protected InviteRequest? FindInviteRequest(int targetCId) => _data.GetValueOrDefault(targetCId);
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
            if (!_data.TryAdd(request.To.Id, request))
                return false;

            return true;
        }

        /// <summary>
        /// 回复邀请
        /// </summary>
        /// <param name="type">邀请类型</param>
        /// <param name="responseId">回复请求者（受邀者）</param>
        /// <param name="key">不同类型邀请值不同</param>
        /// <param name="answer">同意/拒绝</param>
        /// <returns></returns>
        public InviteResult AnswerInvite(int responseId, int key, bool answer)
        {
            InviteResultType result = InviteResultType.NOT_FOUND;

            var request = FindInviteRequest(responseId);

            if (request != null && CheckKey(request, key))
            {
                RemoveRequest(responseId);
                result = answer ? InviteResultType.ACCEPTED : InviteResultType.DENIED;
            }

            return new InviteResult(result, request);
        }

        bool CheckKey(InviteRequest request, int input)
        {
            if (request is TeamInviteRequest team)
                return team.TeamId == input;
            if (request is GuildInviteRequest guild)
                return guild.GuildId == input;
            if (request is AllianceInviteRequest alliance)
                return alliance.AllianceId == input;
            if (request is ChatInviteRequest chat)
                return chat.ChatRoomId == input;
            if (request is FamilySummonInviteRequest familySummon)
                return true;

            return input == request.From.Id;
        }
    }
}
