using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Scripting.Infrastructure;
using Application.Shared.Servers;
using DotNetty.Transport.Channels;
using scripting;
using scripting.Event;
using scripting.npc;
using System.Globalization;

namespace Application.Core.Game
{
    public class OfflineClient : IChannelClient
    {
        public WorldChannel CurrentServer => throw new BusinessCharacterOfflineException();

        public int Channel => 0;

        public int ActualChannel => 0;

        public IPlayer? Character => null;

        public IPlayer OnlinedCharacter => throw new BusinessCharacterOfflineException();

        public bool IsServerTransition => throw new BusinessCharacterOfflineException();

        public NPCConversationManager? NPCConversationManager { get => throw new BusinessCharacterOfflineException(); set => throw new BusinessCharacterOfflineException(); }
        public EngineStorage ScriptEngines { get => throw new BusinessCharacterOfflineException(); set => throw new BusinessCharacterOfflineException(); }

        public bool IsOnlined => false;

        public bool IsActive => throw new BusinessCharacterOfflineException();


        public long SessionId => throw new BusinessCharacterOfflineException();

        public IChannel NettyChannel => throw new BusinessCharacterOfflineException();

        public Hwid? Hwid { get => throw new BusinessCharacterOfflineException(); set => throw new BusinessCharacterOfflineException(); }

        public string RemoteAddress => throw new BusinessCharacterOfflineException();

        public DateTimeOffset LastPacket => throw new BusinessCharacterOfflineException();

        public int AccountId => throw new BusinessCharacterOfflineException();

        public string AccountName => throw new BusinessCharacterOfflineException();

        public int AccountGMLevel => throw new BusinessCharacterOfflineException();

        public AccountCtrl? AccountEntity { get; set; }


        public IServerBase<IServerTransport> CurrentServerBase => throw new NotImplementedException();

        ISocketServer ISocketClient.CurrentServerBase => throw new NotImplementedException();

        WorldChannel IChannelClient.CurrentServer => throw new NotImplementedException();

        int IChannelClient.Channel => throw new NotImplementedException();

        IPlayer? IChannelClient.Character => throw new NotImplementedException();

        IPlayer IChannelClient.OnlinedCharacter => throw new NotImplementedException();

        NPCConversationManager? IChannelClient.NPCConversationManager { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        EngineStorage IChannelClient.ScriptEngines { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        bool IClientBase.IsOnlined => throw new NotImplementedException();

        bool IClientBase.IsActive => throw new NotImplementedException();

        bool IClientBase.IsServerTransition => throw new NotImplementedException();

        AccountCtrl? IClientBase.AccountEntity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        int IClientBase.AccountId => throw new NotImplementedException();

        string IClientBase.AccountName => throw new NotImplementedException();

        int IClientBase.AccountGMLevel => throw new NotImplementedException();

        int IClientBase.Language { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        CultureInfo CurrentCulture { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        ClientCulture IChannelClient.CurrentCulture { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        long ISocketClient.SessionId => throw new NotImplementedException();

        IChannel ISocketClient.NettyChannel => throw new NotImplementedException();

        Hwid? ISocketClient.Hwid { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        string ISocketClient.RemoteAddress => throw new NotImplementedException();

        DateTimeOffset ISocketClient.LastPacket => throw new NotImplementedException();

        public void announceBossHpBar(Monster mm, int mobHash, Packet packet)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void announceHint(string msg, int length)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void announceServerMessage()
        {
            throw new BusinessCharacterOfflineException();
        }

        public bool attemptCsCoupon()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void BanMacs()
        {
            throw new BusinessCharacterOfflineException();
        }

        public bool canClickNPC()
        {
            throw new BusinessCharacterOfflineException();
        }

        public bool CanGainCharacterSlot()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void ChangeChannel(int channel)
        {
            throw new BusinessCharacterOfflineException();
        }

        public bool CheckBirthday(DateTime date)
        {
            throw new BusinessCharacterOfflineException();
        }

        public bool CheckBirthday(int date)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void closePlayerScriptInteractions()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void CloseSession()
        {
            throw new NotImplementedException();
        }

        public void CloseSocket()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void CommitAccount()
        {
            throw new NotImplementedException();
        }

        public void Disconnect(bool isShutdown, bool fromCashShop = false)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void Dispose()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void enableCSActions()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void ForceDisconnect()
        {
            throw new BusinessCharacterOfflineException();
        }

        public bool GainCharacterSlot()
        {
            throw new BusinessCharacterOfflineException();
        }

        public AbstractPlayerInteraction getAbstractPlayerInteraction()
        {
            throw new BusinessCharacterOfflineException();
        }

        public int GetAvailableCharacterSlots()
        {
            throw new BusinessCharacterOfflineException();
        }

        public int getChannel()
        {
            throw new BusinessCharacterOfflineException();
        }

        public WorldChannel getChannelServer()
        {
            throw new BusinessCharacterOfflineException();
        }

        public EventManager? getEventManager(string @event)
        {
            throw new BusinessCharacterOfflineException();
        }

        public string GetSessionRemoteHost()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void LeaveCashShop()
        {
            throw new NotImplementedException();
        }

        public void lockClient()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void OpenNpc(int npcid, string? script = null)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void PongReceived()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void releaseClient()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void removeClickedNPC()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void resetCsCoupon()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void sendPacket(Packet packet)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void SetAccount(AccountCtrl accountEntity)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void SetCharacterOnSessionTransitionState(int cid)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void setClickedNPC()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void SetPlayer(IPlayer? player)
        {
            throw new BusinessCharacterOfflineException();
        }

        public bool tryacquireClient()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void unlockClient()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void UpdateAccountChracterByAdd(int id)
        {
            throw new BusinessCharacterOfflineException();
        }

        void IChannelClient.announceBossHpBar(Monster mm, int mobHash, Packet packet)
        {
            throw new NotImplementedException();
        }

        void IChannelClient.announceHint(string msg, int length)
        {
            throw new NotImplementedException();
        }

        void IChannelClient.announceServerMessage()
        {
            throw new NotImplementedException();
        }

        bool IChannelClient.attemptCsCoupon()
        {
            throw new NotImplementedException();
        }

        bool IChannelClient.canClickNPC()
        {
            throw new NotImplementedException();
        }

        bool IChannelClient.CanGainCharacterSlot()
        {
            throw new NotImplementedException();
        }

        void IChannelClient.ChangeChannel(int channel)
        {
            throw new NotImplementedException();
        }

        bool IChannelClient.CheckBirthday(DateTime date)
        {
            throw new NotImplementedException();
        }

        bool IChannelClient.CheckBirthday(int date)
        {
            throw new NotImplementedException();
        }

        void IChannelClient.closePlayerScriptInteractions()
        {
            throw new NotImplementedException();
        }

        void ISocketClient.CloseSession()
        {
            throw new NotImplementedException();
        }

        void ISocketClient.CloseSocket()
        {
            throw new NotImplementedException();
        }

        void IChannelClient.Disconnect(bool isShutdown, bool fromCashShop)
        {
            throw new NotImplementedException();
        }

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }

        void IChannelClient.enableCSActions()
        {
            throw new NotImplementedException();
        }

        void ISocketClient.ForceDisconnect()
        {
            throw new NotImplementedException();
        }

        bool IChannelClient.GainCharacterSlot()
        {
            throw new NotImplementedException();
        }

        AbstractPlayerInteraction IChannelClient.getAbstractPlayerInteraction()
        {
            throw new NotImplementedException();
        }

        int IChannelClient.getChannel()
        {
            throw new NotImplementedException();
        }

        WorldChannel IChannelClient.getChannelServer()
        {
            throw new NotImplementedException();
        }

        EventManager? IChannelClient.getEventManager(string @event)
        {
            throw new NotImplementedException();
        }

        string ISocketClient.GetSessionRemoteHost()
        {
            throw new NotImplementedException();
        }

        void IChannelClient.LeaveCashShop()
        {
            throw new NotImplementedException();
        }

        void IChannelClient.lockClient()
        {
            throw new NotImplementedException();
        }

        void IChannelClient.OpenNpc(int npcid, string? script)
        {
            throw new NotImplementedException();
        }

        void ISocketClient.PongReceived()
        {
            throw new NotImplementedException();
        }

        void ISocketClient.releaseClient()
        {
            throw new NotImplementedException();
        }

        void IChannelClient.removeClickedNPC()
        {
            throw new NotImplementedException();
        }

        void IChannelClient.resetCsCoupon()
        {
            throw new NotImplementedException();
        }

        void ISocketClient.sendPacket(Packet packet)
        {
            throw new NotImplementedException();
        }

        void IChannelClient.SetAccount(AccountCtrl accountEntity)
        {
            throw new NotImplementedException();
        }

        void IClientBase.SetCharacterOnSessionTransitionState(int cid)
        {
            throw new NotImplementedException();
        }

        void IChannelClient.setClickedNPC()
        {
            throw new NotImplementedException();
        }

        void IChannelClient.SetPlayer(IPlayer? player)
        {
            throw new NotImplementedException();
        }

        bool ISocketClient.tryacquireClient()
        {
            throw new NotImplementedException();
        }

        void IChannelClient.unlockClient()
        {
            throw new NotImplementedException();
        }
    }
}
