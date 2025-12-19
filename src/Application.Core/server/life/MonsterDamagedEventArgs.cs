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
