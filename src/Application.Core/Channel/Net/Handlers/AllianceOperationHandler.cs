/*
 This file is part of the OdinMS Maple Story NewServer
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


using Application.Core.Channel.ServerData;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author XoticStory, Ronan
 */
public class AllianceOperationHandler : ChannelHandlerBase
{

    readonly GuildManager _guildManager;

    public AllianceOperationHandler(GuildManager guildManager)
    {
        _guildManager = guildManager;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {

        var chr = c.OnlinedCharacter;

        var chrGuild = chr.GuildModel;
        if (chrGuild == null)
        {
            c.sendPacket(PacketCreator.enableActions());
            return;
        }

        var alliance = chr.AllianceModel;

        byte b = p.readByte();
        if (alliance == null)
        {
            if (b != 4)
            {
                c.sendPacket(PacketCreator.enableActions());
                return;
            }
        }
        else
        {
            if (b == 4)
            {
                chr.dropMessage(5, "Your guild is already registered on a guild alliance.");
                c.sendPacket(PacketCreator.enableActions());
                return;
            }

            if (chr.AllianceRank > 2 || !alliance.getGuilds().Contains(chr.getGuildId()))
            {
                c.sendPacket(PacketCreator.enableActions());
                return;
            }
        }

        // "alliance" is only null at case 0x04
        switch (b)
        {
            case 0x01:
                alliance!.BroadcastPlayerInfo(chr.Id);
                break;
            case 0x02:
                {
                    // Leave Alliance
                    _guildManager.GuildLeaveAlliance(chr, chr.GuildId);
                    break;
                }
            case 0x03: // Send Invite
                string guildName = p.readString();

                _guildManager.SendAllianceInvitation(c, guildName);

                break;
            case 0x04:
                {
                    // Accept Invite
                    if (chrGuild.AllianceId != 0 || chr.GuildRank != 1)
                    {
                        return;
                    }

                    int allianceId = p.readInt();
                    //slea.readMapleAsciiString();  //recruiter's guild name

                    _guildManager.AnswerAllianceInvitation(chr, allianceId, true);
                    break;
                }
            case 0x06:
                {
                    // Expel Guild
                    int guildid = p.readInt();
                    int allianceid = p.readInt();

                    _guildManager.AllianceExpelGuild(c.OnlinedCharacter, allianceid, guildid);
                    break;
                }
            case 0x07:
                {
                    // Change Alliance Leader
                    int victimid = p.readInt();

                    //NewServer.getInstance().allianceMessage(alliance.getId(), sendChangeLeader(chr.getGuild().getAllianceId(), chr.getId(), slea.readInt()), -1, -1);
                    _guildManager.ChageLeaderAllianceRank(c.OnlinedCharacter, victimid);
                    break;
                }
            case 0x08:
                string[] ranks = new string[5];
                for (int i = 0; i < 5; i++)
                {
                    ranks[i] = p.readString();
                }
                _guildManager.UpdateAllianceRank(chr, ranks);
                break;
            case 0x09:
                {
                    int int1 = p.readInt();
                    sbyte byte1 = p.ReadSByte();

                    _guildManager.ChangePlayerAllianceRank(c.OnlinedCharacter, int1, byte1 > 0);
                    break;
                }
            case 0x0A:
                string notice = p.readString();
                _guildManager.UpdateAllianceNotice(chr, notice);
                break;
            default:
                chr.dropMessage("Feature not available");
                break;
        }
    }

}
