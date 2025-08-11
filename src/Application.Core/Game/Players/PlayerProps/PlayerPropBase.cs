using Google.Protobuf.Collections;

namespace Application.Core.Game.Players.PlayerProps
{
    public abstract class PlayerPropBase<TDTO> where TDTO: class
    {
        public PlayerPropBase(IPlayer owner)
        {
            Owner = owner;
        }

        public IPlayer Owner { get; set; }
        public abstract void LoadData(RepeatedField<TDTO> dbContext);
        public abstract TDTO[] ToDto();
    }
}
