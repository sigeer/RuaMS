namespace Application.Templates.Map
{
    public class MapObstacleTemplate : AbstractTemplate
    {
        public MapObstacleTemplate(int templateId) : base(templateId)
        {
        }

        public int MobDamage { get; set; }
    }
}
