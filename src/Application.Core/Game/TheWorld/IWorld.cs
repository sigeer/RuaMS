using Application.Core.EF.Entities.SystemBase;
using Application.Core.Game.Maps;
using Application.Core.Game.Relation;
using Application.Core.Game.Trades;
using Application.Core.Gameplay.WorldEvents;
using Application.Core.model;
using Application.Core.Servers;
using Application.Core.ServerTransports;
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
        WorldConfigEntity Configs { get; set; }
        public List<IWorldChannel> Channels { get; }
        WorldGuildStorage GuildStorage { get; }
        WorldPlayerStorage Players { get; }
        Dictionary<int, ITeam> TeamStorage { get; }

        public int Id { get; set; }
        public int Flag { get; set; }
        public string Name { get; set; }
        public string WhyAmIRecommended { get; set; }
        public string EventMessage { get; set; }
        public FishingWorldInstance FishingInstance { get; }

        void addCashItemBought(int snid);
        int addChannel(IWorldChannel channel);
        Task<int> removeChannel();
        void addFamily(int id, Family f);
        void addMessengerPlayer(Messenger messenger, string namefrom, int fromchannel, int position);
        void addOwlItemSearch(int itemid);

        void broadcastPacket(Packet packet);
        void buddyChanged(int cid, int cidFrom, string name, int channel, BuddyList.BuddyOperation operation);
        void buddyChat(int[] recipientCharacterIds, int cidFrom, string nameFrom, string chattext);
        bool canUninstall();
        void changeEmblem(int gid, List<int> affectedPlayers, IGuild mgs);
        Messenger createMessenger(MessengerCharacter chrfor);
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
        List<PlayerShop> getActivePlayerShops();
        List<KeyValuePair<PlayerShopItem, AbstractMapObject>> getAvailableItemBundles(int itemid);
        IWorldChannel getChannel(int channel);
        List<IWorldChannel> getChannels();
        int getChannelsSize();
        ICollection<Family> getFamilies();
        Family? getFamily(int id);
        IGuild? getGuild(IPlayer? mgc);
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
        BaseService getServiceAccess(WorldServices sv);
        int getWorldCapacityStatus();
        bool isConnected(string charName);
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
        void registerPetHunger(IPlayer chr, sbyte petSlot);
        void registerPlayerShop(PlayerShop ps);
        void removeFamily(int id);
        void removeMessengerPlayer(Messenger messenger, int position);
        void removePlayer(IPlayer chr);
        BuddyList.BuddyAddResult requestBuddyAdd(string addName, int channelFrom, int cidFrom, string nameFrom);
        void resetPlayerNpcMapData();
        void runPartySearchUpdateSchedule();
        void runPetSchedule();
        void sendPacket(List<int> targetIds, Packet packet, int exception);
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
        void unregisterPetHunger(IPlayer chr, sbyte petSlot);
        void unregisterPlayerShop(PlayerShop ps);
        void updateMessenger(int messengerid, string namefrom, int fromchannel);
        void updateMessenger(Messenger messenger, string namefrom, int position, int fromchannel);
    }
}