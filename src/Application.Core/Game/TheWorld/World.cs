using Application.Core.Channel;
using Application.Core.EF.Entities.SystemBase;
using Application.Core.Game.Relation;
using Application.Core.Game.Trades;
using Application.Shared.Constants.Buddy;
using net.server;
using net.server.channel;
using net.server.coordinator.matchchecker;
using net.server.task;
using tools;
using static Application.Core.Game.Relation.BuddyList;

namespace Application.Core.Game.TheWorld;

public class World
{
    private ILogger log;
    public int Id { get; set; }
    public string Name { get; set; }

    public List<WorldChannel> Channels { get; }
    WorldPlayerStorage? _players;
    public WorldPlayerStorage Players => _players ?? (_players = new WorldPlayerStorage(Id));

    private MatchCheckerCoordinator matchChecker = new MatchCheckerCoordinator();

    private ScheduledFuture? timeoutSchedule;

    public World(WorldConfigEntity config)
    {
        log = LogFactory.GetLogger("World_" + Id);
        Channels = new List<WorldChannel>();

        this.Id = config.Id;
        Name = config.Name;
#if !DEBUG
        timeoutSchedule = Server.getInstance().GlobalTimerManager.register(new TimeoutTask(this), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
#endif

    }

    public List<WorldChannel> getChannels()
    {
        return new(Channels);
    }

    public WorldChannel getChannel(int channel)
    {
        return Channels.ElementAtOrDefault(channel - 1) ?? throw new BusinessFatalException($"Channel {channel} not existed");
    }

    public int addChannel(WorldChannel channel)
    {
        Channels.Add(channel);
        Players.RelateChannel(channel.getId(), channel.Players);
        return Channels.Count;
    }


    public WorldPlayerStorage getPlayerStorage()
    {
        return Players;
    }

    public MatchCheckerCoordinator getMatchCheckerCoordinator()
    {
        return matchChecker;
    }


    public void removePlayer(IPlayer chr)
    {
        var cserv = chr.getChannelServer();

        if (cserv != null)
        {
            if (!cserv.removePlayer(chr))
            {
                // oy the player is not where they should be, find this mf

                foreach (var ch in Channels)
                {
                    if (ch.removePlayer(chr))
                    {
                        break;
                    }
                }
            }
        }
    }


    #region Messenger


    //public void declineChat(string sender, IPlayer player)
    //{
    //    if (isConnected(sender))
    //    {
    //        var senderChr = getPlayerStorage().getCharacterByName(sender);
    //        if (senderChr != null && senderChr.IsOnlined && senderChr.ChatRoomId > 0)
    //        {
    //            if (InviteType.MESSENGER.AnswerInvite(player.getId(), senderChr.ChatRoomId, false).Result == InviteResultType.DENIED)
    //            {
    //                senderChr.sendPacket(PacketCreator.messengerNote(player.getName(), 5, 0));
    //            }
    //        }
    //    }
    //}
    //public bool isConnected(string charName)
    //{
    //    return getPlayerStorage().getCharacterByName(charName)?.IsOnlined == true;
    //}
    #endregion

    public void broadcastPacket(Packet packet)
    {
        foreach (IPlayer chr in Players.GetAllOnlinedPlayers())
        {
            chr.sendPacket(packet);
        }
    }

    public List<KeyValuePair<PlayerShopItem, IPlayerShop>> getAvailableItemBundles(int itemid)
    {
        List<KeyValuePair<PlayerShopItem, IPlayerShop>> hmsAvailable = new();

        foreach (var ch in getChannels())
        {
            foreach (var hm in ch.PlayerShopManager.GetAllOpeningShops())
            {
                List<PlayerShopItem> itemBundles = hm.QueryAvailableBundles(itemid);

                foreach (PlayerShopItem mpsi in itemBundles)
                {
                    hmsAvailable.Add(new(mpsi, hm));
                }
            }
        }


        hmsAvailable = hmsAvailable.OrderBy(x => x.Key.getPrice()).Take(200).ToList();
        return hmsAvailable;
    }


    public void dropMessage(int type, string message)
    {
        foreach (var player in getPlayerStorage().GetAllOnlinedPlayers())
        {
            player.dropMessage(type, message);
        }
    }

    public async Task Shutdown()
    {
        foreach (var ch in getChannels())
        {
            await ch.Shutdown();
        }

        if (timeoutSchedule != null)
        {
            await timeoutSchedule.CancelAsync(false);
            timeoutSchedule = null;
        }
        Players.disconnectAll();
        log.Information("Finished shutting down world {WorldId}", Id);
    }
}
