namespace Application.Module.Marriage.Channel.Models
{
    public record WeddingInfo(int Channel, int MarriageId, bool IsCathedral, bool IsPremium, int GroomId, int BrideId, HashSet<int> Guests, long StartTime);
}
