using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Scripting.Events;
using Application.Core.Scripting.Infrastructure;
using Application.Shared.Servers;
using DotNetty.Transport.Channels;
using scripting.npc;

namespace Application.Core.Game
{
    public class OfflineClient : IChannelClient
    {
        public WorldChannel CurrentServer => throw new BusinessCharacterOfflineException();
        public WorldChannelServer CurrentServerContainer => throw new BusinessCharacterOfflineException();
        public ISocketServer CurrentServerBase => throw new BusinessCharacterOfflineException();

        public int Channel => 0;

        public int ActualChannel => 0;

        public Player? Character => null;

        public Player OnlinedCharacter => throw new BusinessCharacterOfflineException();

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


        NPCConversationManager? IChannelClient.NPCConversationManager { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int Language { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ClientCulture CurrentCulture { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        IChannel ISocketClient.NettyChannel => throw new NotImplementedException();

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



        public void BanMacs()
        {
            throw new BusinessCharacterOfflineException();
        }



        public bool CanGainCharacterSlot()
        {
            throw new BusinessCharacterOfflineException();
        }

        public Task ChangeChannel(int channel)
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



        public void CloseSession()
        {
            throw new NotImplementedException();
        }

        public Task CloseSocket()
        {
            throw new BusinessCharacterOfflineException();
        }

        public void CommitAccount()
        {
            throw new NotImplementedException();
        }

        public Task Disconnect(bool isShutdown, bool fromCashShop = false)
        {
            throw new BusinessCharacterOfflineException();
        }

        public void Dispose()
        {
            throw new BusinessCharacterOfflineException();
        }



        public Task ForceDisconnect()
        {
            throw new BusinessCharacterOfflineException();
        }

        public bool GainCharacterSlot()
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

        public AbstractEventManager? getEventManager(string @event)
        {
            throw new BusinessCharacterOfflineException();
        }

        public string GetSessionRemoteHost()
        {
            throw new BusinessCharacterOfflineException();
        }

        public Task LeaveCashShop()
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



        public void SetPlayer(Player? player)
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











        bool IChannelClient.CheckBirthday(DateTime date)
        {
            throw new NotImplementedException();
        }

        bool IChannelClient.CheckBirthday(int date)
        {
            throw new NotImplementedException();
        }


        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }



        bool IChannelClient.GainCharacterSlot()
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



        string ISocketClient.GetSessionRemoteHost()
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





        public Task SendPacket(Packet p) => throw new NotImplementedException();

        void IChannelClient.SetAccount(AccountCtrl accountEntity)
        {
            throw new NotImplementedException();
        }

        void IClientBase.SetCharacterOnSessionTransitionState(int cid)
        {
            throw new NotImplementedException();
        }



        void IChannelClient.SetPlayer(Player? player)
        {
            throw new NotImplementedException();
        }

        Task ISocketClient.tryacquireClient()
        {
            throw new NotImplementedException();
        }


        public Task ProcessPacket(InPacket packet)
        {
            throw new NotImplementedException();
        }
    }
}
