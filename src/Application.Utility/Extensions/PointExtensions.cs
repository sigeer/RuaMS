using System.Drawing;
using System.Numerics;

namespace Application.Utility.Extensions
{
    public static class PointExtensions
    {
        public static float distance(this Point source, Point other)
        {
            return Vector2.Distance(new Vector2(source.X, source.Y), new Vector2(other.X, other.Y));
        }

        public static float distanceSq(this Point source, Point other)
        {
            return Vector2.DistanceSquared(new Vector2(source.X, source.Y), new Vector2(other.X, other.Y));
        }

        public static int GetX(this Point p) => p.X;
        public static int GetY(this Point p) => p.Y;


        public static Point GetRandomPoint(this Rectangle rect)
        {
            // 使用 Random.Shared 避免每次新建实例（.NET 6+）
            int x = Random.Shared.Next(rect.X, rect.X + rect.Width);
            int y = Random.Shared.Next(rect.Y, rect.Y + rect.Height);
            return new Point(x, y);
        }
    }
}
