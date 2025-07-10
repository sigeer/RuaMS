using Application.Core.Login.Shared;

namespace Application.Core.Login.Models
{
    public class PLifeModel: ITrackableEntityKey<int>
    {
        public int Id { get; set; }

        public int World { get; set; }

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

        public int Hide { get; set; }

        public int Mobtime { get; set; }

        public int Team { get; set; }
    }
}
