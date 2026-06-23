using Application.Core.Channel;
using Application.Core.Client;
using Application.Core.Game.Players;
using Application.Shared.Models;
using Application.Shared.Net;
using Application.Shared.Servers;
using DotNetty.Transport.Channels;
using scripting.npc;

namespace Application.Plugin.FakeCharacter
{
    internal class FakeClient : IChannelClient
    {
        public FakeClient(WorldChannel worldChannel) { CurrentServer = worldChannel; }
        public WorldChannel CurrentServer { get; }

        public int Channel => CurrentServer.Id;

        public Player? Character { get; private set; }

        public Player OnlinedCharacter => Character!;

        public ClientCulture CurrentCulture { get; set; } = ClientCulture.SystemCulture;
        public NPCConversationManager? NPCConversationManager { get; set; }

        public bool IsOnlined => true;

        public bool IsActive => true;

        public bool IsServerTransition => false;

        public AccountCtrl? AccountEntity { get; set; } = new AccountCtrl
        {
            Id = 0,
            Name = "FakeAccount",
            Characterslots = 3,
            GMLevel = 0,
            Birthday = DateTime.MinValue,
        };

        public int AccountId => 0;

        public string AccountName => "FakeAccount";

        public int AccountGMLevel => 0;

        public int Language { get; set; }

        public ISocketServer CurrentServerBase => CurrentServer;

        public long SessionId => 0;

        public IChannel NettyChannel => null!;

        public Hwid? Hwid { get; set; }

        public string RemoteAddress => "0.0.0.0";

        public DateTimeOffset LastPacket => DateTimeOffset.UtcNow;

        public Task ChangeChannel(int channel)
        {
            return Task.CompletedTask;
        }

        public bool CheckBirthday(DateTime date)
        {
            return true;
        }

        public bool CheckBirthday(int date)
        {
            return true;
        }

        public void CloseSession()
        {
        }

        public Task CloseSocket()
        {
            return Task.CompletedTask;
        }

        public Task Disconnect(bool isShutdown, bool fromCashShop = false)
        {
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public Task ForceDisconnect()
        {
            return Task.CompletedTask;
        }

        public bool GainCharacterSlot()
        {
            return true;
        }

        public int getChannel()
        {
            return Channel;
        }

        public WorldChannel getChannelServer() => CurrentServer;

        public string GetSessionRemoteHost()
        {
            return "0.0.0.0";
        }

        public Task LeaveCashShop()
        {
            return Task.CompletedTask;
        }

        public void PongReceived()
        {
        }

        public Task ProcessPacket(InPacket packet)
        {
            return Task.CompletedTask;
        }

        public void releaseClient()
        {
        }

        /// <summary>
        /// 假人没有真实客户端，不需要向自身发送数据包
        /// </summary>
        public Task SendPacket(Packet p)
        {
            return Task.CompletedTask;
        }

        public void SetAccount(AccountCtrl accountEntity)
        {
            AccountEntity = accountEntity;
        }

        public void SetCharacterOnSessionTransitionState(int cid)
        {
        }

        public void SetPlayer(Player? player)
        {
            Character = player;
        }

        public Task tryacquireClient()
        {
            return Task.CompletedTask;
        }
    }
}
