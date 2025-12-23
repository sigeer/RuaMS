namespace server.life;

public sealed class MonsterDamagedEventArgs : EventArgs
{
    public IPlayer Player { get; }
    public int Damage { get; }

    public MonsterDamagedEventArgs(IPlayer player, int damage)
    {
        Player = player;
        Damage = damage;
    }
}


public sealed class MonsterKilledEventArgs : EventArgs
{
    public IPlayer? Killer { get; }
    public int DieAni { get; }

    public MonsterKilledEventArgs(IPlayer? player, int dieAni)
    {
        Killer = player;
        DieAni = dieAni;
    }
}
