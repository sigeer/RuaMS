using Application.Core.Channel;

namespace Application.Core.Game.Players
{
    public interface IClientPlayer : IClientMessenger
    {
        IChannelClient Client { get; }
        Task SendPacket(Packet packet);
    }
}
