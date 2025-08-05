using Application.Core.Login.Shared;
using Application.Module.Family.Common;
using System.Collections.Concurrent;

namespace Application.Module.Family.Master.Models
{
    public class FamilyModel: ITrackableEntityKey<int>
    {
        public FamilyModel(int id)
        {
            Id = id;
        }

        public int Id { get; set; }
        public ConcurrentDictionary<int, FamilyMemberModel> Members { get; set; } = [];
    }

    public class FamilyCharacterModel : ITrackableEntityKey<int>
    {
        public int Id { get; set; }

        public int Familyid { get; set; }

        public int Seniorid { get; set; }

        public int Reputation { get; set; }

        public int Todaysrep { get; set; }

        public int Totalreputation { get; set; }

        public int Reptosenior { get; set; }

        public string? Precepts { get; set; }

        public long Lastresettime { get; set; }

        public List<FamilyEntitlementUseRecord> EntitlementUseRecord { get; set; } = [];

        public bool RemoveRecord(int entitlementId)
        {
            var idx = EntitlementUseRecord.FindLastIndex(x => x.Id == entitlementId);
            EntitlementUseRecord.RemoveAt(idx);
            return idx > -1;
        }

    }
}
