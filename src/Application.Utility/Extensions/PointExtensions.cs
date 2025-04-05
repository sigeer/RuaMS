using System.Drawing;
using System.Numerics;

namespace Application.Utility.Extensions
{
    public static class PointExtensions
    {
        public static void setLocation(this Point p, int x, int y)
        {
            p.X = x;
            p.Y = y;
        }
        public static float distance(this Point source, Point other)
        {
            return Vector2.Distance(new Vector2(source.X, source.Y), new Vector2(other.X, other.Y));
        }

        public static float distanceSq(this Point source, Point other)
        {
            return Vector2.DistanceSquared(new Vector2(source.X, source.Y), new Vector2(other.X, other.Y));
        }
    }
}
