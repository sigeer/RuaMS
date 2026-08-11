using Application.Core.Channel.Net.Packets;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public Task PortalSound() => SendPacket(EffectPacket.Portal());
    }
}
