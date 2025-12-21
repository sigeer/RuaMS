using Application.Utility;
using System.Drawing;

namespace Application.Shared.Constants
{
    public class RandomPoint
    {
        public int MinX { get; set; }
        public int MaxX { get; set; }
        public int X { get; set; }

        public int Y { get; set; }

        public Point GetPoint()
        {
            if (MinX == MaxX)
            {
                return new Point(X, Y);
            }
            else
            {
                return  new (Randomizer.NextInt(MinX, MaxX), Y);
            }
        }
    }
}
