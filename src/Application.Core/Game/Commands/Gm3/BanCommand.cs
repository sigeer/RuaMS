using Application.Core.Managers;
using server;
using System.Text.RegularExpressions;
using tools;

namespace Application.Core.Game.Commands.Gm3;

public class BanCommand : CommandBase
{
    public BanCommand() : base(3, "ban")
    {
        Description = "Ban a player.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 2)
        {
            player.yellowMessage("Syntax: !ban <IGN> <Reason> (Please be descriptive)");
            return;
        }
        string ign = paramsValue[0];
        string reason = joinStringFrom(paramsValue, 1);
        var target = c.getChannelServer().getPlayerStorage().getCharacterByName(ign);
        if (target != null)
        {
            string readableTargetName = CharacterManager.makeMapleReadable(target.getName());
            string ip = target.getClient().RemoteAddress;
            //Ban ip
            try
            {
                if (Regex.IsMatch(ip, "/[0-9]{1,3}\\..*"))
                {
                    using var dbConetxt = new DBContext();
                    dbConetxt.Ipbans.Add(new Ipban
                    {
                        Ip = ip,
                        Aid = target.getClient().AccountEntity.Id.ToString()
                    });
                    dbConetxt.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                c.OnlinedCharacter.message("Error occured while banning IP address");
                c.OnlinedCharacter.message(target.getName() + "'s IP was not banned: " + ip);
            }
            target.getClient().BanMacs();
            reason = c.OnlinedCharacter.getName() + " banned " + readableTargetName + " for " + reason + " (IP: " + ip + ") " + "(MAC: " + c.AccountEntity.Macs + ")";
            target.ban(reason);
            target.yellowMessage("You have been banned by #b" + c.OnlinedCharacter.getName() + " #k.");
            target.yellowMessage("Reason: " + reason);
            c.sendPacket(PacketCreator.getGMEffect(4, 0));
            var rip = target;
            TimerManager.getInstance().schedule(() => rip.getClient().Disconnect(false, false), TimeSpan.FromSeconds(5)); //5 Seconds
            c.CurrentServerContainer.BroadcastWorldMessage(PacketCreator.serverNotice(6, "[RIP]: " + ign + " has been banned."));
        }
        else if (CharacterManager.Ban(ign, reason, false))
        {
            c.sendPacket(PacketCreator.getGMEffect(4, 0));
            c.CurrentServerContainer.BroadcastWorldMessage(PacketCreator.serverNotice(6, "[RIP]: " + ign + " has been banned."));
        }
        else
        {
            c.sendPacket(PacketCreator.getGMEffect(6, 1));
        }
    }
}
