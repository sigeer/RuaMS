namespace Application.Core.Game.Maps
{
    public interface IAnimatedMapObject : IMapObject
    {
        int getStance();
        void setStance(int stance);
        bool isFacingLeft();
    }
}
