

using client;

namespace server.life;

public interface MonsterListener
{

    public Action<int>? monsterKilled { get; set; }
    public Action<Character, int>? monsterDamaged { get; set; }
    public Action<int>? monsterHealed { get; set; }
}

public class ActualMonsterListener : MonsterListener
{

    public Action<int>? monsterKilled { get; set; }
    public Action<Character, int>? monsterDamaged { get; set; }
    public Action<int>? monsterHealed { get; set; }
}
