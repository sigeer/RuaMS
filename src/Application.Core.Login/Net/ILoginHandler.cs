using Application.Core.Login.Client;

namespace Application.Core.Login.Net
{
    public interface ILoginHandler : IPacketHandlerBase<ILoginClient>
    {
    }
}
