namespace Application.Core.Gameplay
{
    public interface IGameEvent
    {
        void RunDisabledServerMessagesSchedule();
        void RunHiredMerchantSchedule();
        void RunMountSchedule();
        void RunPartySearchUpdateSchedule();
        void RunPetSchedule();
        void RunPlayerHpDecreaseSchedule();
        void RunTimedMapObjectSchedule();
        void RunCheckOwnedMapsSchedule();
    }
}
