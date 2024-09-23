

using Application.Core.Game.Maps;
using tools;

namespace server.partyquest;




/**
 * @author Rob
 */
public class MonsterCarnivalParty
{

    private List<IPlayer> members = new();
    private IPlayer leader;
    private byte team;
    private short availableCP = 0, totalCP = 0;
    private int summons = 8;
    private bool winner = false;

    public MonsterCarnivalParty(IPlayer owner, List<IPlayer> members1, byte team1)
    {
        leader = owner;
        members = members1;
        team = team1;

        foreach (IPlayer chr in members)
        {
            chr.setMonsterCarnivalParty(this);
            chr.setTeam(team);
        }
    }

    public IPlayer getLeader()
    {
        return leader;
    }

    public void addCP(IPlayer player, short ammount)
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

    public void useCP(IPlayer player, short ammount)
    {
        availableCP -= ammount;
        player.useCP(ammount);
    }

    public List<IPlayer> getMembers()
    {
        return members;
    }

    public int getTeam()
    {
        return team;
    }

    public void warpOut(int map)
    {
        foreach (var chr in members)
        {
            chr.changeMap(map, 0);
            chr.setMonsterCarnivalParty(null);
            chr.setMonsterCarnival(null);
        }
        members.Clear();
    }

    public void warp(IMap map, int portalid)
    {
        foreach (var chr in members)
        {
            chr.changeMap(map, map.getPortal(portalid));
        }
    }

    public void warpOut()
    {
        var baseId = winner ? 980000003 : 980000004;
        warpOut(baseId + (leader.getMonsterCarnival()!.getRoom() * 100));
    }

    public bool allInMap(IMap map)
    {
        bool status = true;
        foreach (var chr in members)
        {
            if (chr.getMap() != map)
            {
                status = false;
            }
        }
        return status;
    }

    public void removeMember(IPlayer chr)
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

        foreach (var chr in members)
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
