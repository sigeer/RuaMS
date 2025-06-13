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
using Application.Core.Channel.Services;
using Application.Shared.Invitations;
using net.server.world;
using tools;

public class MessengerHandler : ChannelHandlerBase
{
    readonly MessengerService _messengerService;

    public MessengerHandler(MessengerService messengerService)
    {
        _messengerService = messengerService;
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
                var messenger = player.Messenger;
                switch (mode)
                {
                    case 0x00:
                        int messengerid = p.readInt();
                        if (messenger == null)
                        {
                            if (messengerid == 0)
                            {
                                InviteType.MESSENGER.RemoveRequest(player.getId());

                                MessengerCharacter messengerplayer = new MessengerCharacter(player, 0);
                                messenger = world.createMessenger(messengerplayer);
                                player.setMessenger(messenger);
                                player.setMessengerPosition(0);
                            }
                            else
                            {
                                messenger = world.getMessenger(messengerid);
                                if (messenger != null)
                                {
                                    InviteResult inviteRes = InviteType.MESSENGER.AnswerInvite(player.getId(), messengerid, true);
                                    InviteResultType res = inviteRes.Result;
                                    if (res == InviteResultType.ACCEPTED)
                                    {
                                        int position = messenger.getLowestPosition();
                                        MessengerCharacter messengerplayer = new MessengerCharacter(player, position);
                                        if (messenger.getMembers().Count < 3)
                                        {
                                            player.setMessenger(messenger);
                                            player.setMessengerPosition(position);
                                            world.joinMessenger(messenger.getId(), messengerplayer, player.getName(), messengerplayer.getChannel());
                                        }
                                    }
                                    else
                                    {
                                        player.message("Could not verify your Maple Messenger accept since the invitation rescinded.");
                                    }
                                }
                            }
                        }
                        else
                        {
                            _messengerService.AnswerInvite(player, false);
                        }
                        break;
                    case 0x02:
                        player.closePlayerMessenger();
                        break;
                    case 0x03:
                        if (messenger == null)
                        {
                            c.sendPacket(PacketCreator.messengerChat(player.getName() + " : This Maple Messenger is currently unavailable. Please quit this chat."));
                        }
                        else if (messenger.getMembers().Count < 3)
                        {
                            input = p.readString();
                            _messengerService.CreateInvite(player, input);
                            var target = c.CurrentServer.getPlayerStorage().getCharacterByName(input);
                            if (target != null)
                            {
                                if (target.Messenger == null)
                                {
                                    if (InviteType.MESSENGER.CreateInvite(new ChatInviteRequest(c.OnlinedCharacter, target, messenger.getId())))
                                    {
                                        target.sendPacket(PacketCreator.messengerInvite(c.OnlinedCharacter.getName(), messenger.getId()));
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
                                    world.messengerInvite(c.OnlinedCharacter.getName(), messenger.getId(), input, c.Channel);
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
                        if (messenger != null)
                        {
                            MessengerCharacter messengerplayer = new MessengerCharacter(player, player.MessengerPosition);
                            input = p.readString();
                            world.messengerChat(messenger, input, messengerplayer.getName());
                        }
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
