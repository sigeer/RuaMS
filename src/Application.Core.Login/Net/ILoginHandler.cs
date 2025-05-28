using Application.Core.Login.Client;
using Application.Core.Net;

namespace Application.Core.Login.Net
{
    public interface ILoginHandler : IPacketHandlerBase<ILoginClient>
    {
    }
}
