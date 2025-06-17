namespace Application.Module.Family.Common
{
    public class FamilyEntitlementUseRecord
    {
        public FamilyEntitlementUseRecord(int id)
        {
            Id = id;
            Time = DateTimeOffset.UtcNow;
        }

        public FamilyEntitlementUseRecord(FamilyEntitlement data) : this(data.ordinal())
        {
        }

        public int Id { get; set; }
        public DateTimeOffset Time { get; set; }
    }
}
