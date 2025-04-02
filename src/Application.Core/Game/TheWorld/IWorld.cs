using Application.Core.EF.Entities.SystemBase;
using Application.Core.Game.Maps;
using Application.Core.Game.Relation;
using Application.Core.Game.Trades;
using Application.Core.Gameplay.WorldEvents;
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
        #region Events
        event Action? OnMobRateChanged;

        event Action? OnExpRateChanged;
        event Action? OnMesoRateChanged;
        event Action? OnDropRateChanged;
        event Action? OnQuestRateChanged;
        event Action? OnBossDropRateChaged;
        #endregion
        WorldConfigEntity Configs { get; set; }
        public List<IWorldChannel> Channels { get; }
        WorldGuildStorage GuildStorage { get; }
        WorldPlayerStorage Players { get; }
        Dictionary<int, ITeam> TeamStorage { get; }

        public int Id { get; set; }
        public int Flag { get; set; }
        public string Name { get; set; }
        public string WhyAmIRecommended { get; set; }
        public string ServerMessage { get; set; }
        public string EventMessage { get; set; }
        public float ExpRate { get; set; }
        public float DropRate { get; set; }
        public float BossDropRate { get; set; }
        public float MesoRate { get; set; }
        public float QuestRate { get; set; }
        public float TravelRate { get; set; }
        public float FishingRate { get; set; }
        public float MobRate { get; set; }

        public WeddingWorldInstance WeddingInstance { get; }
        public FishingWorldInstance FishingInstance { get; }
        /// <summary>
        /// 调整频道数量
        /// </summary>
        /// <param name="channelSize"></param>
        Task ResizeChannel(int channelSize);
        void addCashItemBought(int snid);
        bool addChannel(IWorldChannel channel);
        Task<int> removeChannel();
        void addFamily(int id, Family f);
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

        void declineChat(string sender, IPlayer player);
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
        int getRelationshipId(int playerId);
        BaseService getServiceAccess(WorldServices sv);
        int getTransportationTime(double travelTime);
        int getWorldCapacityStatus();
        bool isConnected(string charName);
        bool isGuildQueued(int guildId);
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
        bool registerDisabledServerMessage(int chrid);
        void registerHiredMerchant(HiredMerchant hm);
        void registerMountHunger(IPlayer chr);
        void registerPetHunger(IPlayer chr, sbyte petSlot);
        void registerPlayerShop(PlayerShop ps);
        void registerTimedMapObject(Action r, long duration);
        void removeFamily(int id);
        void removeGuildQueued(int guildId);
        void removeMapPartyMembers(int partyid);
        void removeMessengerPlayer(Messenger messenger, int position);
        void removePlayer(IPlayer chr);
        void removePlayerHpDecrease(IPlayer chr);
        BuddyList.BuddyAddResult requestBuddyAdd(string addName, int channelFrom, int cidFrom, string nameFrom);
        void resetDisabledServerMessages();
        void resetPlayerNpcMapData();
        void runDisabledServerMessagesSchedule();
        void runHiredMerchantSchedule();
        void runMountSchedule();
        void runPartySearchUpdateSchedule();
        void runPetSchedule();
        void runPlayerHpDecreaseSchedule();
        void runTimedMapObjectSchedule();
        void sendPacket(List<int> targetIds, Packet packet, int exception);
        void setDropRate(float drop);
        void setExpRate(float exp);
        void setMesoRate(float meso);
        void setGuildAndRank(int cid, int guildid, int rank);
        void setGuildAndRank(List<int> cids, int guildid, int rank, int exception);

        void setOfflineGuildStatus(int guildid, int guildrank, int cid);
        void setPlayerNpcMapData(int mapid, int step, int podium);
        void setPlayerNpcMapPodiumData(int mapid, int podium);
        void setPlayerNpcMapStep(int mapid, int step);
        Task Shutdown();
        void silentJoinMessenger(int messengerid, MessengerCharacter target, int position);
        void silentLeaveMessenger(int messengerid, MessengerCharacter target);
        void unregisterAccountStorage(int accountId);
        bool unregisterDisabledServerMessage(int chrid);
        void unregisterHiredMerchant(HiredMerchant hm);
        void unregisterMountHunger(IPlayer chr);
        void unregisterPetHunger(IPlayer chr, sbyte petSlot);
        void unregisterPlayerShop(PlayerShop ps);
        void updateMessenger(int messengerid, string namefrom, int fromchannel);
        void updateMessenger(Messenger messenger, string namefrom, int position, int fromchannel);
        void updateParty(int partyid, PartyOperation operation, IPlayer target);
    }
}