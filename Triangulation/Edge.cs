using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Triangulation
{
    class Edge
    {
        public enum State
        { 
            COLLINEAR, PARALLEL, SKEW
        }

        public enum Classify
        {
            LEFT, RIGHT, BEYOND, BEHIND, BETWEEN, ORIGIN, DESTINATION
        }

        public PointF First;
        public PointF Second;

        public Edge(PointF p1, PointF p2)
        {
            First = p1;
            Second = p2;
        }

        public Edge(Edge e)
        {
            First = e.First;
            Second = e.Second;
        }

        Edge() { }

        public PointF GetFirst() => First;
        public PointF GetSecond() => Second;

        public Edge Rot()
        {
            PointF m = new PointF();
            m.X = 0.5f * (First.X + Second.X);
            m.Y = 0.5f * (First.Y + Second.Y);

            PointF n= new PointF();
            n.X = 0.5f * (Second.Y - First.Y);
            n.Y = 0.5f * (First.X - Second.X);

            First.X = m.X - n.X;
            First.Y = m.Y - n.Y;

            Second.X = m.X + n.X;
            Second.Y = m.Y + n.Y;

            return this;
        }

        public Edge Flip()
        {
            PointF tmp = First;
            First = Second;
            Second = tmp;

            return this;
        }

        public State intersect(Edge e, ref float t)
        {
            PointF a = First;
            PointF b = Second;
            PointF c = e.First;
            PointF d = e.Second;
            PointF temp1 = new PointF(d.X - c.X, d.Y - c.Y);
            PointF temp2 = new PointF(c.X - d.X, c.Y - d.Y);
            PointF n = new PointF(temp1.Y, temp2.X);
            float denom = dotProduct(n, new PointF(b.X - a.X, b.Y - a.Y));
            if (denom == 0f)
            {
                Classify aclass = classify(First, e.First, e.Second);
                if (aclass == Classify.LEFT || aclass == Classify.RIGHT)
                    return State.PARALLEL;
                else
                    return State.COLLINEAR;
            }
            float num = dotProduct(n, new PointF(a.X - c.X, a.Y - c.Y));
            t = -num / denom;
            return State.SKEW;
        }

        public Classify classify(PointF p, PointF p0, PointF p1)
        {
            PointF a = new PointF(p1.X - p0.X, p1.Y - p0.Y);
            PointF b = new PointF(p.X - p0.X, p.Y - p0.Y);
            float sa = a.X * b.Y - b.X * a.Y;

            if (sa > 0f)
                return Classify.LEFT;
            if (sa < 0f)
                return Classify.RIGHT;
            if ((a.X * b.X < 0f) || (a.Y * b.Y < 0f))
                return Classify.BEHIND;
            if (Math.Sqrt(a.X * a.X + a.Y * a.Y) < Math.Sqrt(b.X * b.X + b.Y * b.Y))
                return Classify.BEYOND;
            if (p0 == p)
                return Classify.ORIGIN;
            if (p1 == p)
                return Classify.DESTINATION;
            return Classify.BETWEEN;
        }

        public int orientation(PointF pp, PointF q, PointF r)
        {
            float val = (q.Y - pp.Y) * (r.X - q.X) -
                      (q.X - pp.X) * (r.Y - q.Y);
            if (val == 0) return 0; // точка на ребре
            return (val > 0) ? 1 : -1; // 1 - слева, -1 - справа
        }

        private float dotProduct(PointF p, PointF q) => (p.X * q.X - p.Y * q.Y);

        public PointF Intersection(PointF p0, PointF p1, PointF p2, PointF p3)
        {
            PointF i = new PointF(-1, -1);
            PointF s1 = new PointF();
            PointF s2 = new PointF();
            s1.X = p1.X - p0.X;
            s1.Y = p1.Y - p0.Y;
            s2.X = p3.X - p2.X;
            s2.Y = p3.Y - p2.Y;
            float s, t;
            s = (-s1.Y * (p0.X - p2.X) + s1.X * (p0.Y - p2.Y)) / (-s2.X * s1.Y + s1.X * s2.Y);
            t = (s2.X * (p0.Y - p2.Y) - s2.Y * (p0.X - p2.X)) / (-s2.X * s1.Y + s1.X * s2.Y);

            if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
            {
                i.X = p0.X + (t * s1.X);
                i.Y = p0.Y + (t * s1.Y);

            }
            return i;
        }
    }
}
