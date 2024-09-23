using constants.id;
using server.partyquest;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public AriantColiseum? ariantColiseum;
        int ariantPoints;
        public AriantColiseum? getAriantColiseum()
        {
            return ariantColiseum;
        }

        public void setAriantColiseum(AriantColiseum? ariantColiseum)
        {
            this.ariantColiseum = ariantColiseum;
        }

        public void gainAriantPoints(int points)
        {
            this.ariantPoints += points;
        }

        public int getAriantPoints()
        {
            return this.ariantPoints;
        }

        public static string getAriantRoomLeaderName(int room)
        {
            return ariantroomleader[room];
        }

        public static int getAriantSlotsRoom(int room)
        {
            return ariantroomslot[room];
        }

        public void updateAriantScore()
        {
            updateAriantScore(0);
        }

        public void updateAriantScore(int dropQty)
        {
            var arena = this.getAriantColiseum();
            if (arena != null)
            {
                arena.updateAriantScore(this, countItem(ItemId.ARPQ_SPIRIT_JEWEL));

                if (dropQty > 0)
                {
                    arena.addLostShards(dropQty);
                }
            }
        }
    }
}
