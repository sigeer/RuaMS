using Application.Core.Game.Maps;

namespace Application.Core.Game.Relation
{
    public interface ITeamObject : IMonsterCarnivalPartyMember, IMapObject
    {
        public int Id { get; }
        public Team? TeamModel { get; set; }
    }
}
