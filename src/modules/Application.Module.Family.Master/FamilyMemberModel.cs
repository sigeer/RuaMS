using Application.Module.Family.Common;

namespace Application.Module.Family.Master
{
    public class FamilyMemberModel
    {
        public int Cid { get; set; }

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
