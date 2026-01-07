using Application.Shared.Models;
using Application.Shared.Net;
using Application.Shared.Servers;
using Application.Utility.Configs;
using Application.Utility.Exceptions;
using Application.Utility.Extensions;
using DotNetty.Codecs;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Application.Shared.Client
{
    public abstract class SocketClient : SimpleChannelInboundHandler<InPacket>, ISocketClient
    {
        public virtual ISocketServer CurrentServerBase { get; protected set; }
        public long SessionId { get; }

        public IChannel NettyChannel { get; protected set; }
        public string RemoteAddress { get; }
        public Hwid? Hwid { get; set; }

        protected System.Threading.Channels.Channel<Packet> packetChannel;
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
            packetChannel = System.Threading.Channels.Channel.CreateUnbounded<Packet>();
            _ = Task.Run(async () =>
            {
                var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(150));

                while (await timer.WaitForNextTickAsync())
                {
                    if (packetChannel.Reader.Count == 0)
                        continue;

                    NettyChannel.EventLoop.Execute(() =>
                    {
                        while (packetChannel.Reader.TryRead(out var p))
                        {
                            NettyChannel.WriteAsync(p);
                        }

                        NettyChannel.Flush();
                    });
                }
            });
        }

        public override string ToString()
        {
            return _clientInfo;
        }

        Lock lockObj = new ();
        private Semaphore actionsSemaphore = new Semaphore(7, 7);
        public void lockClient()
        {
            lockObj.Enter();
        }
        public void unlockClient()
        {
            lockObj.Exit();
        }

        public bool tryacquireClient()
        {
            if (actionsSemaphore.WaitOne())
            {
                lockClient();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void releaseClient()
        {
            unlockClient();
            actionsSemaphore.Release();
        }

        public void sendPacket(Packet packet)
        {
            if (!packetChannel.Writer.TryWrite(packet))
                log.LogError("数据包写入失败");
        }

        public override async void ChannelActive(IChannelHandlerContext ctx)
        {
            var channel = ctx.Channel;
            if (!CurrentServerBase.IsRunning)
            {
                await channel.CloseAsync();
                return;
            }

            IsActive = true;
            this.NettyChannel = channel;
        }

        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            _ = CloseSession();
            IsActive = false;
        }

        protected abstract void ProcessPacket(InPacket packet);
        protected override void ChannelRead0(IChannelHandlerContext ctx, InPacket msg)
        {
            ProcessPacket(msg);
            LastPacket = DateTimeOffset.UtcNow;
        }

        public override void UserEventTriggered(IChannelHandlerContext ctx, object evt)
        {
#if !DEBUG
            if (evt is IdleStateEvent idleEvent)
            {
                CheckIfIdle(idleEvent);
            }
#endif
        }

        private void CheckIfIdle(IdleStateEvent evt)
        {
            var pingedAt = DateTimeOffset.UtcNow;
            sendPacket(PacketCommon.getPing());
            Task.Delay(15000).ContinueWith(async _ =>
            {
                try
                {
                    if (LastPong < pingedAt)
                    {
                        if (NettyChannel.Active)
                        {
                            log.LogInformation("Disconnected {IP} due to idling. Reason: {State}", RemoteAddress, evt.State);

                            await CloseSession();
                        }
                    }
                }
                catch (NullReferenceException e)
                {
                    log.LogError(e.ToString());
                }
            });
        }

        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception cause)
        {
            if (cause is not DecoderException)
                log.LogError(cause, "Exception caught by {ClientInfo}", this);

            if (cause is InvalidPacketHeaderException)
            {
                _ = CloseSession();
            }
            else if (cause is IOException)
            {
                _ = CloseSession();
            }
            else if (cause is BusinessException)
            {
                if (cause is BusinessFatalException)
                    _ = CloseSession();
                else
                    sendPacket(PacketCommon.serverNotice(5, cause.Message));
            }
        }

        protected abstract Task CloseSessionInternal();

        public async Task CloseSession()
        {
            try
            {
                await CloseSessionInternal();
            }
            catch (Exception t)
            {
                log.LogWarning(t, "Account stuck");
            }
            finally
            {
                await CloseSocket();
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

        public async Task CloseSocket()
        {
            await NettyChannel.CloseAsync();
        }

        public virtual void Dispose()
        {
            this.packetChannel.Writer.TryComplete();
        }

        public void PongReceived()
        {
            LastPong = DateTimeOffset.UtcNow;
        }
    }
}
