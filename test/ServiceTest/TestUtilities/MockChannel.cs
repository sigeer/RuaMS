using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServiceTest.TestUtilities
{
    public class MockChannel : IChannel
    {
        public IChannelId Id => throw new NotImplementedException();

        public IByteBufferAllocator Allocator => throw new NotImplementedException();

        public IEventLoop EventLoop => throw new NotImplementedException();

        public IChannel Parent => throw new NotImplementedException();

        public bool Open => throw new NotImplementedException();

        public bool Active => throw new NotImplementedException();

        public bool Registered => throw new NotImplementedException();

        public ChannelMetadata Metadata => throw new NotImplementedException();

        public EndPoint LocalAddress => throw new NotImplementedException();

        public EndPoint RemoteAddress => new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999);

        public bool IsWritable => throw new NotImplementedException();

        public IChannelUnsafe Unsafe => throw new NotImplementedException();

        public IChannelPipeline Pipeline => throw new NotImplementedException();

        public IChannelConfiguration Configuration => throw new NotImplementedException();

        public Task CloseCompletion => throw new NotImplementedException();

        public Task BindAsync(EndPoint localAddress)
        {
            throw new NotImplementedException();
        }

        public Task CloseAsync()
        {
            throw new NotImplementedException();
        }

        public int CompareTo(IChannel? other)
        {
            throw new NotImplementedException();
        }

        public Task ConnectAsync(EndPoint remoteAddress)
        {
            throw new NotImplementedException();
        }

        public Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress)
        {
            throw new NotImplementedException();
        }

        public Task DeregisterAsync()
        {
            throw new NotImplementedException();
        }

        public Task DisconnectAsync()
        {
            throw new NotImplementedException();
        }

        public IChannel Flush()
        {
            throw new NotImplementedException();
        }

        public IAttribute<T> GetAttribute<T>(AttributeKey<T> key) where T : class
        {
            throw new NotImplementedException();
        }

        public bool HasAttribute<T>(AttributeKey<T> key) where T : class
        {
            throw new NotImplementedException();
        }

        public IChannel Read()
        {
            throw new NotImplementedException();
        }

        public Task WriteAndFlushAsync(object message)
        {
            return Task.CompletedTask;
            // throw new NotImplementedException();
        }

        public Task WriteAsync(object message)
        {
            return Task.CompletedTask;
            throw new NotImplementedException();
        }
    }
}
