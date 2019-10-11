using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Triangulation
{
    public class Triangle
    {
        public PointF a, b, c, center;
        public double dist;

        public Triangle(PointF _a, PointF _b, PointF _c)
        {
            a = _a;
            b = _b;
            c = _c;
            getCenter();
            getDistance();
        }

        public void getCenter()
        {
            double x1 = (a.X + b.X) / 2;
            double y1 = (a.Y + b.Y) / 2;

            double x2 = (a.X + c.X) / 2;
            double y2 = (a.Y + c.Y) / 2;

            double a1 = b.X - a.X;
            double b1 = b.Y - a.Y;
            double c1 = x1 * a1 + y1 * b1;

            double a2 = c.X - a.X;
            double b2 = c.Y - a.Y;
            double c2 = x2 * a2 + y2 * b2;

            double centerX = Math.Round((c1 * b2 - c2 * b1) / (a1 * b2 - a2 * b1));
            double centerY = Math.Round((a1 * c2 - a2 * c1) / (a1 * b2 - a2 * b1));

            center = new PointF(Convert.ToInt32(centerX), Convert.ToInt32(centerY));
        }

        public void getDistance()
        {
            dist = ((a.Y - b.Y) * center.X + (b.X - a.X) * center.Y + (a.X * b.Y + b.X * a.Y)) / (Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y)));
        }
    }

}
