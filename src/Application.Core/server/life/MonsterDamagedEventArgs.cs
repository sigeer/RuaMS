namespace server.life;

public sealed class MonsterDamagedEventArgs : EventArgs
{
    public Player Player { get; }
    public int Damage { get; }

    public MonsterDamagedEventArgs(Player player, int damage)
    {
        Player = player;
        Damage = damage;
    }
}


public sealed class MonsterKilledEventArgs : EventArgs
{
    public Player? Killer { get; }
    public int DieAni { get; }
    public MonsterKilledEventArgs(Player? player, int dieAni)
    {
        Killer = player;
        DieAni = dieAni;
    }
}
