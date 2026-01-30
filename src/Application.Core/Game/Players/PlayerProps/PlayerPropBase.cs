using Google.Protobuf.Collections;

namespace Application.Core.Game.Players.PlayerProps
{
    public abstract class PlayerPropBase<TDTO> where TDTO : class
    {
        public PlayerPropBase(Player owner)
        {
            Owner = owner;
        }

        public Player Owner { get; set; }
        public abstract void LoadData(RepeatedField<TDTO> dbContext);
        public abstract TDTO[] ToDto();
    }
}
