using Application.Shared.Models;
using Application.Shared.Net;
using Application.Shared.Servers;
using Application.Utility.Exceptions;
using Application.Utility.Extensions;
using DotNetty.Codecs;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;

namespace Application.Shared.Client
{
    public abstract class SocketClient : SimpleChannelInboundHandler<InPacket>, ISocketClient
    {
        public virtual ISocketServer CurrentServerBase { get; protected set; }
        public long SessionId { get; }

        public IChannel NettyChannel { get; protected set; }
        public string RemoteAddress { get; }
        public Hwid? Hwid { get; set; }

        protected ILogger<ISocketClient> log;
        public DateTimeOffset LastPacket { get; protected set; }
        public DateTimeOffset LastPong { get; protected set; }
        public abstract bool IsOnlined { get; }
        public bool IsActive { get; protected set; }

        protected string _clientInfo;
        protected SocketClient(long sessionId, IChannel nettyChannel, ISocketServer server, ILogger<ISocketClient> log)
        {
            SessionId = sessionId;
            NettyChannel = nettyChannel;
            RemoteAddress = NettyChannel.RemoteAddress.GetIPAddressString();
            CurrentServerBase = server;

            _clientInfo = $"{SessionId}_{RemoteAddress}";

            this.log = log;
        }

        public override string ToString()
        {
            return _clientInfo;
        }

        private SemaphoreSlim actionsSemaphore = new (7, 7);

        public Task tryacquireClient()
        {
            return actionsSemaphore.WaitAsync();
        }

        public void releaseClient()
        {
            actionsSemaphore.Release();
        }

        public async Task SendPacket(Packet p)
        {
            await NettyChannel.WriteAndFlushAsync(p);
        }
        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            var channel = ctx.Channel;
            if (!CurrentServerBase.IsRunning)
            {
                channel.CloseAsync();
                return;
            }

            IsActive = true;
            this.NettyChannel = channel;
        }

        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            CloseSession();
            IsActive = false;
        }

        public abstract Task ProcessPacket(InPacket packet);
        protected override void ChannelRead0(IChannelHandlerContext ctx, InPacket msg)
        {
            LastPacket = DateTimeOffset.UtcNow;
        }

        public override void UserEventTriggered(IChannelHandlerContext ctx, object evt)
        {
#if !DEBUG
            if (evt is IdleStateEvent idleEvent)
            {
                _ = CheckIfIdle(idleEvent);
            }
#endif
        }

        private async Task CheckIfIdle(IdleStateEvent evt)
        {
            var pingedAt = DateTimeOffset.UtcNow;
            await SendPacket(PacketCommon.getPing());
            await Task.Delay(15000);
            try
            {
                if (LastPong < pingedAt)
                {
                    if (NettyChannel.Active)
                    {
                        log.LogInformation("Disconnected {IP} due to idling. Reason: {State}", RemoteAddress, evt.State);

                        CloseSession();
                    }
                }
            }
            catch (NullReferenceException e)
            {
                log.LogError(e.ToString());
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception cause)
        {
            if (cause is not DecoderException)
                log.LogError(cause, "Exception caught by {ClientInfo}", this);

            if (cause is InvalidPacketHeaderException)
            {
                CloseSession();
            }
            else if (cause is IOException)
            {
                CloseSession();
            }
            else if (cause is BusinessException)
            {
                if (cause is BusinessFatalException)
                    CloseSession();
                else
                    _ = SendPacket(PacketCommon.serverNotice(5, cause.Message));
            }
        }

        protected abstract Task CloseSessionInternal();

        public void CloseSession()
        {
            try
            {
                _ = CloseSessionInternal();
            }
            catch (Exception t)
            {
                log.LogWarning(t, "Account stuck");
            }
            finally
            {
                _ = CloseSocket();
            }
        }

        public abstract Task ForceDisconnect();

        public string GetSessionRemoteHost()
        {
            if (Hwid != null)
            {
                return $"{RemoteAddress}-{Hwid.hwid}";
            }
            else
            {
                return RemoteAddress;
            }
        }

        public Task CloseSocket()
        {
            return NettyChannel.CloseAsync();
        }

        public virtual async ValueTask DisposeAsync()
        {
            await CloseSocket();
        }

        public void PongReceived()
        {
            LastPong = DateTimeOffset.UtcNow;
        }
    }
}
