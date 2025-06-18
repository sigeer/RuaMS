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


using Application.Core.Channel.Net;
using Application.Core.Channel.ServerData;
using tools;

public class MessengerHandler : ChannelHandlerBase
{
    readonly ChatRoomService _chatRoomService;

    public MessengerHandler(ChatRoomService chatRoomService)
    {
        _chatRoomService = chatRoomService;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        if (c.tryacquireClient())
        {
            try
            {
                string input;
                byte mode = p.readByte();
                var player = c.OnlinedCharacter;
                var world = c.getWorldServer();
                switch (mode)
                {
                    case 0x00:
                        int messengerid = p.readInt();
                        if (player.ChatRoomId == 0)
                        {
                            if (messengerid == 0)
                            {
                                _chatRoomService.CreateChatRoom(player);
                            }
                            else
                            {
                                _chatRoomService.AnswerInvite(player, messengerid, true);
                            }
                        }
                        else
                        {
                            _chatRoomService.AnswerInvite(player, messengerid, false);
                        }
                        break;
                    case 0x02:
                        _chatRoomService.LeftChatRoom(player);
                        break;
                    case 0x03:
                        if (player.ChatRoomId == 0)
                        {
                            c.sendPacket(PacketCreator.messengerChat(player.getName() + " : This Maple Messenger is currently unavailable. Please quit this chat."));
                        }
                        else if (true)
                        {
                            // messenger.getMembers().Count < 3
                            // 邀请放进MasterServer后再补上
                            input = p.readString();

                            _chatRoomService.CreateInvite(player, input);
                        }
                        else
                        {
                            c.sendPacket(PacketCreator.messengerChat(player.getName() + " : You cannot have more than 3 people in the Maple Messenger"));
                        }
                        break;
                    case 0x05:
                        // 拒绝邀请？
                        string targeted = p.readString();
                        // world.declineChat(targeted, player);
                        _chatRoomService.AnswerInvite(player, -1, false);
                        break;
                    case 0x06:
                        input = p.readString();
                        _chatRoomService.SendMessage(player, input);
                        break;
                }
            }
            finally
            {
                c.releaseClient();
            }
        }
    }
}
