using Application.Module.Family.Common;

namespace Application.Module.Family.Master.Models
{
    public class FamilyEntitlementUseRecord
    {
        public FamilyEntitlementUseRecord(int id)
        {
            Id = id;
        }

        public FamilyEntitlementUseRecord(FamilyEntitlement data) : this(data.Value)
        {
        }

        public int Id { get; set; }
        public long Time { get; set; }
    }
}
