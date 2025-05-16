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
using Application.EF;
using Application.EF.Entities;
using client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using net.packet;
using tools;
using static Application.Core.Game.Relation.BuddyList;
using static Application.Core.Game.Relation.BuddyList.BuddyOperation;

namespace Application.Core.Channel.Net.Handlers;

public class BuddylistModifyHandler : ChannelHandlerBase
{
    readonly ILogger<BuddylistModifyHandler> _logger;

    public BuddylistModifyHandler(ILogger<BuddylistModifyHandler> logger)
    {
        _logger = logger;
    }

    private record CharacterIdNameBuddyCapacity : CharacterNameAndId
    {
        private int buddyCapacity;

        public CharacterIdNameBuddyCapacity(int id, string name, int buddyCapacity) : base(id, name)
        {

            this.buddyCapacity = buddyCapacity;
        }

        public int getBuddyCapacity()
        {
            return buddyCapacity;
        }
    }

    private void nextPendingRequest(IChannelClient c)
    {
        var pendingBuddyRequest = c.OnlinedCharacter.getBuddylist().pollPendingRequest();
        if (pendingBuddyRequest != null)
        {
            c.sendPacket(PacketCreator.requestBuddylistAdd(pendingBuddyRequest.id, c.OnlinedCharacter.getId(), pendingBuddyRequest.name));
        }
    }

    private CharacterIdNameBuddyCapacity? getCharacterIdAndNameFromDatabase(string name)
    {
        var p = AllPlayerStorage.GetOrAddCharacterByName(name);
        return p == null ? null : new CharacterIdNameBuddyCapacity(p.Id, p.Name, p.BuddyList.Capacity);
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        int mode = p.readByte();
        var player = c.OnlinedCharacter;
        BuddyList buddylist = player.getBuddylist();
        using var dbContext = new DBContext();
        if (mode == 1)
        {
            // add
            string addName = p.readString();
            string group = p.readString();
            if (group.Length > 16 || addName.Length < 4 || addName.Length > 13)
            {
                return; //hax.
            }
            var ble = buddylist.get(addName);
            if (ble != null && !ble.Visible && group.Equals(ble.Group))
            {
                c.sendPacket(PacketCreator.serverNotice(1, "You already have \"" + ble.getName() + "\" on your Buddylist"));
            }
            else if (buddylist.isFull() && ble == null)
            {
                c.sendPacket(PacketCreator.serverNotice(1, "Your buddylist is already full"));
            }
            else if (ble == null)
            {
                try
                {
                    var world = c.getWorldServer();
                    CharacterIdNameBuddyCapacity? charWithId;
                    int channel;
                    var otherChar = c.CurrentServer.getPlayerStorage().getCharacterByName(addName);
                    if (otherChar != null)
                    {
                        channel = c.CurrentServer.getId();
                        charWithId = new CharacterIdNameBuddyCapacity(otherChar.getId(), otherChar.getName(), otherChar.BuddyList.Capacity);
                    }
                    else
                    {
                        channel = world.find(addName);
                        charWithId = getCharacterIdAndNameFromDatabase(addName);
                    }
                    if (charWithId != null)
                    {
                        BuddyAddResult buddyAddResult = BuddyAddResult.OK;
                        if (channel != -1)
                        {
                            buddyAddResult = world.requestBuddyAdd(addName, c.CurrentServer.getId(), player.getId(), player.getName());
                        }
                        else
                        {

                            var countOfBuddy = dbContext.Buddies.Where(x => x.CharacterId == charWithId.id && x.Pending == 0).Count();
                            if (countOfBuddy >= charWithId.getBuddyCapacity())
                            {
                                buddyAddResult = BuddyAddResult.BUDDYLIST_FULL;
                            }

                            var statusModel = dbContext.Buddies.Where(x => x.CharacterId == charWithId.id && x.BuddyId == player.getId()).Select(x => new { x.Pending }).FirstOrDefault();
                            if (statusModel != null)
                            {
                                buddyAddResult = BuddyAddResult.ALREADY_ON_LIST;
                            }
                        }

                        if (buddyAddResult == BuddyAddResult.BUDDYLIST_FULL)
                        {
                            c.sendPacket(PacketCreator.serverNotice(1, "\"" + addName + "\"'s Buddylist is full"));
                        }
                        else
                        {
                            int displayChannel = -1;
                            if (buddyAddResult == BuddyAddResult.ALREADY_ON_LIST && channel != -1)
                            {
                                displayChannel = channel;
                                notifyRemoteChannel(c, channel, charWithId.id, ADDED);
                            }
                            else if (buddyAddResult != BuddyAddResult.ALREADY_ON_LIST && channel == -1)
                            {
                                dbContext.Buddies.Add(new BuddyEntity
                                {
                                    CharacterId = charWithId.id,
                                    BuddyId = player.getId(),
                                    Pending = 1
                                });
                                dbContext.SaveChanges();
                            }
                            buddylist.put(group, charWithId.id);
                            c.sendPacket(PacketCreator.updateBuddylist(buddylist.getBuddies()));
                        }
                    }
                    else
                    {
                        c.sendPacket(PacketCreator.serverNotice(1, "A character called \"" + addName + "\" does not exist"));
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                }
            }
            else
            {
                ble.Group = group;
                c.sendPacket(PacketCreator.updateBuddylist(buddylist.getBuddies()));
            }
        }
        else if (mode == 2)
        {
            // accept buddy
            int otherCid = p.readInt();
            if (!buddylist.isFull())
            {
                try
                {
                    int channel = c.getWorldServer().find(otherCid);//worldInterface.find(otherCid);
                    string? otherName = null;
                    var otherChar = c.CurrentServer.getPlayerStorage().getCharacterById(otherCid);
                    if (otherChar == null)
                    {
                        otherName = dbContext.Characters.Where(x => x.Id == otherCid).Select(x => new { x.Name }).FirstOrDefault()?.Name;
                    }
                    else
                    {
                        otherName = otherChar.getName();
                    }
                    if (otherName != null)
                    {
                        buddylist.put("Default Group", otherCid);
                        c.sendPacket(PacketCreator.updateBuddylist(buddylist.getBuddies()));
                        notifyRemoteChannel(c, channel, otherCid, ADDED);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                }
            }
            nextPendingRequest(c);
        }
        else if (mode == 3)
        { // delete
            int otherCid = p.readInt();
            player.deleteBuddy(otherCid);
        }
    }

    private void notifyRemoteChannel(IChannelClient c, int remoteChannel, int otherCid, BuddyOperation operation)
    {
        var player = c.OnlinedCharacter;
        if (remoteChannel != -1)
        {
            c.getWorldServer().buddyChanged(otherCid, player.getId(), player.getName(), c.CurrentServer.getId(), operation);
        }
    }
}
