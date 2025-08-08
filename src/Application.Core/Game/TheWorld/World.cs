using Application.Core.Channel;
using Application.Core.EF.Entities.SystemBase;

namespace Application.Core.Game.TheWorld;

public class World
{
    private ILogger log;
    public int Id { get; set; }
    public string Name { get; set; }

    public List<WorldChannel> Channels { get; }

    public World(WorldConfigEntity config)
    {
        log = LogFactory.GetLogger("World_" + Id);
        Channels = new List<WorldChannel>();

        this.Id = config.Id;
        Name = config.Name;
    }

    public int addChannel(WorldChannel channel)
    {
        Channels.Add(channel);
        return Channels.Count;
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

}
