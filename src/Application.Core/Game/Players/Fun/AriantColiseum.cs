using Application.Core.scripting.Events.Instances;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public void updateAriantScore()
        {
            updateAriantScore(0);
        }

        public void updateAriantScore(int dropQty)
        {
            var arena = getEventInstance() as AriantEventInstanceManager;
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
