using System.Collections.Concurrent;

namespace Application.Module.Family.Master
{
    public class FamilyModel
    {
        public FamilyModel(int id)
        {
            Id = id;
        }

        public int Id { get; set; }
        public ConcurrentDictionary<int, FamilyMemberModel> Members { get; set; } = [];
    }
}
