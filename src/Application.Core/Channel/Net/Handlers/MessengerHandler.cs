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
using Application.Core.Game.Invites;
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
                                InviteType.MESSENGER.RemoveRequest(player.getId());

                                _chatRoomService.CreateChatRoom(player);
                            }
                            else
                            {
                                InviteResult inviteRes = InviteType.MESSENGER.AnswerInvite(player.getId(), messengerid, true);
                                InviteResultType res = inviteRes.Result;
                                if (res == InviteResultType.ACCEPTED)
                                {
                                    _chatRoomService.JoinChatRoom(player, messengerid);
                                }
                                else
                                {
                                    player.message("Could not verify your Maple Messenger accept since the invitation rescinded.");
                                }
                            }
                        }
                        else
                        {
                            InviteType.MESSENGER.AnswerInvite(player.getId(), messengerid, false);
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
                            var target = c.CurrentServer.getPlayerStorage().getCharacterByName(input);
                            if (target != null)
                            {
                                if (target.ChatRoomId == 0)
                                {
                                    if (InviteType.MESSENGER.CreateInvite(new ChatInviteRequest(c.OnlinedCharacter, target, player.ChatRoomId)))
                                    {
                                        target.sendPacket(PacketCreator.messengerInvite(c.OnlinedCharacter.getName(), player.ChatRoomId));
                                        c.sendPacket(PacketCreator.messengerNote(input, 4, 1));
                                    }
                                    else
                                    {
                                        c.sendPacket(PacketCreator.messengerChat(player.getName() + " : " + input + " is already managing a Maple Messenger invitation"));
                                    }
                                }
                                else
                                {
                                    c.sendPacket(PacketCreator.messengerChat(player.getName() + " : " + input + " is already using Maple Messenger"));
                                }
                            }
                            else
                            {
                                if (world.find(input) > -1)
                                {
                                    world.messengerInvite(c.OnlinedCharacter.getName(), player.ChatRoomId, input, c.Channel);
                                }
                                else
                                {
                                    c.sendPacket(PacketCreator.messengerNote(input, 4, 0));
                                }
                            }
                        }
                        else
                        {
                            c.sendPacket(PacketCreator.messengerChat(player.getName() + " : You cannot have more than 3 people in the Maple Messenger"));
                        }
                        break;
                    case 0x05:
                        string targeted = p.readString();
                        world.declineChat(targeted, player);
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
