using Application.Core.Channel;
using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.Game.Players;
using Application.Core.Scripting.Events;
using Application.Shared.Models;
using Application.Shared.Net;
using Application.Shared.Servers;
using DotNetty.Transport.Channels;
using scripting;
using scripting.npc;
using System.Numerics;

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
        public NPCConversationManager? NPCConversationManager { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsOnlined => true;

        public bool IsActive => throw new NotImplementedException();

        public bool IsServerTransition => throw new NotImplementedException();

        public AccountCtrl? AccountEntity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int AccountId => throw new NotImplementedException();

        public string AccountName => throw new NotImplementedException();

        public int AccountGMLevel => throw new NotImplementedException();

        public int Language { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ISocketServer CurrentServerBase => throw new NotImplementedException();

        public long SessionId => throw new NotImplementedException();

        public IChannel NettyChannel => throw new NotImplementedException();

        public Hwid? Hwid { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string RemoteAddress => throw new NotImplementedException();

        public DateTimeOffset LastPacket => throw new NotImplementedException();

        public Task announceBossHpBar(Monster mm, int mobHash, Packet packet)
        {
            throw new NotImplementedException();
        }

        public Task announceHint(string msg, int length)
        {
            throw new NotImplementedException();
        }

        public Task announceServerMessage()
        {
            throw new NotImplementedException();
        }

        public bool attemptCsCoupon()
        {
            throw new NotImplementedException();
        }

        public bool canClickNPC()
        {
            throw new NotImplementedException();
        }

        public Task ChangeChannel(int channel)
        {
            throw new NotImplementedException();
        }

        public bool CheckBirthday(DateTime date)
        {
            throw new NotImplementedException();
        }

        public bool CheckBirthday(int date)
        {
            throw new NotImplementedException();
        }

        public Task closePlayerScriptInteractions()
        {
            throw new NotImplementedException();
        }

        public void CloseSession()
        {
            throw new NotImplementedException();
        }

        public Task CloseSocket()
        {
            throw new NotImplementedException();
        }

        public Task Disconnect(bool isShutdown, bool fromCashShop = false)
        {
            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        public Task enableCSActions()
        {
            throw new NotImplementedException();
        }

        public Task ForceDisconnect()
        {
            throw new NotImplementedException();
        }

        public bool GainCharacterSlot()
        {
            throw new NotImplementedException();
        }

        public AbstractPlayerInteraction getAbstractPlayerInteraction()
        {
            throw new NotImplementedException();
        }

        public int getChannel()
        {
            throw new NotImplementedException();
        }

        public WorldChannel getChannelServer() => CurrentServer;

        public AbstractEventManager? getEventManager(string @event)
        {
            throw new NotImplementedException();
        }

        public string GetSessionRemoteHost()
        {
            throw new NotImplementedException();
        }

        public Task LeaveCashShop()
        {
            throw new NotImplementedException();
        }

        public void PongReceived()
        {
            throw new NotImplementedException();
        }

        public Task ProcessPacket(InPacket packet)
        {
            throw new NotImplementedException();
        }

        public void releaseClient()
        {
            throw new NotImplementedException();
        }

        public void removeClickedNPC()
        {
            throw new NotImplementedException();
        }

        public void resetCsCoupon()
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void SetCharacterOnSessionTransitionState(int cid)
        {
            throw new NotImplementedException();
        }

        public void setClickedNPC()
        {
            throw new NotImplementedException();
        }

        public void SetPlayer(Player? player)
        {
            Character = player;
        }

        public Task tryacquireClient()
        {
            throw new NotImplementedException();
        }
    }
}
