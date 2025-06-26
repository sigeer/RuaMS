namespace Application.Core.Channel.Services
{
    public interface IDueyService
    {
        void DueyTalk(IChannelClient c, bool quickDelivery);
    }

    public class DefaultDueyService : IDueyService
    {
        public void DueyTalk(IChannelClient c, bool quickDelivery)
        {
            throw new BusinessNotsupportException();
        }
    }
}
