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


using Application.Core.Datas;
using Application.Core.Game.Maps;
using Application.Core.Game.Relation;
using Application.Core.Game.Tasks;
using Application.Core.Gameplay.ChannelEvents;
using Application.Core.Servers;
using Application.Core.ServerTransports;
using Application.Shared.Configs;
using Application.Shared.Net;
using Application.Shared.Servers;
using Microsoft.Extensions.DependencyInjection;
using net.packet;
using net.server.services;
using net.server.services.type;
using scripting.Event;
using scripting.map;
using scripting.npc;
using scripting.portal;
using scripting.quest;
using scripting.reactor;
using server.events.gm;
using server.expeditions;
using server.maps;
using System.Net;

namespace Application.Core.Game.TheWorld
{
    public interface IWorldChannel : IServerBase<IChannelServerTransport>
    {
        IServiceScope LifeScope { get; }

        ChannelClientStorage ClientStorage { get; }

        public event Action? OnWorldMobRateChanged;
        public float WorldMobRate { get; }
        public event Action? OnWorldMesoRateChanged;
        public float WorldMesoRate { get; }
        public event Action? OnWorldExpRateChanged;
        public float WorldExpRate { get; }
        public event Action? OnWorldDropRateChanged;
        public float WorldDropRate { get; }
        public event Action? OnWorldBossDropRateChanged;
        public float WorldBossDropRate { get; }
        public event Action? OnWorldQuestRateChanged;
        public float WorldQuestRate { get; }
        public float WorldTravelRate { get; }
        public float WorldFishingRate { get; }
        public string WorldServerMessage { get; }
        ChannelServerConfig ServerConfig { get; }
        ChannelPlayerStorage Players { get; }
        DojoInstance DojoInstance { get; }
        WeddingChannelInstance WeddingInstance { get; }
        ServerMessageController ServerMessageController { get; }
        CharacterHpDecreaseController CharacterHpDecreaseController { get; }
        MapObjectController MapObjectController { get; }
        MountTirednessController MountTirednessController { get; }
        HiredMerchantController HiredMerchantController { get; }
        PetHungerController PetHungerController { get; }
        CharacterDiseaseController CharacterDiseaseController { get; }

        MapScriptManager MapScriptManager { get; }
        ReactorScriptManager ReactorScriptManager { get; }
        NPCScriptManager NPCScriptManager { get; }
        PortalScriptManager PortalScriptManager { get; }
        QuestScriptManager QuestScriptManager { get; }
        void UpdateWorldConfig(WorldConfigPatch updatePatch);

        int getTransportationTime(double travelTime);
        bool acceptOngoingWedding(bool cathedral);
        bool addExpedition(Expedition exped);
        bool addMiniDungeon(int dungeonid);
        void addPlayer(IPlayer chr);
        void broadcastGMPacket(Packet packet);
        void broadcastPacket(Packet packet);
        bool canInitMonsterCarnival(bool cpq1, int field);
        bool canUninstall();
        void closeOngoingWedding(bool cathedral);
        void dismissDojoSchedule(int dojoMapId, ITeam party);
        void dropMessage(int type, string message);
        void finishMonsterCarnival(bool cpq1, int field);
        void freeDojoSectionIfEmpty(int dojoMapId);
        int getChannelCapacity();
        long getDojoFinishTime(int dojoMapId);
        Event? getEvent();
        EventScriptManager getEventSM();
        Expedition? getExpedition(ExpeditionType type);
        List<Expedition> getExpeditions();
        int getId();
        IPEndPoint getIP();
        MapManager getMapFactory();
        MiniDungeon? getMiniDungeon(int dungeonid);
        int getOngoingWedding(bool cathedral);
        ChannelPlayerStorage getPlayerStorage();
        string getServerMessage();
        BaseService getServiceAccess(ChannelServices sv);
        int getStoredVar(int key);
        int getWeddingReservationStatus(int? weddingId, bool cathedral);
        string? getWeddingReservationTimeLeft(int? weddingId);
        int getWorld();
        int ingressDojo(bool isPartyDojo, int fromStage);
        int ingressDojo(bool isPartyDojo, ITeam? party, int fromStage);
        void initMonsterCarnival(bool cpq1, int field);
        void insertPlayerAway(int chrId);
        bool isActive();
        bool isOngoingWeddingGuest(bool cathedral, int playerId);
        bool isWeddingReserved(int weddingId);
        int lookupPartyDojo(ITeam? party);
        int[] multiBuddyFind(int charIdFrom, int[] characterIds);
        int pushWeddingReservation(int? weddingId, bool cathedral, bool premium, int groomId, int brideId);
        void registerOwnedMap(IMap map);
        void reloadEventScriptManager();
        void removeExpedition(Expedition exped);
        void removeMiniDungeon(int dungeonid);
        bool removePlayer(IPlayer chr);
        void removePlayerAway(int chrId);
        void resetDojo(int dojoMapId);
        void resetDojoMap(int fromMapId);
        void runCheckOwnedMapsSchedule();
        bool setDojoProgress(int dojoMapId);
        void setEvent(Event? evt);
        void setServerMessage(string message);
        void setStoredVar(int key, int val);
        void unregisterOwnedMap(IMap map);
        void BroadcastWorldPacket(Packet p);
        void StashCharacterBuff(IPlayer player);
        void StashCharacterDisease(IPlayer player);

        void RecoverCharacterBuff(IPlayer character);
        void RecoverCharacterDisease(IPlayer character);
        IPEndPoint GetChannelEndPoint(int channel);

        /// <summary>
        /// 向id的partner推送通知
        /// </summary>
        /// <param name="id"></param>
        void NotifyPartner(int id);
        /// <summary>
        /// 获取一个正在准备进入游戏的CharacterValueObject
        /// <para>1. 既是cid的player数据</para>
        /// <para>2. 且该account正处于LOGIN_SERVER_TRANSITION</para>
        /// </summary>
        /// <param name="cid"></param>
        /// <returns></returns>
        CharacterValueObject? GetPlayerData(string clientSession, int cid);
        /// <summary>
        /// 下线时，更新好友窗口
        /// </summary>
        /// <param name="characterId"></param>
        /// <param name="channel"></param>
        /// <param name="buddies"></param>
        void UpdateBuddyByLoggedOff(int characterId, int channel, int[] buddies);
        /// <summary>
        /// 上线时，更新好友窗口
        /// </summary>
        /// <param name="characterId"></param>
        /// <param name="channel"></param>
        /// <param name="buddies"></param>
        void UpdateBuddyByLoggedIn(int characterId, int channel, int[] buddies);
        /// <summary>
        /// 将name传送到频道channel的mapId
        /// </summary>
        /// <param name="name"></param>
        /// <param name="channel"></param>
        /// <param name="mapId"></param>
        /// <param name="portal"></param>
        /// <returns></returns>
        bool WarpPlayer(string name, int? channel, int mapId, int? portal);
        string GetExpeditionInfo();
        /// <summary>
        /// 修改玩家的alliance等级
        /// </summary>
        /// <param name="targetCharacterId"></param>
        /// <param name="isRaise"></param>
        void ChangePlayerAllianceRank(int targetCharacterId, bool isRaise);
        int GetAccountCharcterCount(int accId);
        bool CheckCharacterName(string name);
        void SendLogoff(int id);
    }
}