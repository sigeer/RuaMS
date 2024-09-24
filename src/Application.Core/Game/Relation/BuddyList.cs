using client;
using Microsoft.EntityFrameworkCore;
using net.packet;
using System.Collections.Concurrent;
using tools;

namespace Application.Core.Game.Relation;

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

    public IPlayer Owner { get; set; }
    public int Capacity { get; set; }
    private ConcurrentDictionary<int, BuddylistEntry> buddies = new();
    private ConcurrentQueue<CharacterNameAndId> _pendingRequests = new();

    public BuddyList(IPlayer owner, int capacity)
    {
        Owner = owner;
        Capacity = capacity;
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
        return ble?.Visible ?? false;

    }

    public int getCapacity()
    {
        return Capacity;
    }

    public void setCapacity(int capacity)
    {
        Capacity = capacity;
    }

    public BuddylistEntry? get(int characterId)
    {
        return buddies.GetValueOrDefault(characterId);
    }

    public BuddylistEntry? get(string characterName)
    {
        return getBuddies().FirstOrDefault(x => x.getName().Equals(characterName, StringComparison.OrdinalIgnoreCase));
    }

    public void put(BuddylistEntry entry)
    {
        buddies.AddOrUpdate(entry.getCharacterId(), entry);
    }

    public void put(string group, int characterId, bool visible = true)
    {
        var entry = Owner.getWorldServer().Players.getCharacterById(characterId) ?? throw new BusinessCharacterNotFoundException(characterId);
        buddies.AddOrUpdate(characterId, new BuddylistEntry(entry, group, visible));
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
            return buddies.Count >= Capacity;
        }
    }

    public int[] getBuddyIds()
    {
        lock (buddies)
        {
            return buddies.Keys.ToArray();
        }
    }

    public void broadcast(Packet packet)
    {
        foreach (int bid in getBuddyIds())
        {
            var chr = Owner.getWorldServer().Players.getCharacterById(bid);

            if (chr != null && chr.isLoggedinWorld())
            {
                chr.sendPacket(packet);
            }
        }
    }

    public void LoadFromDb()
    {
        using var dbContext = new DBContext();
        var dbList = (from a in dbContext.Buddies
                      join b in dbContext.Characters on a.BuddyId equals b.Id
                      where a.CharacterId == Owner.Id
                      select new { a.BuddyId, BuddyName = b.Name, a.Pending, a.Group }).ToList();
        List<int> buddyPlayerList = dbList.Where(x => x.Pending != 1).Select(x => x.BuddyId).ToList();
        var buddies = Owner.getWorldServer().Players.GetPlayersByIds(buddyPlayerList);
        dbList.ForEach(x =>
        {
            if (x.Pending == 1)
                _pendingRequests.Enqueue(new CharacterNameAndId(x.BuddyId, x.BuddyName));
            else
            {
                var player = buddies.FirstOrDefault(y => y.Id == x.BuddyId);
                if (player != null)
                    put(new BuddylistEntry(player, x.Group, true));
            }
        });
        dbContext.Buddies.Where(x => x.CharacterId == Owner.Id && x.Pending == 1).ExecuteDelete();
    }

    public CharacterNameAndId? pollPendingRequest()
    {
        if (_pendingRequests.TryDequeue(out var d))
            return d;
        return null;
    }

    public void addBuddyRequest(IClient c, int cidFrom, string nameFrom, int channelFrom)
    {
        // 只是申请为什么要加数据？
        put("Default Group", cidFrom, false);
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
