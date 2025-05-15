using Application.Core.Net;
using Application.Core.Servers;
using Application.Core.ServerTransports;
using Application.Shared.Login;
using Application.Shared.Servers;
using DotNetty.Codecs;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using net.netty;
using net.packet;
using net.packet.logging;
using net.server;
using net.server.coordinator.session;
using tools;

namespace Application.Core.Client
{
    public abstract class SocketClient : ChannelHandlerAdapter, ISocketClient
    {
        public long SessionId { get; }
        public IServerBase<IServerTransport> CurrentServer { get; protected set; }
        public IChannel NettyChannel { get; protected set; }
        public string RemoteAddress { get; }
        public Hwid? Hwid { get; set; }

        protected System.Threading.Channels.Channel<Packet> packetChannel;
        protected ILogger<IClientBase> log;
        public DateTimeOffset LastPacket { get; protected set; }
        public DateTimeOffset LastPong { get; protected set; }
        public abstract bool IsOnlined { get; }
        public bool IsActive { get; protected set; }

        protected string _clientInfo;
        protected SocketClient(long sessionId, IServerBase<IServerTransport> currentServer, IChannel nettyChannel, ILogger<IClientBase> log)
        {
            SessionId = sessionId;
            CurrentServer = currentServer;
            NettyChannel = nettyChannel;
            RemoteAddress = NettyChannel.RemoteAddress.GetIPAddressString();

            _clientInfo = $"{SessionId}_{RemoteAddress}";

            packetChannel = System.Threading.Channels.Channel.CreateUnbounded<Packet>();
            Task.Run(async () =>
            {
                await foreach (var p in packetChannel.Reader.ReadAllAsync())
                {
                    await NettyChannel.WriteAndFlushAsync(p);
                }
            });
            this.log = log;
        }

        public override string ToString()
        {
            return _clientInfo;
        }

        object lockObj = new object();
        private Semaphore actionsSemaphore = new Semaphore(7, 7);
        public void lockClient()
        {
            Monitor.Enter(lockObj);
        }
        public void unlockClient()
        {
            Monitor.Exit(lockObj);
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
            if (!Server.getInstance().IsOnline)
            {
                await channel.CloseAsync();
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

        protected abstract void ProcessPacket(InPacket packet);
        public override void ChannelRead(IChannelHandlerContext ctx, object msg)
        {
            if (msg is not InPacket packet)
            {
                log.LogWarning("Received invalid message: {Packet}", msg);
                return;
            }

            ProcessPacket(packet);
            LastPacket = DateTimeOffset.Now;
        }

        public override void UserEventTriggered(IChannelHandlerContext ctx, object evt)
        {
            if (evt is IdleStateEvent idleEvent)
            {
                CheckIfIdle(idleEvent);
            }
        }

        private void CheckIfIdle(IdleStateEvent evt)
        {
            var pingedAt = DateTimeOffset.Now;
            sendPacket(PacketCreator.getPing());
            Task.Delay(15000).ContinueWith(_ =>
            {
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
            });
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
                    sendPacket(PacketCreator.serverNotice(1, cause.Message));
            }
        }

        protected abstract void CloseSessionInternal();

        private void CloseSession()
        {
           
            try
            {
                CloseSessionInternal();
            }
            catch (Exception t)
            {
                log.LogWarning(t, "Account stuck");
            }
            finally
            {
                CloseSocket();
            }
        }

        public abstract void ForceDisconnect();

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

        public void CloseSocket()
        {
            NettyChannel.CloseAsync().Wait();
        }

        public abstract void Dispose();

        public void PongReceived()
        {
            LastPong = DateTimeOffset.Now;
        }
    }
}
