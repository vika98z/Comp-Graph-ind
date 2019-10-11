using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Triangulation
{
    public partial class Form1 : Form
    {

        private Graphics g;
        private List<PointF> points = new List<PointF>();

        private Pen pen = new Pen(Color.Black);
        private SolidBrush brush = new SolidBrush(Color.Blue);

        private List<Edge> edges = new List<Edge>();
        private List<Edge> livingEdges = new List<Edge>();

        private List<Triangle> triangles = new List<Triangle>();
        private List<Edge> frontier;
        public Form1()
        {
            InitializeComponent();
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(pictureBox1.Image);
        }

        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            points.Add(e.Location);
            SolidBrush brush = new SolidBrush(Color.Red);
            g.FillEllipse(brush, e.X - 2.5f, e.Y - 2.5f, 5, 5);
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

        void updateFrontier(List<Edge> frontier, PointF a, PointF b)
        {
            Edge e = new Edge(a, b);
            if (frontier.First(edge => edge.First == e.First && edge.Second == e.Second) != null)
            {
                frontier.Remove(e);
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

            for (int i = 2; i < n; i++)
            {
                int pos = orientation(points[0], points[i], points[m]);
                if (pos == 1)/////????
                    m = i;
            }
            return new Edge(points[0], points[m]); 
        }

        private void drawEdge(Edge e) => g.DrawLine(pen, e.First, e.Second);

        private void drawPoint(PointF p) => g.FillEllipse(brush, p.X - 3, p.Y - 3, 7, 7);

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

        public int orientation(PointF pp, PointF q, PointF r)
        {
            float val = (q.Y - pp.Y) * (r.X - q.X) -
                      (q.X - pp.X) * (r.Y - q.Y);
            if (val == 0) return 0; // точка на ребре
            return (val > 0) ? 1 : -1; // 1 - слева, -1 - справа
        }

        // Поиск первой точки
        private PointF findFirstPoint(List<PointF> pts)
        {
            float xMin = pictureBox1.Width;
            PointF firstPoint = pts[0];

            foreach (PointF p in pts)            
                if (p.X < xMin)
                {
                    xMin = p.X;
                    firstPoint = p;
                }

            return firstPoint;
        }

        // Поиск второй точки
        private PointF findSecondPoint(List<PointF> pts, PointF firstPoint)
        {
            double min = 180;
            PointF secondPoint = firstPoint;
            foreach (PointF p in pts)
            {
                Edge e1 = new Edge(firstPoint, new PointF(firstPoint.X, firstPoint.Y+1));
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

        int mate(Edge e, List<PointF> points, PointF p)
        {
            float t;
            float bestT = float.MaxValue;
            Edge f = new Edge(e);
            f.Rot();
            for (int i = 0; i < n; i++)
            {
                if (orientation(e.First, points[i], e.Second) == -1)
                {
                    Edge g = new Edge(e.Second, points[i]);
                    g.Rot();
                    //if (f.Intersection(g.First, t.Second,))
                }
            }
        }

        List<Triangle> delTriangulation(List<PointF> Points, int n)
        {
            PointF p;
            Edge e = hullEdge(Points, n);
            frontier.Add(e);
            while (!(frontier.Count == 0))
            {

            }
            
            //bool edgeIsFound = false;

            ////redrawTriangulation();

            //PointF firstPoint = findFirstPoint(Points);
            //PointF secondPoint = findSecondPoint(Points, firstPoint);

            //livingEdges.Add(new Edge(firstPoint, secondPoint));

            //while (livingEdges.Count() > 0)
            //{
            //    int i = livingEdges.Count() - 1;
            //    Edge alive = livingEdges[i];
            //    livingEdges.RemoveAt(i);

            //    PointF thirdPoint = PointF.Empty;
            //    double radius = double.MaxValue;

            //    foreach (PointF p in points)
            //    {
            //        if (orientation(alive.First, p, alive.Second) == -1)
            //        {
            //            Triangle triang = new Triangle(alive.First, alive.Second, p);
            //            if (triang.dist < radius)
            //            {
            //                radius = triang.dist;
            //                thirdPoint = p;
            //            }
            //        }
            //        drawPoint(p);
            //    }

            //    drawEdge(alive);
        }
    }
}
