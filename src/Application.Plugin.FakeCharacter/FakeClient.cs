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
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Plugin.FakeCharacter
{
    internal class FakeClient : IChannelClient
    {
        public WorldChannel CurrentServer => throw new NotImplementedException();

        public int Channel => throw new NotImplementedException();

        public Player? Character => throw new NotImplementedException();

        public Player OnlinedCharacter => throw new NotImplementedException();

        public ClientCulture CurrentCulture { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public NPCConversationManager? NPCConversationManager { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsOnlined => false;

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

        public void announceBossHpBar(Monster mm, int mobHash, Packet packet)
        {
            throw new NotImplementedException();
        }

        public void announceHint(string msg, int length)
        {
            throw new NotImplementedException();
        }

        public void announceServerMessage()
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

        public void ChangeChannel(int channel)
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

        public void closePlayerScriptInteractions()
        {
            throw new NotImplementedException();
        }

        public void CloseSession()
        {
            throw new NotImplementedException();
        }

        public void CloseSocket()
        {
            throw new NotImplementedException();
        }

        public Task Disconnect(bool isShutdown, bool fromCashShop = false)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void enableCSActions()
        {
            throw new NotImplementedException();
        }

        public void ForceDisconnect()
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

        public WorldChannel getChannelServer()
        {
            throw new NotImplementedException();
        }

        public AbstractEventManager? getEventManager(string @event)
        {
            throw new NotImplementedException();
        }

        public string GetSessionRemoteHost()
        {
            throw new NotImplementedException();
        }

        public void LeaveCashShop()
        {
            throw new NotImplementedException();
        }

        public void lockClient()
        {
            throw new NotImplementedException();
        }

        public void OpenNpc(int npcid, string? script = null)
        {
            throw new NotImplementedException();
        }

        public void PongReceived()
        {
            throw new NotImplementedException();
        }

        public void ProcessPacket(InPacket packet)
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

        public void sendPacket(Packet packet)
        {
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
            throw new NotImplementedException();
        }

        public bool tryacquireClient()
        {
            throw new NotImplementedException();
        }

        public void unlockClient()
        {
            throw new NotImplementedException();
        }
    }
}
