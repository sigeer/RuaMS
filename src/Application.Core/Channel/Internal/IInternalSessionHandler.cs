using Google.Protobuf;

namespace Application.Core.Channel.Internal
{
    public interface IInternalSessionHandler
    {
        int MessageId { get; }
        Task Handle(ByteString message, CancellationToken cancellationToken = default);
    }
}
