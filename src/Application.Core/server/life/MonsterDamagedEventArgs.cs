using Application.Core.Game.Life;
using Application.Core.Game.Maps;

namespace server.life;

public sealed class MonsterDamagedEventArgs : EventArgs
{
    public ICombatantObject Attacker { get; }
    public int Damage { get; }

    public MonsterDamagedEventArgs(ICombatantObject attacker, int damage)
    {
        Attacker = attacker;
        Damage = damage;
    }
}


public sealed class MonsterKilledEventArgs : EventArgs
{
    public ICombatantObject? Killer { get; }
    public int DieAni { get; }
    public MonsterKilledEventArgs(ICombatantObject? killer, int dieAni)
    {
        Killer = killer;
        DieAni = dieAni;
    }
}

public sealed class MonsterReviveEventArgs : EventArgs
{
    public MonsterReviveEventArgs(Monster nextMob, ICombatantObject? killer)
    {
        NextMob = nextMob;
        Killer = killer;
    }

    public Monster NextMob { get; }
    public ICombatantObject? Killer { get; }

}
