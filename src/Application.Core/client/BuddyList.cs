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


using Application.Core.Game.TheWorld;
using Microsoft.EntityFrameworkCore;
using net.packet;
using System.Collections.Concurrent;
using tools;

namespace client;


public class BuddyList
{
    public enum BuddyOperation
    {
        ADDED, DELETED
    }

    public enum BuddyAddResult
    {
        BUDDYLIST_FULL, ALREADY_ON_LIST, OK
    }

    private ConcurrentDictionary<int, BuddylistEntry> buddies = new();
    private int capacity;
    private ConcurrentQueue<CharacterNameAndId> _pendingRequests = new();

    public BuddyList(int capacity)
    {
        this.capacity = capacity;
    }

    public bool contains(int characterId)
    {
        lock (buddies)
        {
            return buddies.ContainsKey(characterId);
        }
    }

    public bool containsVisible(int characterId)
    {
        BuddylistEntry? ble = buddies.GetValueOrDefault(characterId);

        if (ble == null)
        {
            return false;
        }
        return ble.isVisible();

    }

    public int getCapacity()
    {
        return capacity;
    }

    public void setCapacity(int capacity)
    {
        this.capacity = capacity;
    }

    public BuddylistEntry? get(int characterId)
    {
        return buddies.GetValueOrDefault(characterId);
    }

    public BuddylistEntry? get(string characterName)
    {
        string lowerCaseName = characterName.ToLower();
        foreach (BuddylistEntry ble in getBuddies())
        {
            if (ble.getName().ToLower().Equals(lowerCaseName))
            {
                return ble;
            }
        }

        return null;
    }

    public void put(BuddylistEntry entry)
    {
        buddies.AddOrUpdate(entry.getCharacterId(), entry);
    }

    public void remove(int characterId)
    {
        buddies.Remove(characterId);
    }

    public ICollection<BuddylistEntry> getBuddies()
    {
        return buddies.Values.ToList();
    }

    public bool isFull()
    {
        lock (buddies)
        {
            return buddies.Count >= capacity;
        }
    }

    public int[] getBuddyIds()
    {
        lock (buddies)
        {
            int[] buddyIds = new int[buddies.Count];
            int i = 0;
            foreach (BuddylistEntry ble in buddies.Values)
            {
                buddyIds[i++] = ble.getCharacterId();
            }
            return buddyIds;
        }
    }

    public void broadcast(Packet packet, WorldPlayerStorage pstorage)
    {
        foreach (int bid in getBuddyIds())
        {
            var chr = pstorage.getCharacterById(bid);

            if (chr != null && chr.isLoggedinWorld())
            {
                chr.sendPacket(packet);
            }
        }
    }

    public void loadFromDb(int characterId)
    {
        using var dbContext = new DBContext();
        var dbList = (from a in dbContext.Buddies
                      join b in dbContext.Characters on a.BuddyId equals b.Id
                      where a.CharacterId == characterId
                      select new { a.BuddyId, BuddyName = b.Name, a.Pending, a.Group }).ToList();
        dbList.ForEach(x =>
        {
            if (x.Pending == 1)
                _pendingRequests.Enqueue(new CharacterNameAndId(x.BuddyId, x.BuddyName));
            else
                put(new BuddylistEntry(x.BuddyName, x.Group, x.BuddyId, -1, true));
        });
        dbContext.Buddies.Where(x => x.CharacterId == characterId && x.Pending == 1).ExecuteDelete();
    }

    public CharacterNameAndId? pollPendingRequest()
    {
        if (_pendingRequests.TryDequeue(out var d))
            return d;
        return null;
    }

    public void addBuddyRequest(IClient c, int cidFrom, string nameFrom, int channelFrom)
    {
        put(new BuddylistEntry(nameFrom, "Default Group", cidFrom, channelFrom, false));
        if (_pendingRequests.Count == 0)
        {
            c.sendPacket(PacketCreator.requestBuddylistAdd(cidFrom, c.OnlinedCharacter.getId(), nameFrom));
        }
        else
        {
            _pendingRequests.Enqueue(new CharacterNameAndId(cidFrom, nameFrom));
        }
    }
}
