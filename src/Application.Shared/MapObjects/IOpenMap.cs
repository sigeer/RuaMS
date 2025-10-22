using System.Drawing;

namespace Application.Shared.MapObjects
{
    /// <summary>
    /// 脚本调用的方法，方便溯源
    /// </summary>
    public interface IOpenMap
    {
        void toggleEnvironment(string ms);
        void moveEnvironment(string ms, int type);

        void allowSummonState(bool b);
        bool getSummonState();

        void toggleDrops();

        void setReactorState();
        bool isAllReactorState(int reactorId, int state);

        void setAllowSpawnPointInBox(bool allow, Rectangle box);

        void warpEveryone(int to, int pto);

        void resetPQ(int difficulty = 1);

        void broadcastShip(bool state);
        void broadcastEnemyShip(bool state);

        int getNumPlayersInArea(int index);

        int countMonsters();
        int countMonster(int id);
        int countMonster(int minid, int maxid);
    }
}
