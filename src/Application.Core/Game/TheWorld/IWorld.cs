using Application.Core.Game.Maps;
using Application.Core.Game.Relation;
using Application.Core.Game.Trades;
using Application.Core.model;
using client;
using net.packet;
using net.server.channel;
using net.server.coordinator.matchchecker;
using net.server.coordinator.partysearch;
using net.server.services;
using net.server.services.type;
using net.server.world;
using server;

namespace Application.Core.Game.TheWorld
{
    public interface IWorld
    {
        public List<IWorldChannel> Channels { get; }
        WorldGuildStorage GuildStorage { get; }
        WorldPlayerStorage Players { get; }
        Dictionary<int, ITeam> TeamStorage { get; }

        public int Id { get; set; }
        public int Flag { get; set; }
        public string EventMessage { get; set; }
        public int ExpRate { get; set; }
        public int DropRate { get; set; }
        public int BossDropRate { get; set; }
        public int MesoRate { get; set; }
        public int QuestRate { get; set; }
        public int TravelRate { get; set; }
        public int FishingRate { get; set; }
        void addCashItemBought(int snid);
        bool addChannel(IWorldChannel channel);
        void addFamily(int id, Family f);
        bool addMarriageGuest(int marriageId, int playerId);
        void addMessengerPlayer(Messenger messenger, string namefrom, int fromchannel, int position);
        void addOwlItemSearch(int itemid);
        void addPlayerHpDecrease(IPlayer chr);
        void broadcastPacket(Packet packet);
        void buddyChanged(int cid, int cidFrom, string name, int channel, BuddyList.BuddyOperation operation);
        void buddyChat(int[] recipientCharacterIds, int cidFrom, string nameFrom, string chattext);
        bool canUninstall();
        void changeEmblem(int gid, List<int> affectedPlayers, IGuild mgs);
        Messenger createMessenger(MessengerCharacter chrfor);
        ITeam createParty(IPlayer chrfor);
        int createRelationship(int groomId, int brideId);
        void debugMarriageStatus();
        void declineChat(string sender, IPlayer player);
        void deleteRelationship(int playerId, int partnerId);
        void dropMessage(int type, string message);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">character id</param>
        /// <returns>channel</returns>
        int find(int id);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">character name</param>
        /// <returns>channel</returns>
        int find(string name);
        Storage getAccountStorage(int accountId);
        List<HiredMerchant> getActiveMerchants();
        List<PlayerShop> getActivePlayerShops();
        List<KeyValuePair<PlayerShopItem, AbstractMapObject>> getAvailableItemBundles(int itemid);
        IWorldChannel getChannel(int channel);
        List<IWorldChannel> getChannels();
        int getChannelsSize();
        ICollection<Family> getFamilies();
        Family? getFamily(int id);
        IGuild? getGuild(IPlayer? mgc);
        HiredMerchant? getHiredMerchant(int ownerid);
        int getId();
        CoupleIdPair? getMarriageQueuedCouple(int marriageId);
        KeyValuePair<bool, bool>? getMarriageQueuedLocation(int marriageId);
        MatchCheckerCoordinator getMatchCheckerCoordinator();
        Messenger? getMessenger(int messengerid);
        List<List<int>> getMostSellerCashItems();
        List<KeyValuePair<int, int>> getOwlSearchedItems();
        ITeam? getParty(int partyid);
        PartySearchCoordinator getPartySearchCoordinator();
        int getPlayerNpcMapPodiumData(int mapid);
        int getPlayerNpcMapStep(int mapid);
        PlayerShop? getPlayerShop(int ownerid);
        WorldPlayerStorage getPlayerStorage();
        CoupleIdPair? getRelationshipCouple(int relationshipId);
        int getRelationshipId(int playerId);
        BaseService getServiceAccess(WorldServices sv);
        int getTransportationTime(int travelTime);
        CoupleIdPair? getWeddingCoupleForGuest(int guestId, bool cathedral);
        int getWorldCapacityStatus();
        bool isConnected(string charName);
        bool isGuildQueued(int guildId);
        bool isMarriageQueued(int marriageId);
        bool isWorldCapacityFull();
        void joinMessenger(int messengerid, MessengerCharacter target, string from, int fromchannel);
        void leaveMessenger(int messengerid, MessengerCharacter target);
        void loadAccountStorage(int accountId);
        List<IPlayer> loadAndGetAllCharactersView();
        void loggedOff(string name, int characterId, int channel, int[] buddies);
        void loggedOn(string name, int characterId, int channel, int[] buddies);
        void messengerChat(Messenger messenger, string chattext, string namefrom);
        void messengerInvite(string sender, int messengerid, string target, int fromchannel);
        CharacterIdChannelPair[] multiBuddyFind(int charIdFrom, int[] characterIds);
        void partyChat(ITeam party, string chattext, string namefrom);
        void putGuildQueued(int guildId);
        void putMarriageQueued(int marriageId, bool cathedral, bool premium, int groomId, int brideId);
        bool registerDisabledServerMessage(int chrid);
        bool registerFisherPlayer(IPlayer chr, int baitLevel);
        void registerHiredMerchant(HiredMerchant hm);
        void registerMountHunger(IPlayer chr);
        void registerPetHunger(IPlayer chr, sbyte petSlot);
        void registerPlayerShop(PlayerShop ps);
        void registerTimedMapObject(Action r, long duration);
        int removeChannel();
        void removeFamily(int id);
        void removeGuildQueued(int guildId);
        void removeMapPartyMembers(int partyid);
        KeyValuePair<bool, HashSet<int>> removeMarriageQueued(int marriageId);
        void removeMessengerPlayer(Messenger messenger, int position);
        void removePlayer(IPlayer chr);
        void removePlayerHpDecrease(IPlayer chr);
        BuddyList.BuddyAddResult requestBuddyAdd(string addName, int channelFrom, int cidFrom, string nameFrom);
        void resetDisabledServerMessages();
        void resetPlayerNpcMapData();
        void runCheckFishingSchedule();
        void runDisabledServerMessagesSchedule();
        void runHiredMerchantSchedule();
        void runMountSchedule();
        void runPartySearchUpdateSchedule();
        void runPetSchedule();
        void runPlayerHpDecreaseSchedule();
        void runTimedMapObjectSchedule();
        void sendPacket(List<int> targetIds, Packet packet, int exception);
        void setDropRate(int drop);
        void setExpRate(int exp);
        void setMesoRate(int meso);
        void setGuildAndRank(int cid, int guildid, int rank);
        void setGuildAndRank(List<int> cids, int guildid, int rank, int exception);

        void setOfflineGuildStatus(int guildid, int guildrank, int cid);
        void setPlayerNpcMapData(int mapid, int step, int podium);
        void setPlayerNpcMapPodiumData(int mapid, int podium);
        void setPlayerNpcMapStep(int mapid, int step);
        void setServerMessage(string msg);
        void shutdown();
        void silentJoinMessenger(int messengerid, MessengerCharacter target, int position);
        void silentLeaveMessenger(int messengerid, MessengerCharacter target);
        void unregisterAccountStorage(int accountId);
        bool unregisterDisabledServerMessage(int chrid);
        int unregisterFisherPlayer(IPlayer chr);
        void unregisterHiredMerchant(HiredMerchant hm);
        void unregisterMountHunger(IPlayer chr);
        void unregisterPetHunger(IPlayer chr, sbyte petSlot);
        void unregisterPlayerShop(PlayerShop ps);
        void updateMessenger(int messengerid, string namefrom, int fromchannel);
        void updateMessenger(Messenger messenger, string namefrom, int position, int fromchannel);
        void updateParty(int partyid, PartyOperation operation, IPlayer target);
    }
}