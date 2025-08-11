namespace Application.Core.Channel.Services
{
    public interface IFishingService
    {
        bool AttemptCatchFish(IPlayer chr, int baitLevel);
        void StopFishing(IPlayer chr);
    }
    public class DefaultFishingService : IFishingService
    {
        public bool AttemptCatchFish(IPlayer chr, int baitLevel)
        {
            return false;
        }

        public void StopFishing(IPlayer chr)
        {

        }
    }
}
