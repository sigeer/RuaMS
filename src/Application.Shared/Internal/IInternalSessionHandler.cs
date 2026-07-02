using Google.Protobuf;
using System.Threading.Tasks;

namespace Application.Shared.Internal
{
    public interface IInternalSessionHandler
    {
        int MessageId { get; }
        Task Handle(ByteString message);
    }
}
