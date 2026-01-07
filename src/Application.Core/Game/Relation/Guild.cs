using Application.Core.Channel;
using net.server.guild;
using System.Collections.Concurrent;

namespace Application.Core.Game.Relation;


public class PlayerGuildSnapshot
{
    public PlayerGuildSnapshot(int guildId, string guildName, int allianceId, int logo, int logoColor, int logoBg, int logoBgColor)
    {
        GuildId = guildId;
        GuildName = guildName;
        AllianceId = allianceId;
        Logo = logo;
        LogoColor = logoColor;
        LogoBg = logoBg;
        LogoBgColor = logoBgColor;
    }

    public int GuildId { get; set; }
    public string GuildName { get; set; }
    public int AllianceId { get; set; }
    public int Logo { get; set; }
    public int LogoColor { get; set; }
    public int LogoBg { get; set; }
    public int LogoBgColor { get; set; }

}

public class PlayerAllianceSnapshot
{
    public PlayerAllianceSnapshot(int id, string name, int capacity)
    {
        Id = id;
        Name = name;
        Capacity = capacity;
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public int Capacity { get; set; }
}