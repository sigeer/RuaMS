namespace Application.Core.tools
{
    public struct Line2D
    {
        public Point2D P1 { get; set; }
        public Point2D P2 { get; set; }

        public static Line2D Float(float x1, float y1, float x2, float y2)
        {
            return new Line2D
            {
                P1 = new Point2D(x1, y2),
                P2 = new Point2D(x2, y2)
            };
        }

        public int getX1()
        {
            return (int)P1.X;
        }

        public int getX2()
        {
            return (int)P2.X;
        }
    }

    public struct Point2D
    {
        public Point2D()
        {
        }

        public Point2D(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { get; set; }
        public float Y { get; set; }
    }
}
