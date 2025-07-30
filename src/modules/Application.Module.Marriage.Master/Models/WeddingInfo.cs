namespace Application.Module.Marriage.Master.Models
{
    public record WeddingInfo(int Id, int Channel, bool IsCathedral, bool IsPremium, int GroomId, int BrideId, HashSet<int> Guests, long StartTime);
}
