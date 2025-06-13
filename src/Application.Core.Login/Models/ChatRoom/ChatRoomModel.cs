namespace Application.Core.Login.Models.ChatRoom
{
    public class ChatRoomModel
    {
        const int MaxCount = 3;
        public int Id { get; set; }
        public ChatRoomModel(int id)
        {
            Id = id;

            _freeSlots = new Stack<int>(MaxCount);
            for (int i = 2; i >= 0; i--)
                _freeSlots.Push(i);

            Members = new int[MaxCount];
        }
        public int[] Members { get; }
        private readonly Stack<int> _freeSlots;

        public bool TryAddMember(int memberId, out int position)
        {
            position = 4;
            if (_freeSlots.TryPop(out var index))
            {
                position = index + 1;
                Members[index] = memberId;
                return true;
            }
            return false;
        }

        public bool TryRemoveMember(int memberId, out int position)
        {
            var idx = Array.IndexOf(Members, memberId);
            position = idx + 1;
            if (idx > -1)
            {
                Members[idx] = -1;
                _freeSlots.Push(idx);
                return true;
            }
            return false;
        }
    }
}
