/*
This file is part of the OdinMS Maple Story Server
Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
Matthias Butz <matze@odinms.de>
Jan Christian Meyer <vimes@odinms.de>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation version 3 as published by
the Free Software Foundation. You may not use, modify or distribute
this program under any other version of the GNU Affero General Public
License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using Application.Core.Game.Life;
using Application.Core.Game.TheWorld;
using Application.Core.Scripting.Infrastructure;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using net.packet;
using net.server.coordinator.session;
using scripting;
using scripting.Event;
using scripting.npc;
using scripting.quest;

namespace Application.Core.Game
{
    public interface IClient
    {
        /// <summary>
        /// 客户端未登录：建立了连接~选择角色
        /// </summary>
        public IPlayer? Character { get; }
        public bool IsGameOnlined => Character != null;
        public IPlayer OnlinedCharacter => Character ?? throw new BusinessCharacterOfflineException();
        public string ClientInfo { get; }
        public int World { get; set; }
        public int Channel { get; set; }
        public NPCConversationManager? NPCConversationManager { get; set; }
        bool acceptToS();
        void addVotePoints(int points);
        void announceBossHpBar(Monster mm, int mobHash, Packet packet);
        void announceHint(string msg, int length);
        void announceServerMessage();
        bool attemptCsCoupon();
        void banHWID();
        void banMacs();
        bool canBypassPic();
        bool canBypassPin();
        bool canClickNPC();
        bool canGainCharacterSlot();
        bool canRequestCharlist();
        void changeChannel(int channel);
        bool checkBirthDate(DateTime date);
        void checkChar(int accid);
        void checkIfIdle(IdleStateEvent evt);
        bool checkPic(string other);
        bool checkPin(string other);
        void closePlayerScriptInteractions();
        void closeSession();
        bool deleteCharacter(int cid, int senderAccId);
        void disconnect(bool shutdown, bool cashshop);
        void disconnectSession();
        void enableCSActions();
        void ExceptionCaught(IChannelHandlerContext ctx, Exception cause);
        int finishLogin();
        void forceDisconnect();
        bool gainCharacterSlot();
        AbstractPlayerInteraction getAbstractPlayerInteraction();
        int getAccID();
        string getAccountName();
        short getAvailableCharacterSlots();
        short getAvailableCharacterWorldSlots();
        short getAvailableCharacterWorldSlots(int world);
        void setChannel(int channel);
        int getChannel();
        IWorldChannel getChannelServer();
        IWorldChannel getChannelServer(byte channel);
        short getCharacterSlots();
        EventManager? getEventManager(string evt);
        sbyte getGender();
        int getGMLevel();
        sbyte getGReason();
        Hwid getHwid();
        int getLanguage();
        long getLastPacket();
        int getLoginState();
        HashSet<string> getMacs();
        string getPic();
        string getPin();
        IPlayer? getPlayer();
        string getRemoteAddress();
        IEngine? getScriptEngine(string name);
        long getSessionId();
        DateTimeOffset? getTempBanCalendar();
        DateTimeOffset? getTempBanCalendarFromDB();
        int getVisibleWorlds();
        int getVotePoints();
        int getWorld();
        IWorld getWorldServer();
        bool hasBannedHWID();
        bool hasBannedIP();
        bool hasBannedMac();
        bool hasBeenBanned();
        bool isInTransition();
        bool isLoggedIn();
        List<IPlayer> loadCharacters(int serverId);
        void lockClient();
        int login(string login, string pwd, Hwid nibbleHwid);
        void pongReceived();
        void releaseClient();
        void removeClickedNPC();
        void removeScriptEngine(string name);
        void requestedServerlist(int worlds);
        void resetCsCoupon();
        void sendCharList(int server);
        void sendPacket(Packet packet);
        void setAccID(int id);
        void setAccountName(string a);

        void setCharacterOnSessionTransitionState(int cid);
        void setCharacterSlots(sbyte slots);
        void setClickedNPC();
        void setGender(sbyte m);
        void setGMLevel(int level);
        void setHwid(Hwid? hwid);
        void setLanguage(int lingua);
        void setPic(string pic);
        void setPin(string pin);
        void setPlayer(IPlayer? player);
        void setScriptEngine(string name, IEngine e);
        void setWorld(int world);
        bool tryacquireClient();
        bool tryacquireEncoder();
        void unlockClient();
        void unlockEncoder();
        void updateHwid(Hwid hwid);
        void updateLastPacket();
        void updateLoginState(int newState);
        void updateMacs(string macData);
        void UserEventTriggered(IChannelHandlerContext ctx, object evt);
        void useVotePoints(int points);

        void OpenNpc(int npcid, string? script = null);
    }
}