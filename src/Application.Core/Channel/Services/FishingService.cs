namespace Application.Core.Channel.Services
{
    public interface IFishingService
    {
        bool AttemptCatchFish(Player chr, int baitLevel);
        void StopFishing(Player chr);
    }
    public class DefaultFishingService : IFishingService
    {
        public bool AttemptCatchFish(Player chr, int baitLevel)
        {
            return false;
        }

        public void StopFishing(Player chr)
        {

        }
    }
}
