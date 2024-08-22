/*
	This file is part of the OdinMS Maple Story Server
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


using client;
using client.autoban;
using net.packet;
using server;
using tools;
using static tools.PacketCreator;

namespace net.server.channel.handlers;

/**
 * @author Chronos
 */
public class WhisperHandler : AbstractPacketHandler
{
    // result types, not sure if there are proper names for these
    public static byte RT_ITC = 0x00;
    public static byte RT_SAME_CHANNEL = 0x01;
    public static byte RT_CASH_SHOP = 0x02;
    public static byte RT_DIFFERENT_CHANNEL = 0x03;

    public override void handlePacket(InPacket p, Client c)
    {
        byte request = p.readByte();
        string name = p.readString();
        var target = c.getWorldServer().getPlayerStorage().getCharacterByName(name);

        if (target == null)
        {
            c.sendPacket(PacketCreator.getWhisperResult(name, false));
            return;
        }

        switch (request)
        {
            case WhisperFlag.LOCATION | WhisperFlag.REQUEST:
                handleFind(c.getPlayer(), target, WhisperFlag.LOCATION);
                break;
            case WhisperFlag.WHISPER | WhisperFlag.REQUEST:
                string message = p.readString();
                handleWhisper(message, c.getPlayer(), target);
                break;
            case WhisperFlag.LOCATION_FRIEND | WhisperFlag.REQUEST:
                handleFind(c.getPlayer(), target, WhisperFlag.LOCATION_FRIEND);
                break;
            default:
                log.Warning("Unknown request {Request} triggered by {CharacterName}", request, c.getPlayer().getName());
                break;
        }
    }

    private void handleFind(Character user, Character target, byte flag)
    {
        if (user.gmLevel() >= target.gmLevel())
        {
            if (target.getCashShop().isOpened())
            {
                user.sendPacket(PacketCreator.getFindResult(target, RT_CASH_SHOP, -1, flag));
            }
            else if (target.getClient().getChannel() == user.getClient().getChannel())
            {
                user.sendPacket(PacketCreator.getFindResult(target, RT_SAME_CHANNEL, target.getMapId(), flag));
            }
            else
            {
                user.sendPacket(PacketCreator.getFindResult(target, RT_DIFFERENT_CHANNEL, target.getClient().getChannel() - 1, flag));
            }
        }
        else
        {
            // not found for whisper is the same message
            user.sendPacket(PacketCreator.getWhisperResult(target.getName(), false));
        }
    }

    private void handleWhisper(string message, Character user, Character target)
    {
        if (user.getAutobanManager().getLastSpam(7) + 200 > currentServerTime())
        {
            return;
        }
        user.getAutobanManager().spam(7);

        if (message.Length > sbyte.MaxValue)
        {
            AutobanFactory.PACKET_EDIT.alert(user, user.getName() + " tried to packet edit with whispers.");
            log.Warning("Chr {CharacterName} tried to send text with length of {MessageLength}", user.getName(), message.Length);
            user.getClient().disconnect(true, false);
            return;
        }

        ChatLogger.log(user.getClient(), "Whisper To " + target.getName(), message);

        target.sendPacket(PacketCreator.getWhisperReceive(user.getName(), user.getClient().getChannel() - 1, user.isGM(), message));

        bool hidden = target.isHidden() && target.gmLevel() > user.gmLevel();
        user.sendPacket(PacketCreator.getWhisperResult(target.getName(), !hidden));
    }
}
