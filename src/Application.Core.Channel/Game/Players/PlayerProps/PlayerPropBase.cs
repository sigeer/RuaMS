namespace Application.Core.Game.Players.PlayerProps
{
    public abstract class PlayerPropBase
    {
        public PlayerPropBase(Player owner)
        {
            Owner = owner;
        }

        public Player Owner { get; set; }
        public abstract void LoadData(DBContext dbContext);
        public abstract void SaveData(DBContext dbContext);
    }
}
