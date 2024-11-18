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


using Application.Core.Game.Relation;
using Application.Core.Game.TheWorld;
using Application.Core.Managers;
using net.packet;
using net.server.guild;
using tools;

namespace net.server.channel.handlers;

/**
 * @author XoticStory, Ronan
 */
public class AllianceOperationHandler : AbstractPacketHandler
{

    public override void HandlePacket(InPacket p, IClient c)
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
                alliance.broadcastMessage(GuildPackets.sendShowInfo(alliance!.getId(), chr.getId()), -1, -1);
                break;
            case 0x02:
                {
                    // Leave Alliance
                    if (chr.AllianceModel == null || chr.GuildRank != 1)
                    {
                        return;
                    }

                    chr.AllianceModel.RemoveGuildFromAlliance(chr.GuildId, 1);
                    break;
                }
            case 0x03: // Send Invite
                string guildName = p.readString();

                if (alliance!.getGuilds().Count == alliance.getCapacity())
                {
                    chr.dropMessage(5, "Your alliance cannot comport any more guilds at the moment.");
                }
                else
                {
                    AllianceManager.sendInvitation(c, guildName, alliance.getId());
                }

                break;
            case 0x04:
                {
                    // Accept Invite
                    if (chrGuild.AllianceId != 0 || chr.GuildRank != 1)
                    {
                        return;
                    }

                    int allianceid = p.readInt();
                    //slea.readMapleAsciiString();  //recruiter's guild name

                    alliance = AllAllianceStorage.GetAllianceById(allianceid);
                    if (alliance == null)
                    {
                        return;
                    }

                    if (!AllianceManager.answerInvitation(c.OnlinedCharacter.getId(), chrGuild.getName(), alliance.getId(), true))
                    {
                        return;
                    }

                    if (alliance.getGuilds().Count == alliance.getCapacity())
                    {
                        chr.dropMessage(5, "Your alliance cannot comport any more guilds at the moment.");
                        return;
                    }

                    int guildid = chr.getGuildId();

                    alliance.AddGuild(guildid);
                    chrGuild.resetAllianceGuildPlayersRank();

                    chr.setAllianceRank(2);
                    chr.saveGuildStatus();

                    alliance.broadcastMessage(GuildPackets.addGuildToAlliance(alliance, chrGuild, c), -1, -1);
                    alliance.broadcastMessage(GuildPackets.updateAllianceInfo(alliance), -1, -1);
                    alliance.broadcastMessage(GuildPackets.allianceNotice(alliance.getId(), alliance.getNotice()), -1, -1);
                    chrGuild.dropMessage("Your guild has joined the [" + alliance.getName() + "] union.");

                    break;
                }
            case 0x06:
                {
                    // Expel Guild
                    int guildid = p.readInt();
                    int allianceid = p.readInt();
                    if (chrGuild.AllianceId == 0 || chrGuild.AllianceId != allianceid)
                    {
                        return;
                    }

                    alliance!.RemoveGuildFromAlliance(guildid, 2);
                    break;
                }
            case 0x07:
                {
                    // Change Alliance Leader
                    if (chrGuild.AllianceId == 0 || chr.GuildId < 1)
                    {
                        return;
                    }
                    int victimid = p.readInt();
                    var player = c.getWorldServer().getPlayerStorage().getCharacterById(victimid);
                    if (player == null || !player.IsOnlined || player.AllianceRank != 2)
                    {
                        return;
                    }

                    //NewServer.getInstance().allianceMessage(alliance.getId(), sendChangeLeader(chr.getGuild().getAllianceId(), chr.getId(), slea.readInt()), -1, -1);
                    changeLeaderAllianceRank(alliance!, player);
                    break;
                }
            case 0x08:
                string[] ranks = new string[5];
                for (int i = 0; i < 5; i++)
                {
                    ranks[i] = p.readString();
                }
                alliance.setRankTitle(ranks);
                alliance.broadcastMessage(GuildPackets.changeAllianceRankTitle(alliance.getId(), ranks), -1, -1);
                break;
            case 0x09:
                {
                    int int1 = p.readInt();
                    sbyte byte1 = p.ReadSByte();

                    //NewServer.getInstance().allianceMessage(alliance.getId(), sendChangeRank(chr.getGuild().getAllianceId(), chr.getId(), int1, byte1), -1, -1);
                    var player = Server.getInstance().getWorld(c.getWorld()).getPlayerStorage().getCharacterById(int1);
                    changePlayerAllianceRank(alliance!, player, (byte1 > 0));

                    break;
                }
            case 0x0A:
                string notice = p.readString();
                alliance.setNotice(notice);
                alliance.broadcastMessage(GuildPackets.allianceNotice(alliance.getId(), notice), -1, -1);

                alliance.dropMessage(5, "* Alliance Notice : " + notice);
                break;
            default:
                chr.dropMessage("Feature not available");
                break;
        }

        alliance?.saveToDB();
    }

    private void changeLeaderAllianceRank(IAlliance alliance, IPlayer newLeader)
    {
        var oldLeader = alliance.getLeader();
        oldLeader.setAllianceRank(2);
        oldLeader.saveGuildStatus();

        newLeader.setAllianceRank(1);
        newLeader.saveGuildStatus();

        alliance.broadcastMessage(GuildPackets.getGuildAlliances(alliance), -1, -1);
        alliance.dropMessage("'" + newLeader.Name + "' has been appointed as the new head of this Alliance.");
    }

    private void changePlayerAllianceRank(IAlliance alliance, IPlayer chr, bool raise)
    {
        int newRank = chr.getAllianceRank() + (raise ? -1 : 1);
        if (newRank < 3 || newRank > 5)
        {
            return;
        }

        chr.setAllianceRank(newRank);
        chr.saveGuildStatus();

        alliance.broadcastMessage(GuildPackets.getGuildAlliances(alliance), -1, -1);
        alliance.dropMessage("'" + chr.getName() + "' has been reassigned to '" + alliance.getRankTitle(newRank) + "' in this Alliance.");
    }

}
