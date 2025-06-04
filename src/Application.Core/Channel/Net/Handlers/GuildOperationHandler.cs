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


using Application.Core.constants;
using Application.Core.Game.Players;
using Application.Core.Managers;
using Application.Utility.Configs;
using constants.game;
using Microsoft.Extensions.Logging;
using net.server.coordinator.matchchecker;
using net.server.guild;
using tools;

namespace Application.Core.Channel.Net.Handlers;

public class GuildOperationHandler : ChannelHandlerBase
{
    readonly ILogger<GuildOperationHandler> _logger;

    public GuildOperationHandler(ILogger<GuildOperationHandler> logger)
    {
        _logger = logger;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var mc = c.OnlinedCharacter;
        byte type = p.readByte();
        switch (type)
        {
            case 0x00:
                //c.sendPacket(PacketCreator.showGuildInfo(mc));
                break;
            case 0x02:
                if (mc.GuildId > 0)
                {
                    mc.dropMessage(1, "You cannot create a new Guild while in one.");
                    return;
                }
                if (mc.getMeso() < YamlConfig.config.server.CREATE_GUILD_COST)
                {
                    mc.dropMessage(1, "You do not have " + GameConstants.numberWithCommas(YamlConfig.config.server.CREATE_GUILD_COST) + " mesos to create a Guild.");
                    return;
                }
                string guildName = p.readString();
                if (!GuildManager.CheckGuildName(guildName))
                {
                    mc.dropMessage(1, "The Guild name you have chosen is not accepted.");
                    return;
                }

                HashSet<IPlayer> eligibleMembers = new(GuildManager.getEligiblePlayersForGuild(mc));
                if (eligibleMembers.Count < YamlConfig.config.server.CREATE_GUILD_MIN_PARTNERS)
                {
                    if (mc.getMap().getAllPlayers().Count < YamlConfig.config.server.CREATE_GUILD_MIN_PARTNERS)
                    {
                        // thanks NovaStory for noticing message in need of smoother info
                        mc.dropMessage(1, "Your Guild doesn't have enough cofounders present here and therefore cannot be created at this time.");
                    }
                    else
                    {
                        // players may be unaware of not belonging on a party in order to become eligible, thanks Hair (Legalize) for pointing this out
                        mc.dropMessage(1, "Please make sure everyone you are trying to invite is neither on a guild nor on a party.");
                    }

                    return;
                }

                if (!c.CurrentServer.TeamManager.CreateParty(mc, true))
                {
                    mc.dropMessage(1, "You cannot create a new Guild while in a party.");
                    return;
                }

                var eligibleCids = eligibleMembers.Select(x => x.Id).ToHashSet();

                c.getWorldServer().getMatchCheckerCoordinator().createMatchConfirmation(MatchCheckerType.GUILD_CREATION, 0, mc.getId(), eligibleCids, guildName);
                break;
            case 0x05:
                if (mc.GuildId <= 0 || mc.GuildRank > 2)
                {
                    return;
                }

                string targetName = p.readString();
                var mgr = GuildManager.SendInvitation(c, targetName);
                if (mgr != null)
                {
                    c.sendPacket(mgr.Value.getPacket(targetName));
                }
                else
                {
                } // already sent invitation, do nothing

                break;
            case 0x06:
                if (mc.GuildId > 0)
                {
                    _logger.LogWarning("[Hack] Chr {CharacterName} attempted to join a guild when s/he is already in one.", mc.getName());
                    return;
                }
                int gid = p.readInt();
                int cid = p.readInt();
                if (cid != mc.getId())
                {
                    _logger.LogWarning("[Hack] Chr {CharacterName} attempted to join a guild with a different chrId", mc.getName());
                    return;
                }

                if (!GuildManager.AnswerInvitation(mc, gid, true))
                {
                    return;
                }

                mc.GuildId = gid;
                mc.GuildRank = 5;
                mc.AllianceRank = 5;

                int s = mc.GuildModel!.addGuildMember(mc);
                if (s == 0)
                {
                    mc.dropMessage(1, "The guild you are trying to join is already full.");
                    mc.GuildId = Defaults.GuildId;
                    return;
                }

                c.sendPacket(GuildPackets.showGuildInfo(mc));

                if (mc.AllianceModel != null)
                {
                    mc.AllianceModel.updateAlliancePackets(mc);
                }

                mc.saveGuildStatus(); // update database
                mc.getMap().broadcastPacket(mc, GuildPackets.guildNameChanged(mc.getId(), mc.GuildModel!.Name));
                mc.getMap().broadcastPacket(mc, GuildPackets.guildMarkChanged(mc.getId(), mc.GuildModel));
                break;
            case 0x07:
                cid = p.readInt();
                string name = p.readString();
                if (cid != mc.getId() || !name.Equals(mc.getName()) || mc.GuildModel == null)
                {
                    _logger.LogWarning("[Hack] Chr {CharacterName} tried to quit guild under the name {GuildName} and current guild id of {GuildId}", mc.getName(), name, mc.getGuildId());
                    return;
                }


                c.sendPacket(GuildPackets.updateGP(mc.GuildId, 0));
                mc.GuildModel!.leaveGuild(mc);

                c.sendPacket(GuildPackets.showGuildInfo(null));
                if (mc.AllianceModel != null)
                {
                    mc.AllianceModel.updateAlliancePackets(mc);
                }

                mc.saveGuildStatus();
                mc.getMap().broadcastPacket(mc, GuildPackets.guildNameChanged(mc.getId(), ""));
                break;
            case 0x08:
                cid = p.readInt();
                name = p.readString();
                if (mc.GuildRank > 2 || mc.GuildId <= 0)
                {
                    _logger.LogWarning("[Hack] Chr {CharacterName} is trying to expel without rank 1 or 2", mc.getName());
                    return;
                }

                mc.GuildModel?.expelMember(mc, name, cid);

                if (mc.AllianceModel != null)
                {
                    mc.AllianceModel!.updateAlliancePackets(mc);
                }
                break;
            case 0x0d:
                if (mc.GuildModel == null || mc.getGuildRank() != 1)
                {
                    _logger.LogWarning("[Hack] Chr {CharacterName} tried to change guild rank titles when s/he does not have permission", mc.getName());
                    return;
                }
                string[] ranks = new string[5];
                for (int i = 0; i < 5; i++)
                {
                    ranks[i] = p.readString();
                }

                mc.GuildModel.changeRankTitle(ranks);
                break;
            case 0x0e:
                cid = p.readInt();
                byte newRank = p.readByte();
                if (mc.getGuildRank() > 2 || (newRank <= 2 && mc.getGuildRank() != 1) || mc.GuildModel == null)
                {
                    _logger.LogWarning("[Hack] Chr {CharacterName} is trying to change rank outside of his/her permissions.", mc.getName());
                    return;
                }
                if (newRank <= 1 || newRank > 5)
                {
                    return;
                }
                mc.GuildModel.changeRank(cid, newRank);
                break;
            case 0x0f:
                if (mc.GuildModel == null || mc.GuildRank != 1 || mc.getMapId() != MapId.GUILD_HQ)
                {
                    _logger.LogWarning("[Hack] Chr {CharacterName} tried to change guild emblem without being the guild leader", mc.getName());
                    return;
                }
                if (mc.getMeso() < YamlConfig.config.server.CHANGE_EMBLEM_COST)
                {
                    c.sendPacket(PacketCreator.serverNotice(1, "You do not have " + GameConstants.numberWithCommas(YamlConfig.config.server.CHANGE_EMBLEM_COST) + " mesos to change the Guild emblem."));
                    return;
                }
                short bg = p.readShort();
                byte bgcolor = p.readByte();
                short logo = p.readShort();
                byte logocolor = p.readByte();
                mc.GuildModel.setGuildEmblem(bg, bgcolor, logo, logocolor);

                if (mc.AllianceModel != null)
                {
                    mc.AllianceModel.broadcastMessage(GuildPackets.getGuildAlliances(mc.AllianceModel), -1, -1);
                }

                mc.gainMeso(-YamlConfig.config.server.CHANGE_EMBLEM_COST, true, false, true);
                mc.GuildModel!.broadcastEmblemChanged();
                mc.GuildModel!.broadcastNameChanged();
                break;
            case 0x10:
                if (mc.GuildModel == null || mc.GuildRank > 2)
                {
                    if (mc.GuildModel == null)
                    {
                        _logger.LogWarning("[Hack] Chr {CharacterName} tried to change guild notice while not in a guild", mc.Name);
                    }
                    return;
                }
                string notice = p.readString();
                if (notice.Length > 100)
                {
                    return;
                }
                mc.GuildModel.setGuildNotice(notice);
                break;
            case 0x1E:
                p.readInt();
                var wserv = c.getWorldServer();

                if (mc.getParty() != null)
                {
                    wserv.getMatchCheckerCoordinator().dismissMatchConfirmation(mc.getId());
                    return;
                }

                int leaderid = wserv.getMatchCheckerCoordinator().getMatchConfirmationLeaderid(mc.getId());
                if (leaderid != -1)
                {
                    bool result = p.readByte() != 0;
                    if (result && wserv.getMatchCheckerCoordinator().isMatchConfirmationActive(mc.getId()))
                    {
                        var leader = wserv.getPlayerStorage().getCharacterById(leaderid);
                        if (leader != null)
                        {
                            int partyid = leader.getPartyId();
                            if (partyid != -1)
                            {
                                c.CurrentServer.TeamManager.JoinParty(mc, partyid, true);    // GMS gimmick "party to form guild" recalled thanks to Vcoc
                            }
                        }
                    }

                    wserv.getMatchCheckerCoordinator().answerMatchConfirmation(mc.getId(), result);
                }

                break;
            default:
                _logger.LogWarning("Unhandled GUILD_OPERATION packet: {Packet}", p);
                break;
        }
    }
}
