namespace Application.Core.Game.Players.PlayerProps
{
    public abstract class PlayerPropBase
    {
        public PlayerPropBase(IPlayer owner)
        {
            Owner = owner;
        }

        public IPlayer Owner { get; set; }
        public abstract void LoadData(DBContext dbContext);
        public abstract void SaveData(DBContext dbContext);
    }
}
