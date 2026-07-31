namespace Application.Core.EF.Entities
{
    public class PNpcEntity : IKeyedEntity<int>
    {
        protected PNpcEntity()
        {
        }

        public PNpcEntity(int id, int mapId, int lifeId, int x, int y, int fh, string type)
        {
            Id = id;
            Life = lifeId;
            F = 0;
            Fh = fh;
            Cy = y;
            Rx0 = x + 50;
            Rx1 = x - 50;
            Type = type;
            X = x;
            Y = y;
            Map = mapId;
        }

        public int Id { get; set; }

        public int Map { get; set; }

        public int Life { get; set; }

        public string Type { get; set; } = null!;

        public int Cy { get; set; }

        public int F { get; set; }

        public int Fh { get; set; }

        public int Rx0 { get; set; }

        public int Rx1 { get; set; }

        public int X { get; set; }

        public int Y { get; set; }
        /// <summary>
        /// -1 永久
        /// </summary>
        public long Expired { get; set; }
        /// <summary>
        /// 脚本模板
        /// </summary>
        public string? ScriptTemplate { get; set; }
    }
}
