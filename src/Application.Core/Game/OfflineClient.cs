using Application.Core.Game.Life;
using Application.Core.Channel;
using Application.Core.Scripting.Infrastructure;
using Application.Shared.Servers;
using DotNetty.Transport.Channels;
using scripting;
using scripting.Event;
using scripting.npc;

namespace Application.Core.Game
{
    public class OfflineClient : IChannelClient
    {
        public WorldChannel CurrentServer => throw new BusinessCharacterOfflineException();

        public int Channel => throw new BusinessCharacterOfflineException();

        public int ActualChannel => throw new BusinessCharacterOfflineException();

        public IPlayer? Character => throw new BusinessCharacterOfflineException();

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

        public AccountCtrl AccountEntity { get; set; }


        public IServerBase<IServerTransport> CurrentServerBase => throw new NotImplementedException();

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

        public World getWorldServer()
        {
            throw new BusinessCharacterOfflineException();
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
    }
}
