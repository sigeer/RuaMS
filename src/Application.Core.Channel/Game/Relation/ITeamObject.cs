using Application.Core.Game.Maps;

namespace Application.Core.Game.Relation
{
    public interface ITeamObject : IMonsterCarnivalPartyMember, IMapObject
    {
        public int Id { get; }
        public ITeam? TeamModel { get; set; }
    }
}
