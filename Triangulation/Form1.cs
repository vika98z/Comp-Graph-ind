using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Triangulation
{
    public partial class Form1 : Form
    {

        private Graphics g;

        private List<PointF> points = new List<PointF>();
        private List<Edge> livingEdges = new List<Edge>();
        private List<Edge> frontier = new List<Edge>();
        public Form1()
        {
            InitializeComponent();
            
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(pictureBox1.Image);
            g.Clear(Color.White);
        }

        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            points.Add(e.Location);
            g.FillEllipse(new SolidBrush(Color.Red), e.X - 2.5f, e.Y - 2.5f, 5, 5);
            pictureBox1.Invalidate();
        }

        private int edgeCmp(Edge a, Edge b)
        {
            if (a.First.X < b.First.X && a.First.Y < b.First.Y)
                return -1;
            if (a.First.X > b.First.X && a.First.Y > b.First.Y)
                return 1;
            if (a.Second.X < b.Second.X && a.Second.Y < b.Second.Y)
                return -1;
            if (a.Second.X > b.Second.X && a.Second.Y > b.Second.Y)
                return 1;
            return 0;
        }

        void updateFrontier(ref List<Edge> frontier, PointF a, PointF b)
        {
            Edge e = new Edge(a, b);
            if (frontier.Contains(e))//(frontier.First(edge => edge.First == e.First && edge.Second == e.Second) != null)
            {
                int ind = frontier.FindIndex(new Predicate<Edge>(_edge => _edge.Equals(e)));
                if (ind != -1) frontier.RemoveAt(ind);
                //frontier.Remove(e);
            }
            else
            {
                e.Flip();
                frontier.Add(e);
            }
        }

        Edge hullEdge(List<PointF> points, int n)
        {
            int m = 0;
            for (int i = 0; i < n; i++)
                if (points[i].X < points[m].X && points[i].Y < points[m].Y)
                    m = i;

            //swap
            PointF c = points[0];
            points[0] = points[m];
            points[m] = c;

            Edge helpEdge = new Edge(new PointF(), new PointF());
            m = 1;//???
            for (int i = 2; i < n; i++)
            {
                Edge.Classify pos = helpEdge.classify(points[i], points[0], points[m]);
                if (pos == Edge.Classify.LEFT || pos == Edge.Classify.BETWEEN)
                    m = i;
            }
            return new Edge(points[0], points[m]); 
        }
        

        private double angleBetweenTwoEdges(Edge e1, Edge e2)
        {
            PointF a1 = new PointF();
            a1.X = e1.Second.X - e1.First.X;
            a1.Y = e1.Second.Y - e1.First.Y;

            PointF a2 = new PointF();
            a2.X = e2.Second.X - e2.First.X;
            a2.Y = e2.Second.Y - e2.First.Y;

            return Math.Acos((a1.X * a2.X + a1.Y * a2.Y) / 
                (Math.Sqrt(a1.X * a1.X + a1.Y * a1.Y) * 
                Math.Sqrt(a2.X * a2.X + a2.Y * a2.Y))) * (180 / Math.PI);
        }

        public int orientation(PointF p1, PointF q, PointF p2)
        {
            double pos = (p2.Y - p1.Y) * q.X + (p1.X - p2.X) * q.Y + 
                (p1.Y - p2.Y) * p1.X + (p2.X - p1.X) * p1.Y;
            if (pos < 0)
                return -1; // точка справа
            else if (pos > 0)
                return 1; // точка слева
            else
                return 0; // точка на ребре
        }

        private PointF findFirstPoint(List<PointF> points)
        {
            PointF minPoint = new PointF(pictureBox1.Width, pictureBox1.Height);
            PointF resPoint = points[0];

            foreach (PointF p in points)
            {
                if (p.X < minPoint.X)
                    minPoint = resPoint = p;
                else if (p.X == minPoint.X)
                    if (p.Y <= minPoint.Y)
                        minPoint = resPoint = p;
            }

            return resPoint;
        }

        private PointF findSecondPoint(List<PointF> pts, PointF firstPoint)
        {
            double min = 180;
            PointF secondPoint = firstPoint;
            foreach (PointF p in pts)
            {
                Edge e1 = new Edge(firstPoint, new PointF(firstPoint.X, firstPoint.Y - 1));
                Edge e2 = new Edge(firstPoint, p);
                double angle = angleBetweenTwoEdges(e1, e2);
                if (angle < min)
                {
                    min = angle;
                    secondPoint = p;
                }
            }
            return secondPoint;
        }

        bool mate(Edge e, List<PointF> points, int n, ref PointF p)
        {
            float t = 0; ;
            PointF bestP = new PointF(-1f, -1f);
            float bestT = float.MaxValue;
            Edge f = new Edge(e);
            f.Rot();
            for (int i = 0; i < n; i++)
            {
                if (e.classify(points[i], e.First, e.Second) == Edge.Classify.RIGHT)
                {
                    Edge g = new Edge(e.Second, points[i]);
                    g.Rot();
                    f.intersect(g, ref t);
                    if (t < bestT)
                    {
                        bestP = points[i];
                        bestT = t;
                    }
                }
            }
            if (bestP.X != -1f)
            {
                p = bestP;
                return true;
            }
            return false;
        }

        double radiusTriandle(PointF a, PointF b, PointF c)
        {
            var center = getCenter();
            return ((a.Y - b.Y) * center.X + (b.X - a.X) * center.Y + (a.X * b.Y + b.X * a.Y)) / (Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y)));

            PointF getCenter()
            {
                float x1 = (a.X + b.X) / 2;
                float y1 = (a.Y + b.Y) / 2;

                float x2 = (a.X + c.X) / 2;
                float y2 = (a.Y + c.Y) / 2;

                float a1 = b.X - a.X;
                float b1 = b.Y - a.Y;
                float c1 = x1 * a1 + y1 * b1;

                float a2 = c.X - a.X;
                float b2 = c.Y - a.Y;
                float c2 = x2 * a2 + y2 * b2;

                return new PointF((c1 * b2 - c2 * b1) / (a1 * b2 - a2 * b1), (a1 * c2 - a2 * c1) / (a1 * b2 - a2 * b1));
            }
        }

        private void delTriangulation(List<PointF> Points, int n)
        {
            g.Clear(Color.White);
            PointF firstPoint = findFirstPoint(Points);
            PointF secondPoint = findSecondPoint(Points, firstPoint);
            g.FillEllipse(new SolidBrush(Color.Green), firstPoint.X - 2.5f, firstPoint.Y - 2.5f, 5, 5);
            g.FillEllipse(new SolidBrush(Color.Green), secondPoint.X - 2.5f, secondPoint.Y - 2.5f, 5, 5);
            livingEdges.Add(new Edge(firstPoint, secondPoint));

            while (livingEdges.Count() > 0)
            {
                int i = livingEdges.Count() - 1;
                Edge alive = livingEdges[i];
                livingEdges.RemoveAt(i);

                g.DrawLine(new Pen(Color.Black), alive.First, alive.Second);

                PointF thirdPoint = PointF.Empty;
                double radius = double.MaxValue;

                foreach (PointF p in points)
                {
                    if (orientation(alive.First, p, alive.Second) == -1)
                    {
                        var dist = radiusTriandle(alive.First, alive.Second, p);
                        if (dist < radius)
                        {
                            radius = dist;
                            thirdPoint = p;
                        }
                    }

                    g.FillEllipse(new SolidBrush(Color.Green), p.X - 2.5f, p.Y - 2.5f, 5, 5);
                }            

                if (thirdPoint != Point.Empty)
                {
                    Edge edge = new Edge(alive.First, thirdPoint);
                    int ind = livingEdges.FindIndex(new Predicate<Edge>(_edge => _edge.eqEdges(edge.Flip())));
                    if (ind >= 0)
                    {
                        g.DrawLine(new Pen(Color.Black), livingEdges[ind].First, livingEdges[ind].Second);
                        livingEdges.RemoveAt(ind);
                    }
                    else
                        livingEdges.Add(edge);

                    edge = new Edge(thirdPoint, alive.Second);
                    ind = livingEdges.FindIndex(new Predicate<Edge>(_edge => _edge.eqEdges(edge.Flip())));
                    if (ind >= 0)
                    {
                        g.DrawLine(new Pen(Color.Black), livingEdges[ind].First, livingEdges[ind].Second);
                        livingEdges.RemoveAt(ind);
                    }
                    else
                        livingEdges.Add(edge);
                }
            }
            pictureBox1.Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            delTriangulation(points, points.Count);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            g.Clear(Color.White);
            livingEdges.Clear();
            points.Clear();
            pictureBox1.Invalidate();
        }
    }
}
