using Application.Core.Game.Relation;

namespace net.server.guild;



public class GuildPackets
{
    public static Packet showGuildInfo(IPlayer? chr)
    {
        OutPacket p = OutPacket.create(SendOpcode.GUILD_OPERATION);
        p.writeByte(0x1A); //signature for showing guild info
        if (chr == null)
        { //show empty guild (used for leaving, expelled)
            p.writeByte(0);
            return p;
        }
        var g = chr.GuildModel;
        if (g == null)
        { //failed to read from DB - don't show a guild
            p.writeByte(0);
            return p;
        }
        getGuildInfo(p, g);
        return p;
    }

    public static Packet guildMemberOnline(int guildId, int chrId, bool bOnline)
    {
        OutPacket p = OutPacket.create(SendOpcode.GUILD_OPERATION);
        p.writeByte(0x3d);
        p.writeInt(guildId);
        p.writeInt(chrId);
        p.writeBool(bOnline);
        return p;
    }

    public static Packet guildInvite(int guildId, string charName)
    {
        OutPacket p = OutPacket.create(SendOpcode.GUILD_OPERATION);
        p.writeByte(0x05);
        p.writeInt(guildId);
        p.writeString(charName);
        return p;
    }

    public static Packet createGuildMessage(string masterName, string guildName)
    {
        OutPacket p = OutPacket.create(SendOpcode.GUILD_OPERATION);
        p.writeByte(0x3);
        p.writeInt(0);
        p.writeString(masterName);
        p.writeString(guildName);
        return p;
    }

    /// <summary>
    /// <para>28: guild name already in use</para>
    /// <para>31: problem in locating players during agreement</para>
    /// <para>33/40: already joined a guild</para>
    /// <para>35: Cannot make guild</para>
    /// <para>36: problem in player agreement</para>
    /// <para>38: problem during forming guild</para>
    /// <para>41: max number of players in joining guild</para>
    /// <para>42: character can't be found this channel</para>
    /// <para>45/48: character not in guild</para>
    /// <para>52: problem in disbanding guild</para>
    /// <para>56: admin cannot make guild</para>
    /// <para>57: problem in increasing guild size</para>
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public static Packet genericGuildMessage(byte code)
    {
        OutPacket p = OutPacket.create(SendOpcode.GUILD_OPERATION);
        p.writeByte(code);
        return p;
    }


    /// <summary>
    /// <para>53: player not accepting guild invites</para>
    /// <para>54: player already managing an invite</para>
    /// <para>55: player denied an invite</para>
    /// </summary>
    /// <param name="code"></param>
    /// <param name="targetName"></param>
    /// <returns></returns>
    public static Packet responseGuildMessage(byte code, string targetName)
    {
        OutPacket p = OutPacket.create(SendOpcode.GUILD_OPERATION);
        p.writeByte(code);
        p.writeString(targetName);
        return p;
    }

    public static Packet newGuildMember(int guildId, GuildMember mgc)
    {
        OutPacket p = OutPacket.create(SendOpcode.GUILD_OPERATION);
        p.writeByte(0x27);
        p.writeInt(guildId);
        p.writeInt(mgc.Id);
        p.writeFixedString(mgc.Name);
        p.writeInt(mgc.JobId);
        p.writeInt(mgc.Level);
        p.writeInt(mgc.GuildRank); //should be always 5 but whatevs
        p.writeInt(mgc.Channel > 0 ? 1 : 0); //should always be 1 too
        p.writeInt(1); //? could be guild signature, but doesn't seem to matter
        p.writeInt(3);
        return p;
    }

    //someone leaving, mode == 0x2c for leaving, 0x2f for expelled
    public static Packet memberLeft(IPlayer mgc, bool bExpelled) => memberLeft(mgc.GuildId, mgc.Id, mgc.Name, bExpelled);

    public static Packet memberLeft(int guildId, int playerId, string playerName, bool bExpelled)
    {
        OutPacket p = OutPacket.create(SendOpcode.GUILD_OPERATION);
        p.writeByte(bExpelled ? 0x2f : 0x2c);
        p.writeInt(guildId);
        p.writeInt(playerId);
        p.writeString(playerName);
        return p;
    }

    //rank change
    public static Packet changeRank(IPlayer mgc) => changeRank(mgc.GuildId, mgc.Id, mgc.GuildRank);


    public static Packet changeRank(int guildId, int playerId, int guildRank)
    {
        OutPacket p = OutPacket.create(SendOpcode.GUILD_OPERATION);
        p.writeByte(0x40);
        p.writeInt(guildId);
        p.writeInt(playerId);
        p.writeByte(guildRank);
        return p;
    }

    public static Packet guildNotice(int guildId, string notice)
    {
        OutPacket p = OutPacket.create(SendOpcode.GUILD_OPERATION);
        p.writeByte(0x44);
        p.writeInt(guildId);
        p.writeString(notice);
        return p;
    }

    public static Packet guildMemberLevelJobUpdate(int guildId, int playerId, int level, int jobId)
    {
        OutPacket p = OutPacket.create(SendOpcode.GUILD_OPERATION);
        p.writeByte(0x3C);
        p.writeInt(guildId);
        p.writeInt(playerId);
        p.writeInt(level);
        p.writeInt(jobId);
        return p;
    }

    public static Packet rankTitleChange(int guildId, string[] ranks)
    {
        OutPacket p = OutPacket.create(SendOpcode.GUILD_OPERATION);
        p.writeByte(0x3E);
        p.writeInt(guildId);
        for (int i = 0; i < 5; i++)
        {
            p.writeString(ranks[i]);
        }
        return p;
    }

    public static Packet guildDisband(int guildId)
    {
        OutPacket p = OutPacket.create(SendOpcode.GUILD_OPERATION);
        p.writeByte(0x32);
        p.writeInt(guildId);
        p.writeByte(1);
        return p;
    }

    public static Packet guildQuestWaitingNotice(byte channel, int waitingPos)
    {
        OutPacket p = OutPacket.create(SendOpcode.GUILD_OPERATION);
        p.writeByte(0x4C);
        p.writeByte(channel - 1);
        p.writeByte(waitingPos);
        return p;
    }

    public static Packet guildEmblemChange(int guildId, short bg, byte bgcolor, short logo, byte logoColor)
    {
        OutPacket p = OutPacket.create(SendOpcode.GUILD_OPERATION);
        p.writeByte(0x42);
        p.writeInt(guildId);
        p.writeShort(bg);
        p.writeByte(bgcolor);
        p.writeShort(logo);
        p.writeByte(logoColor);
        return p;
    }

    public static Packet guildCapacityChange(int guildId, int capacity)
    {
        OutPacket p = OutPacket.create(SendOpcode.GUILD_OPERATION);
        p.writeByte(0x3A);
        p.writeInt(guildId);
        p.writeByte(capacity);
        return p;
    }



    public static Packet showGuildRanks(int npcid, List<GuildProto.GuildDto> dataList)
    {
        OutPacket p = OutPacket.create(SendOpcode.GUILD_OPERATION);
        p.writeByte(0x49);
        p.writeInt(npcid);
        if (dataList.Count == 0)
        { //no guilds o.o
            p.writeInt(0);
            return p;
        }
        p.writeInt(dataList.Count); //number of entries
        foreach (var rs in dataList)
        {
            p.writeString(rs.Name);
            p.writeInt(rs.Gp);
            p.writeInt(rs.Logo);
            p.writeInt(rs.LogoColor);
            p.writeInt(rs.LogoBg);
            p.writeInt(rs.LogoBgColor);
        }
        return p;
    }

    public static Packet showPlayerRanks(int npcid, List<RankedCharacterInfo> worldRanking)
    {
        OutPacket p = OutPacket.create(SendOpcode.GUILD_OPERATION);
        p.writeByte(0x49);
        p.writeInt(npcid);
        if (worldRanking.Count == 0)
        {
            p.writeInt(0);
            return p;
        }
        p.writeInt(worldRanking.Count);
        foreach (var wr in worldRanking)
        {
            p.writeString(wr.CharacterName);
            p.writeInt(wr.CharacterLevel);
            p.writeInt(0);
            p.writeInt(0);
            p.writeInt(0);
            p.writeInt(0);
        }
        return p;
    }

    public static Packet updateGP(int guildId, int GP)
    {
        OutPacket p = OutPacket.create(SendOpcode.GUILD_OPERATION);
        p.writeByte(0x48);
        p.writeInt(guildId);
        p.writeInt(GP);
        return p;
    }

    public static void getGuildInfo(OutPacket p, Guild guild)
    {
        p.writeInt(guild.getId());
        p.writeString(guild.getName());
        for (int i = 1; i <= 5; i++)
        {
            p.writeString(guild.getRankTitle(i));
        }
        var members = guild.getMembers();
        p.writeByte(members.Count);
        foreach (var mgc in members)
        {
            p.writeInt(mgc.Id);
        }
        foreach (var mgc in members)
        {
            p.writeFixedString(mgc.Name);
            p.writeInt(mgc.JobId);
            p.writeInt(mgc.Level);
            p.writeInt(mgc.GuildRank);
            p.writeInt(mgc.Channel > 0 ? 1 : 0);
            p.writeInt((int)guild.getSignature());
            p.writeInt(mgc.AllianceRank);
        }
        p.writeInt(guild.getCapacity());
        p.writeShort(guild.getLogoBG());
        p.writeByte(guild.getLogoBGColor());
        p.writeShort(guild.getLogo());
        p.writeByte(guild.getLogoColor());
        p.writeString(guild.getNotice());
        p.writeInt(guild.getGP());
        p.writeInt(guild.getAllianceId());
    }

    public static Packet getAllianceInfo(Alliance alliance)
    {
        OutPacket p = OutPacket.create(SendOpcode.ALLIANCE_OPERATION);
        p.writeByte(0x0C);
        p.writeByte(1);
        p.writeInt(alliance.getId());
        p.writeString(alliance.getName());
        for (int i = 1; i <= 5; i++)
        {
            p.writeString(alliance.getRankTitle(i));
        }
        p.writeByte(alliance.getGuilds().Count);
        p.writeInt(alliance.getCapacity()); // probably capacity
        foreach (int guild in alliance.getGuilds())
        {
            p.writeInt(guild);
        }
        p.writeString(alliance.getNotice());
        return p;
    }

    public static Packet updateAllianceInfo(Alliance alliance)
    {
        OutPacket p = OutPacket.create(SendOpcode.ALLIANCE_OPERATION);
        p.writeByte(0x0F);
        p.writeInt(alliance.getId());
        p.writeString(alliance.getName());
        for (int i = 1; i <= 5; i++)
        {
            p.writeString(alliance.getRankTitle(i));
        }
        p.writeByte(alliance.getGuilds().Count);
        foreach (int guild in alliance.getGuilds())
        {
            p.writeInt(guild);
        }
        p.writeInt(alliance.getCapacity()); // probably capacity
        p.writeShort(0);

        var allianceGuilds = alliance.Guilds;
        foreach (var guild in allianceGuilds)
        {
            getGuildInfo(p, guild.Value);
        }
        return p;
    }

    public static Packet getGuildAlliances(Alliance alliance)
    {
        OutPacket p = OutPacket.create(SendOpcode.ALLIANCE_OPERATION);
        p.writeByte(0x0D);

        var allianceGuilds = alliance.Guilds;
        p.writeInt(allianceGuilds.Count);
        foreach (var guild in allianceGuilds)
        {
            getGuildInfo(p, guild.Value);
        }
        return p;
    }

    public static Packet addGuildToAlliance(Alliance alliance, Guild newGuild)
    {
        OutPacket p = OutPacket.create(SendOpcode.ALLIANCE_OPERATION);
        p.writeByte(0x12);
        p.writeInt(alliance.getId());
        p.writeString(alliance.getName());
        for (int i = 1; i <= 5; i++)
        {
            p.writeString(alliance.getRankTitle(i));
        }
        p.writeByte(alliance.getGuilds().Count);
        foreach (int guild in alliance.getGuilds())
        {
            p.writeInt(guild);
        }
        p.writeInt(alliance.getCapacity());
        p.writeString(alliance.getNotice());
        p.writeInt(newGuild.GuildId);
        getGuildInfo(p, newGuild);
        return p;
    }

    public static Packet allianceMemberOnline(Guild guild, int playerId, bool online)
    {
        OutPacket p = OutPacket.create(SendOpcode.ALLIANCE_OPERATION);
        p.writeByte(0x0E);
        p.writeInt(guild.AllianceId);
        p.writeInt(guild.GuildId);
        p.writeInt(playerId);
        p.writeBool(online);
        return p;
    }

    public static Packet allianceNotice(int id, string notice)
    {
        OutPacket p = OutPacket.create(SendOpcode.ALLIANCE_OPERATION);
        p.writeByte(0x1C);
        p.writeInt(id);
        p.writeString(notice);
        return p;
    }

    public static Packet changeAllianceRankTitle(int alliance, string[] ranks)
    {
        OutPacket p = OutPacket.create(SendOpcode.ALLIANCE_OPERATION);
        p.writeByte(0x1A);
        p.writeInt(alliance);
        for (int i = 0; i < 5; i++)
        {
            p.writeString(ranks[i]);
        }
        return p;
    }


    public static Packet updateAllianceJobLevel(Guild guild, int memberId, int level, int jobId)
    {
        OutPacket p = OutPacket.create(SendOpcode.ALLIANCE_OPERATION);
        p.writeByte(0x18);
        p.writeInt(guild.AllianceId);
        p.writeInt(guild.GuildId);
        p.writeInt(memberId);
        p.writeInt(level);
        p.writeInt(jobId);
        return p;
    }

    public static Packet removeGuildFromAlliance(Alliance alliance, Guild expelledGuild)
    {
        OutPacket p = OutPacket.create(SendOpcode.ALLIANCE_OPERATION);
        p.writeByte(0x10);
        p.writeInt(alliance.getId());
        p.writeString(alliance.getName());
        for (int i = 1; i <= 5; i++)
        {
            p.writeString(alliance.getRankTitle(i));
        }

        p.writeByte(alliance.getGuilds().Count);
        foreach (int guild in alliance.getGuilds())
        {
            p.writeInt(guild);
        }
        p.writeInt(alliance.getCapacity());
        p.writeString(alliance.getNotice());
        p.writeInt(expelledGuild.GuildId);
        getGuildInfo(p, expelledGuild);
        p.writeByte(0x01);
        return p;
    }

    public static Packet disbandAlliance(int alliance)
    {
        OutPacket p = OutPacket.create(SendOpcode.ALLIANCE_OPERATION);
        p.writeByte(0x1D);
        p.writeInt(alliance);
        return p;
    }

    public static Packet allianceInvite(int allianceid, IPlayer chr)
    {
        OutPacket p = OutPacket.create(SendOpcode.ALLIANCE_OPERATION);
        p.writeByte(0x03);
        p.writeInt(allianceid);
        p.writeString(chr.getName());
        p.writeShort(0);
        return p;
    }

    public static Packet GuildBoss_HealerMove(short nY)
    {
        OutPacket p = OutPacket.create(SendOpcode.GUILD_BOSS_HEALER_MOVE);
        p.writeShort(nY); //New Y Position
        return p;
    }

    public static Packet GuildBoss_PulleyStateChange(byte nState)
    {
        OutPacket p = OutPacket.create(SendOpcode.GUILD_BOSS_PULLEY_STATE_CHANGE);
        p.writeByte(nState);
        return p;
    }

    /**
     * Guild Name & Mark update packet, thanks to Arnah (Vertisy)
     *
     * @param guildName The Guild name, blank for nothing.
     */
    public static Packet guildNameChanged(int chrid, string guildName)
    {
        OutPacket p = OutPacket.create(SendOpcode.GUILD_NAME_CHANGED);
        p.writeInt(chrid);
        p.writeString(guildName);
        return p;
    }

    public static Packet guildMarkChanged(int chrId, Guild guild) => guildMarkChanged(chrId, guild.getLogoBG(), guild.getLogoBGColor(), guild.getLogo(), guild.getLogoColor());

    public static Packet guildMarkChanged(int chrId, int logoBg, int logoBgColor, int logo, int logoColor)
    {
        OutPacket p = OutPacket.create(SendOpcode.GUILD_MARK_CHANGED);
        p.writeInt(chrId);
        p.writeShort(logoBg);
        p.writeByte(logoBgColor);
        p.writeShort(logo);
        p.writeByte(logoColor);
        return p;
    }


    public static Packet sendShowInfo(int allianceid, int playerid)
    {
        OutPacket p = OutPacket.create(SendOpcode.ALLIANCE_OPERATION);
        p.writeByte(0x02);
        p.writeInt(allianceid);
        p.writeInt(playerid);
        return p;
    }

    public static Packet sendInvitation(int allianceid, int playerid, string guildname)
    {
        OutPacket p = OutPacket.create(SendOpcode.ALLIANCE_OPERATION);
        p.writeByte(0x05);
        p.writeInt(allianceid);
        p.writeInt(playerid);
        p.writeString(guildname);
        return p;
    }

    public static Packet sendChangeGuild(int allianceid, int playerid, int guildid, int option)
    {
        OutPacket p = OutPacket.create(SendOpcode.ALLIANCE_OPERATION);
        p.writeByte(0x07);
        p.writeInt(allianceid);
        p.writeInt(guildid);
        p.writeInt(playerid);
        p.writeByte(option);
        return p;
    }

    public static Packet sendChangeLeader(int allianceid, int playerid, int victim)
    {
        OutPacket p = OutPacket.create(SendOpcode.ALLIANCE_OPERATION);
        p.writeByte(0x08);
        p.writeInt(allianceid);
        p.writeInt(playerid);
        p.writeInt(victim);
        return p;
    }

    public static Packet sendChangeRank(int allianceid, int playerid, int int1, byte byte1)
    {
        OutPacket p = OutPacket.create(SendOpcode.ALLIANCE_OPERATION);
        p.writeByte(0x09);
        p.writeInt(allianceid);
        p.writeInt(playerid);
        p.writeInt(int1);
        p.writeInt(byte1);
        return p;
    }
}
