using Application.Core.Game.Maps;

namespace Application.Core.Game.Relation
{
    public interface ITeamObject : IMapObject
    {
        public int Id { get; }
        public Team? TeamModel { get; set; }
    }
}
