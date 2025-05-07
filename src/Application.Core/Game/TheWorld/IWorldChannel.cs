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


using Application.Core.Game.Maps;
using Application.Core.Game.Relation;
using Application.Core.Game.Tasks;
using Application.Core.Game.Trades;
using Application.Core.Gameplay.ChannelEvents;
using Application.Core.model;
using Application.Core.Servers;
using Application.Core.ServerTransports;
using Application.Shared.Configs;
using net.packet;
using net.server.services;
using net.server.services.type;
using scripting.Event;
using server.events.gm;
using server.expeditions;
using server.maps;
using System.Net;

namespace Application.Core.Game.TheWorld
{
    public interface IWorldChannel: IServerBase<IChannelServerTransport>
    {
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

        void UpdateWorldConfig(WorldConfigPatch updatePatch);

        int getTransportationTime(double travelTime);
        bool acceptOngoingWedding(bool cathedral);
        bool addExpedition(Expedition exped);
        void addHiredMerchant(int chrid, HiredMerchant hm);
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
        Dictionary<int, HiredMerchant> getHiredMerchants();
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
        void removeHiredMerchant(int chrid);
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
        Task Shutdown();
        void unregisterOwnedMap(IMap map);
    }
}