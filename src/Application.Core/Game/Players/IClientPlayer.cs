using Application.Shared.MapObjects.Players;

namespace Application.Core.Game.Players
{
    public interface IClientPlayer : IClientMessenger
    {
        IChannelClient Client { get; }
        Task SendPacket(Packet packet);
    }
}
