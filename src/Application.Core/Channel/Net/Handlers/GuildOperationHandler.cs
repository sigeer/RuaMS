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
using Application.Resources.Messages;
using Microsoft.Extensions.Logging;

namespace Application.Core.Channel.Net.Handlers;

public class GuildOperationHandler : ChannelHandlerBase
{
    readonly ILogger<GuildOperationHandler> _logger;
    readonly GuildManager _guildManager;

    public GuildOperationHandler(ILogger<GuildOperationHandler> logger, GuildManager guildManager)
    {
        _logger = logger;
        _guildManager = guildManager;
    }

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        var mc = c.OnlinedCharacter;
        byte type = p.readByte();
        switch (type)
        {
            case 0x00:
                //c.sendPacket(PacketCreator.showGuildInfo(mc));
                break;
            case 0x02:
                string guildName = p.readString();
                await _guildManager.CreateGuild(mc, guildName);
                break;
            case 0x05:
                if (mc.GuildId <= 0 || mc.GuildRank > 2)
                {
                    return;
                }

                string targetName = p.readString();
                await _guildManager.SendInvitation(c, targetName);
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

                await _guildManager.AnswerInvitation(mc, gid, true);
                break;
            case 0x07:
                cid = p.readInt();
                string name = p.readString();
                if (cid != mc.getId() || !name.Equals(mc.getName()))
                {
                    _logger.LogWarning("[Hack] Chr {CharacterName} tried to quit guild under the name {GuildName} and current guild id of {GuildId}", mc.getName(), name, mc.getGuildId());
                    return;
                }

                await _guildManager.LeaveMember(mc);
                break;
            case 0x08:
                cid = p.readInt();
                name = p.readString();

                await _guildManager.ExpelMember(mc, cid);
                break;
            case 0x0d:
                if (mc.GuildRank != 1)
                {
                    _logger.LogWarning("[Hack] Chr {CharacterName} tried to change guild rank titles when s/he does not have permission", mc.getName());
                    return;
                }
                string[] ranks = new string[5];
                for (int i = 0; i < 5; i++)
                {
                    ranks[i] = p.readString();
                }

                await _guildManager.SetGuildRankTitle(mc, ranks);
                break;
            case 0x0e:
                cid = p.readInt();
                byte newRank = p.readByte();
                if (mc.getGuildRank() > 2 || (newRank <= 2 && mc.getGuildRank() != 1))
                {
                    _logger.LogWarning("[Hack] Chr {CharacterName} is trying to change rank outside of his/her permissions.", mc.getName());
                    return;
                }
                if (newRank <= 1 || newRank > 5)
                {
                    return;
                }
                await _guildManager.ChangeRank(c.OnlinedCharacter, cid, newRank);
                break;
            case 0x0f:
                if (mc.GuildRank != 1 || mc.getMapId() != MapId.GUILD_HQ)
                {
                    _logger.LogWarning("[Hack] Chr {CharacterName} tried to change guild emblem without being the guild leader", mc.getName());
                    return;
                }
                if (mc.getMeso() < YamlConfig.config.server.CHANGE_EMBLEM_COST)
                {
                    mc.Popup(nameof(ClientMessage.Guild_ChangeEmblemFail_Meso), mc.Client.CurrentCulture.Number(YamlConfig.config.server.CHANGE_EMBLEM_COST).ToString());
                    return;
                }
                short bg = p.readShort();
                byte bgcolor = p.readByte();
                short logo = p.readShort();
                byte logocolor = p.readByte();

                mc.GainMeso(-YamlConfig.config.server.CHANGE_EMBLEM_COST, inChat: true);
                await _guildManager.SetGuildEmblem(mc, bg, bgcolor, logo, logocolor);


                break;
            case 0x10:
                string notice = p.readString();
                if (notice.Length > 100)
                {
                    return;
                }
                await _guildManager.SetGuildNotice(mc, notice);
                break;
            case 0x1E:
                _logger.LogWarning("功能移除：回复家族发起");

                break;
            default:
                _logger.LogWarning("Unhandled GUILD_OPERATION packet: {Packet}", p);
                break;
        }
    }
}
