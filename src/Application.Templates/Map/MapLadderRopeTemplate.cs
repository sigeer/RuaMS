using System.Drawing;

namespace Application.Templates.Map
{
    public class MapLadderRopeTemplate
    {
        public MapLadderRopeTemplate(int index)
        {
            Index = index;
        }

        public int Index { get; set;  }
        public int X { get; set; }
        public int Y1 { get; set; }
        public int Y2 { get; set; }
        /// <summary>
        /// 0. 绳子 1. 梯子
        /// </summary>
        [WZPath("~/l")]
        public int Type { get; set; }

        public bool Contains(Point p)
        {
            return ((p.X > X - 10) && (p.X < X + 10) && (p.Y < Y2 + 12) && (p.Y > Y1 - 12));
        }
    }
}
