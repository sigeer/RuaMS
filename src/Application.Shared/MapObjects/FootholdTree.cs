using Application.Templates.Map;
using System.Drawing;

namespace Application.Shared.MapObjects
{
    public class FootholdTree
    {
        private List<Foothold> footholds = new();

        public FootholdTree(List<Foothold> list)
        {
            footholds = list;
            footholds.Sort();
        }

        public Foothold? FindBelowFoothold(Point p)
        {
            foreach (Foothold fh in footholds)
            {
                if (!fh.isWall() && fh.LeftX <= p.X && fh.RightX >= p.X && p.Y <= fh.TopY)
                {
                    var calcY = fh.calculateFooting(p.X);
                    if (p.Y <= calcY)
                    {
                        return fh;
                    }
                }
            }
            return null;
        }

        public Point? FindBelowPoint(Point p)
        {
            foreach (Foothold fh in footholds)
            {
                if (!fh.isWall() && fh.LeftX <= p.X && fh.RightX >= p.X && p.Y <= fh.TopY)
                {
                    var calcY = fh.calculateFooting(p.X);
                    if (p.Y <= calcY)
                    {
                        return new Point(p.X, (int)Math.Floor(calcY));
                    }
                }
            }
            return null;
        }

        public static FootholdTree FromTemplate(MapTemplate mapTemplate)
        {
            List<Foothold> allFootholds = new();
            foreach (var item in mapTemplate.Footholds)
            {
                Foothold fh = new Foothold(new Point(item.X1, item.Y1), new Point(item.X2, item.Y2), item.Index);
                fh.setPrev(item.Prev);
                fh.setNext(item.Next);
                allFootholds.Add(fh);
            }

            var footholds = new FootholdTree(allFootholds);
            return footholds;
        }
    }
}
