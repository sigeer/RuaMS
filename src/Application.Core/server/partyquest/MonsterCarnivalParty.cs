using Application.Core.Game.Relation;
using constants.String;
using tools;

namespace server.partyquest;




/**
 * @author Rob
 */
public class MonsterCarnivalParty
{
    /// <summary>
    /// 红队0  蓝队1
    /// </summary>
    public sbyte TeamFlag { get; set; }

    public int AvailableCP { get; set; }
    public int TotalCP { get; set; }
    public int SummonedMonster { get; set; }
    public int SummonedReactor { get; set; }
    public bool IsWinner { get; private set; }
    public Team Team { get; }
    public MonsterCarnival Event { get; }
    public MonsterCarnivalParty? Enemy { get; set; }

    public MonsterCarnivalParty(MonsterCarnival @event, Team team, sbyte team1)
    {
        Event = @event;
        Team = team;

        TeamFlag = team1;

        var portal = 0;
        var map = @event.getEventMap();
        if (map.isPurpleCPQMap())
        {
            portal = TeamFlag == 0 ? 2 : 1;
        }

        foreach (var mc in team.GetChannelMembers(@event.WorldChannel))
        {
            if (mc != null)
            {
                mc.setTeam(TeamFlag);
                mc.setMonsterCarnival(@event);
                mc.setFestivalPoints(0);
                mc.forceChangeMap(map, map.getPortal(portal));
                mc.dropMessage(6, LanguageConstants.getMessage(mc, LanguageConstants.CPQEntry));
            }
        }
    }

    public void SetEnemy(MonsterCarnivalParty party)
    {
        this.Enemy = party;
    }

    public void AddCP(IPlayer player, int amount)
    {
        TotalCP += amount;
        AvailableCP += amount;
        player.addCP(amount);
    }
    public void UseCP(IPlayer player, int amount)
    {
        AvailableCP -= amount;
        player.useCP(amount);
    }

    public void Summon()
    {
        this.SummonedMonster++;
    }

    public bool CanSummon()
    {
        return this.SummonedMonster < Event.getEventMap().MaxMobs;
    }

    public bool CanGuardian()
    {
        var strFlag = TeamFlag.ToString();
        return Event.getEventMap().getAllReactors().Count(x => x.getName().Substring(0, 1) == strFlag) < Event.getEventMap().MaxReactors;
    }

    public void SetResult(bool isWinner)
    {
        IsWinner = isWinner;
    }

    public void PlayUI()
    {
        var map = Event.getEventMap();
        var effect = IsWinner ? map.EffectWin : map.EffectLose;
        var sound = IsWinner ? map.SoundWin : map.SoundLose;
        foreach (var mc in Team.GetChannelMembers(Event.WorldChannel))
        {
            if (mc.IsOnlined)
            {
                mc.sendPacket(PacketCreator.showEffect(effect));
                mc.sendPacket(PacketCreator.playSound(sound));
                mc.dispelDebuffs();
            }
        }
    }

    /// <summary>
    /// 清理状态，前往结算地图
    /// </summary>
    public void Finish()
    {
        var mapFactory = Event.getEventMap().getChannelServer().getMapFactory();
        var map = Event.getEventMap();

        var rewardMap = mapFactory.getMap(IsWinner ? map.RewardMapWin : map.RewardMapLose);
        foreach (var mc in Team.GetChannelMembers(Event.WorldChannel))
        {
            if (mc.IsOnlined)
            {
                mc.gainFestivalPoints(this.TotalCP);
                mc.setMonsterCarnival(null);
                mc.changeMap(rewardMap, rewardMap.getPortal(0));
                mc.setTeam(-1);
                mc.dispelDebuffs();
            }
        }
    }
    /// <summary>
    /// 清理状态，回到大厅
    /// </summary>
    /// <param name="warpout"></param>
    public void Dispose(bool warpout)
    {
        var outMap = Event.GetOutMap();
        foreach (var mc in Team.GetChannelMembers(Event.WorldChannel))
        {
            if (mc != null)
            {
                mc.resetCP();
                mc.setTeam(-1);
                mc.setMonsterCarnival(null);
                if (warpout)
                {
                    mc.changeMap(outMap, outMap.getPortal(0));
                }
            }
        }
        this.Enemy = null;
        TotalCP = 0;
        AvailableCP = 0;
    }
}
