using Application.Shared.Net;

namespace Application.Shared.MapObjects.Players
{
    public interface IFieldPlayer
    {
        int Id { get; }
        Task EnableAction();
        Task SendPacket(Packet p);
    }
}
