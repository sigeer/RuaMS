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
using net.server.coordinator.matchchecker;
using scripting.Event;
using server.maps;
using server.partyquest;
using tools;
using static net.server.coordinator.matchchecker.MatchCheckerListenerFactory;

namespace net.server.world;

public class Party
{

    private int id;
    private Party enemy = null;
    private int leaderId;
    private List<PartyCharacter> members = new();
    private List<PartyCharacter> pqMembers = new List<PartyCharacter>();

    private Dictionary<int, int> histMembers = new();
    private int nextEntry = 0;

    private Dictionary<int, Door> doors = new();

    private object lockObj = new object();

    public Party(int id, PartyCharacter chrfor)
    {
        this.leaderId = chrfor.getId();
        this.id = id;
    }

    public bool containsMembers(PartyCharacter member)
    {
        Monitor.Enter(lockObj);
        try
        {
            return members.Contains(member);
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public void addMember(PartyCharacter member)
    {
        Monitor.Enter(lockObj);
        try
        {
            histMembers.AddOrUpdate(member.getId(), nextEntry);
            nextEntry++;

            members.Add(member);
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public void removeMember(PartyCharacter member)
    {
        Monitor.Enter(lockObj);
        try
        {
            histMembers.Remove(member.getId());

            members.Remove(member);
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public void setLeader(PartyCharacter victim)
    {
        this.leaderId = victim.getId();
    }

    public void updateMember(PartyCharacter member)
    {
        Monitor.Enter(lockObj);
        try
        {
            for (int i = 0; i < members.Count; i++)
            {
                if (members.get(i).getId() == member.getId())
                {
                    members.set(i, member);
                }
            }
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public PartyCharacter getMemberById(int id)
    {
        Monitor.Enter(lockObj);
        try
        {
            foreach (PartyCharacter chr in members)
            {
                if (chr.getId() == id)
                {
                    return chr;
                }
            }
            return null;
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public ICollection<PartyCharacter> getMembers()
    {
        Monitor.Enter(lockObj);
        try
        {
            return members.ToList();
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public List<PartyCharacter> getPartyMembers()
    {
        Monitor.Enter(lockObj);
        try
        {
            return new(members);
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public List<PartyCharacter> getPartyMembersOnline()
    {
        Monitor.Enter(lockObj);
        try
        {
            List<PartyCharacter> ret = new();

            foreach (PartyCharacter mpc in members)
            {
                if (mpc.isOnline())
                {
                    ret.Add(mpc);
                }
            }

            return ret;
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    // used whenever entering PQs: will draw every party member that can attempt a target PQ while ingnoring those unfit.
    public ICollection<PartyCharacter> getEligibleMembers()
    {
        return pqMembers.ToList();
    }

    public void setEligibleMembers(List<PartyCharacter> eliParty)
    {
        pqMembers = eliParty;
    }

    public int getId()
    {
        return id;
    }

    public void setId(int id)
    {
        this.id = id;
    }

    public int getLeaderId()
    {
        return leaderId;
    }

    public PartyCharacter? getLeader()
    {
        Monitor.Enter(lockObj);
        try
        {
            foreach (PartyCharacter mpc in members)
            {
                if (mpc.getId() == leaderId)
                {
                    return mpc;
                }
            }

            return null;
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public Party getEnemy()
    {
        return enemy;
    }

    public void setEnemy(Party enemy)
    {
        this.enemy = enemy;
    }

    public List<int> getMembersSortedByHistory()
    {
        List<KeyValuePair<int, int>> histList;

        Monitor.Enter(lockObj);
        try
        {
            histList = new(histMembers);
        }
        finally
        {
            Monitor.Exit(lockObj);
        }

        histList.Sort((o1, o2) => (o1.Value).CompareTo(o2.Value));

        List<int> histSort = new();
        foreach (var e in histList)
        {
            histSort.Add(e.Key);
        }

        return histSort;
    }

    public sbyte getPartyDoor(int cid)
    {
        List<int> histList = getMembersSortedByHistory();
        sbyte slot = 0;
        foreach (int e in histList)
        {
            if (e == cid)
            {
                break;
            }
            slot++;
        }

        return slot;
    }

    public void addDoor(int owner, Door door)
    {
        Monitor.Enter(lockObj);
        try
        {
            this.doors.AddOrUpdate(owner, door);
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public void removeDoor(int owner)
    {
        Monitor.Enter(lockObj);
        try
        {
            this.doors.Remove(owner);
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public Dictionary<int, Door> getDoors()
    {
        Monitor.Enter(lockObj);
        try
        {
            return new Dictionary<int, Door>(doors);
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public void assignNewLeader(Client c)
    {
        World world = c.getWorldServer();
        PartyCharacter? newLeadr = null;

        Monitor.Enter(lockObj);
        try
        {
            foreach (PartyCharacter mpc in members)
            {
                if (mpc.getId() != leaderId && (newLeadr == null || newLeadr.getLevel() < mpc.getLevel()))
                {
                    newLeadr = mpc;
                }
            }
        }
        finally
        {
            Monitor.Exit(lockObj);
        }

        if (newLeadr != null)
        {
            world.updateParty(this.getId(), PartyOperation.CHANGE_LEADER, newLeadr);
        }
    }

    public override int GetHashCode()
    {
        int prime = 31;
        int result = 1;
        result = prime * result + id;
        return result;
    }

    public PartyCharacter? getMemberByPos(int pos)
    {
        return members.ElementAtOrDefault(pos);
    }

    public override bool Equals(object? obj)
    {
        return obj is Party other && other.id == id;
    }

    public static bool createParty(Character player, bool silentCheck)
    {
        var party = player.getParty();
        if (party == null)
        {
            if (player.getLevel() < 10 && !YamlConfig.config.server.USE_PARTY_FOR_STARTERS)
            {
                player.sendPacket(PacketCreator.partyStatusMessage(10));
                return false;
            }
            else if (player.getAriantColiseum() != null)
            {
                player.dropMessage(5, "You cannot request a party creation while participating the Ariant Battle Arena.");
                return false;
            }

            PartyCharacter partyplayer = new PartyCharacter(player);
            party = player.getWorldServer().createParty(partyplayer);
            player.setParty(party);
            player.setMPC(partyplayer);
            player.getMap().addPartyMember(player, party.getId());
            player.silentPartyUpdate();

            player.updatePartySearchAvailability(false);
            player.partyOperationUpdate(party, null);

            player.sendPacket(PacketCreator.partyCreated(party, partyplayer.getId()));

            return true;
        }
        else
        {
            if (!silentCheck)
            {
                player.sendPacket(PacketCreator.partyStatusMessage(16));
            }

            return false;
        }
    }

    public static bool joinParty(Character player, int partyid, bool silentCheck)
    {
        var party = player.getParty();
        World world = player.getWorldServer();

        if (party == null)
        {
            party = world.getParty(partyid);
            if (party != null)
            {
                if (party.getMembers().Count < 6)
                {
                    PartyCharacter partyplayer = new PartyCharacter(player);
                    player.getMap().addPartyMember(player, party.getId());

                    world.updateParty(party.getId(), PartyOperation.JOIN, partyplayer);
                    player.receivePartyMemberHP();
                    player.updatePartyMemberHP();

                    player.resetPartySearchInvite(party.getLeaderId());
                    player.updatePartySearchAvailability(false);
                    player.partyOperationUpdate(party, null);
                    return true;
                }
                else
                {
                    if (!silentCheck)
                    {
                        player.sendPacket(PacketCreator.partyStatusMessage(17));
                    }
                }
            }
            else
            {
                player.sendPacket(PacketCreator.serverNotice(5, "You couldn't join the party since it had already been disbanded."));
            }
        }
        else
        {
            if (!silentCheck)
            {
                player.sendPacket(PacketCreator.serverNotice(5, "You can't join the party as you are already in one."));
            }
        }

        return false;
    }

    public static void leaveParty(Party party, Client c)
    {
        World world = c.getWorldServer();
        Character player = c.getPlayer();
        PartyCharacter partyplayer = player.getMPC();

        if (party != null && partyplayer != null)
        {
            if (partyplayer.getId() == party.getLeaderId())
            {
                c.getWorldServer().removeMapPartyMembers(party.getId());

                MonsterCarnival mcpq = player.getMonsterCarnival();
                if (mcpq != null)
                {
                    mcpq.leftParty(player.getId());
                }

                world.updateParty(party.getId(), PartyOperation.DISBAND, partyplayer);

                EventInstanceManager eim = player.getEventInstance();
                if (eim != null)
                {
                    eim.disbandParty();
                }
            }
            else
            {
                MapleMap map = player.getMap();
                if (map != null)
                {
                    map.removePartyMember(player, party.getId());
                }

                MonsterCarnival mcpq = player.getMonsterCarnival();
                if (mcpq != null)
                {
                    mcpq.leftParty(player.getId());
                }

                world.updateParty(party.getId(), PartyOperation.LEAVE, partyplayer);

                EventInstanceManager eim = player.getEventInstance();
                if (eim != null)
                {
                    eim.leftParty(player);
                }
            }

            player.setParty(null);

            MatchCheckerCoordinator mmce = c.getWorldServer().getMatchCheckerCoordinator();
            if (mmce.getMatchConfirmationLeaderid(player.getId()) == player.getId() && mmce.getMatchConfirmationType(player.getId()) == MatchCheckerType.GUILD_CREATION)
            {
                mmce.dismissMatchConfirmation(player.getId());
            }
        }
    }

    public static void expelFromParty(Party party, Client c, int expelCid)
    {
        World world = c.getWorldServer();
        Character player = c.getPlayer();
        PartyCharacter partyplayer = player.getMPC();

        if (party != null && partyplayer != null)
        {
            if (partyplayer.Equals(party.getLeader()))
            {
                PartyCharacter expelled = party.getMemberById(expelCid);
                if (expelled != null)
                {
                    Character emc = expelled.getPlayer();
                    if (emc != null)
                    {
                        List<Character> partyMembers = emc.getPartyMembersOnline();

                        MapleMap map = emc.getMap();
                        if (map != null)
                        {
                            map.removePartyMember(emc, party.getId());
                        }

                        MonsterCarnival mcpq = player.getMonsterCarnival();
                        if (mcpq != null)
                        {
                            mcpq.leftParty(emc.getId());
                        }

                        EventInstanceManager eim = emc.getEventInstance();
                        if (eim != null)
                        {
                            eim.leftParty(emc);
                        }

                        emc.setParty(null);
                        world.updateParty(party.getId(), PartyOperation.EXPEL, expelled);

                        emc.updatePartySearchAvailability(true);
                        emc.partyOperationUpdate(party, partyMembers);
                    }
                    else
                    {
                        world.updateParty(party.getId(), PartyOperation.EXPEL, expelled);
                    }
                }
            }
        }
    }
}
