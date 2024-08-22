

using client;
using server.maps;
using tools;

namespace server.partyquest;




/**
 * @author Rob
 */
public class MonsterCarnivalParty
{

    private List<Character> members = new();
    private Character leader;
    private byte team;
    private short availableCP = 0, totalCP = 0;
    private int summons = 8;
    private bool winner = false;

    public MonsterCarnivalParty(Character owner, List<Character> members1, byte team1)
    {
        leader = owner;
        members = members1;
        team = team1;

        foreach (Character chr in members)
        {
            chr.setMonsterCarnivalParty(this);
            chr.setTeam(team);
        }
    }

    public Character getLeader()
    {
        return leader;
    }

    public void addCP(Character player, short ammount)
    {
        totalCP += ammount;
        availableCP += ammount;
        player.addCP(ammount);
    }

    public int getTotalCP()
    {
        return totalCP;
    }

    public int getAvailableCP()
    {
        return availableCP;
    }

    public void useCP(Character player, short ammount)
    {
        availableCP -= ammount;
        player.useCP(ammount);
    }

    public List<Character> getMembers()
    {
        return members;
    }

    public int getTeam()
    {
        return team;
    }

    public void warpOut(int map)
    {
        foreach (Character chr in members)
        {
            chr.changeMap(map, 0);
            chr.setMonsterCarnivalParty(null);
            chr.setMonsterCarnival(null);
        }
        members.Clear();
    }

    public void warp(MapleMap map, int portalid)
    {
        foreach (Character chr in members)
        {
            chr.changeMap(map, map.getPortal(portalid));
        }
    }

    public void warpOut()
    {
        if (winner == true)
        {
            warpOut(980000003 + (leader.getMonsterCarnival().getRoom() * 100));
        }
        else
        {
            warpOut(980000004 + (leader.getMonsterCarnival().getRoom() * 100));
        }
    }

    public bool allInMap(MapleMap map)
    {
        bool status = true;
        foreach (Character chr in members)
        {
            if (chr.getMap() != map)
            {
                status = false;
            }
        }
        return status;
    }

    public void removeMember(Character chr)
    {
        members.Remove(chr);
        chr.changeMap(980000010);
        chr.setMonsterCarnivalParty(null);
        chr.setMonsterCarnival(null);
    }

    public bool isWinner()
    {
        return winner;
    }

    public void setWinner(bool status)
    {
        winner = status;
    }

    public void displayMatchResult()
    {
        string effect = winner ? "quest/carnival/win" : "quest/carnival/lose";

        foreach (Character chr in members)
        {
            chr.sendPacket(PacketCreator.showEffect(effect));
        }
    }

    public void summon()
    {
        this.summons--;
    }

    public bool canSummon()
    {
        return this.summons > 0;
    }
}
