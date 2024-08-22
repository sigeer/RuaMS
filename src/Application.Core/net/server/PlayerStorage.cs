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

namespace net.server;


public class PlayerStorage
{
    private Dictionary<int, Character> storage = new();
    private Dictionary<string, Character> nameStorage = new();
    private ReaderWriterLockSlim locks = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    public PlayerStorage()
    {

    }

    public void addPlayer(Character chr)
    {
        locks.EnterWriteLock();
        try
        {
            storage.AddOrUpdate(chr.getId(), chr);
            nameStorage.AddOrUpdate(chr.getName().ToLower(), chr);
        }
        finally
        {
            locks.ExitWriteLock();
        }
    }

    public Character? removePlayer(int chr)
    {
        locks.EnterWriteLock();
        try
        {
            storage.Remove(chr, out var mc);
            if (mc != null)
                nameStorage.Remove(mc.getName().ToLower());

            return mc;
        }
        finally
        {
            locks.ExitWriteLock();
        }
    }

    public Character? getCharacterByName(string name)
    {
        locks.EnterReadLock();
        try
        {
            return nameStorage.GetValueOrDefault(name.ToLower());
        }
        finally
        {
            locks.ExitReadLock();
        }
    }

    public Character? getCharacterById(int id)
    {
        locks.EnterReadLock();
        try
        {
            return storage.GetValueOrDefault(id);
        }
        finally
        {
            locks.ExitReadLock();
        }
    }

    public ICollection<Character> getAllCharacters()
    {
        locks.EnterReadLock();
        try
        {
            return new List<Character>(storage.Values);
        }
        finally
        {
            locks.ExitReadLock();
        }
    }

    public void disconnectAll()
    {
        List<Character> chrList;
        locks.EnterReadLock();
        try
        {
            chrList = new(storage.Values);
        }
        finally
        {
            locks.ExitReadLock();
        }

        foreach (Character mc in chrList)
        {
            Client client = mc.getClient();
            if (client != null)
            {
                client.forceDisconnect();
            }
        }

        locks.EnterWriteLock();
        try
        {
            storage.Clear();
        }
        finally
        {
            locks.ExitWriteLock();
        }
    }

    public int getSize()
    {
        locks.EnterReadLock();
        try
        {
            return storage.Count;
        }
        finally
        {
            locks.ExitReadLock();
        }
    }
}
