using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
namespace lab1
{
    using Point3D = Tuple<int, int, int>;
    using Vector3D = Tuple<double, double, double>;
    public partial class Form1 : Form
    {
        public Graphics g;
        Polyhedron figure;
        protected Pen penMain;
        protected static Form1 instance;

        public Form1()
        {
            InitializeComponent();
            instance = this;
            modeComboBox.SelectedIndex = 0;
            axisComboBox.SelectedIndex = 0;
            g = pictureBox1.CreateGraphics();
            figure = new Polyhedron(pictureBox1.Width, pictureBox1.Height);
            figure.SetCameraAngles(48, 48);
            figure.InitCamera();
            penMain = new Pen(Brushes.Black);
            saveFileDialog1.Filter = "Text files(*.txt) | *txt";
            openFileDialog1.Filter = "Text files(*.txt) | *txt";

            this.MouseWheel += new MouseEventHandler(onScroll);
        }
        public static Form1 getInstance()
        {
            return instance;
        }
        private void onScroll(object sender, MouseEventArgs e)
        {
            bool deltaIsPositive = e.Delta > 0;
            string axis = axisComboBox.Text;
            string mode = modeComboBox.Text;
            Type methodType = this.GetType();
            MethodInfo handler = methodType.GetMethod(mode + "Figure");
            handler.Invoke(this, new object[] { axis, deltaIsPositive });
            figure.draw(penMain);
        }
        private void onClick(object sender, MouseEventArgs e)
        {

        }

        public void resizeFigure(string axis, bool increase)
        {
            figure.Resize(axis, increase);
        }
        public void moveFigure(string axis, bool increase)
        {
            figure.Move(axis, increase);
        }

        public void rotateFigure(string axis, bool increase)
        {
            figure.Rotate(axis, increase);
        }

        public void watchFigure(string axis, bool increase)
        {
            figure.MoveCam(axis, increase);
        }

        private void doAction(object sender, EventArgs e)
        {
            string action = (sender as Button).Text;
            bool increase = action == "+";
            string axis = axisComboBox.Text;
            string mode = modeComboBox.Text;
            Console.WriteLine(action + '\n' + axis + '\n' + mode + '\n');
            Type methodType = this.GetType();
            MethodInfo handler = methodType.GetMethod(mode + "Figure");
            handler.Invoke(this, new object[] { axis, increase });
            figure.draw(penMain);
        }

        private void onSaveClick(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            string filename = saveFileDialog1.FileName;

            System.IO.File.WriteAllText(filename, figure.ConvertTo("csv"));
            MessageBox.Show("Многогранник сохранен в файл!");
        }

        private void onLoadCLick(object sender, EventArgs e)
        {
            figure = new Polyhedron(pictureBox1.Width, pictureBox1.Height);
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            string filename = openFileDialog1.FileName;
            string fileText = System.IO.File.ReadAllText(filename);
            figure.ConvertFrom(fileText, "csv");
            figure.draw(penMain);
        }

        //private void drawFigure(Pen pen)
        //{
        //    g.Clear(pictureBox1.BackColor);
        //    if (figure.NotEmpty())
        //    {
        //        for (int i = 0; i < figure.EdgeCount(); i++)
        //        {
        //            Tuple<int, int> Edge = figure.GetEdge(i);
        //            Point p1 = figure.Cam(Edge.Item1);
        //            Point p2 = figure.Cam(Edge.Item2);
        //            g.DrawLine(pen, p1, p2);
        //        }
        //    }
        //}

        private void onExitBtnClick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void onClearBtnClick(object sender, EventArgs e)
        {
            figure = new Polyhedron(pictureBox1.Width, pictureBox1.Height);
            modeComboBox.SelectedIndex = 0;
            g.Clear(pictureBox1.BackColor);
        }
    }
    class Polyhedron
    {
        protected Tuple<Vector3D, Vector3D, Vector3D> Camera;
        private double AlphaCam;
        private double BetaCam;
        private Point Center;
        int DegreeStep;
        int Step;
        Tuple<int, int> extYs;
        Form1 frm;
        public List<List<Point>> LinePointsForFig;
        private double  a, b, c, p,
                        d, e, f, q,
                        h, i, j, r,
                        l, m, n, s;
        //поворот - a, b, c, d
        //масштабирование a, d, s
        //отражение OX a OY b
        //отражение y=x c y=-x b
        //сдвиг - a,b,c,d
        //перенос - m, n
        protected List<Point3D> Points;
        protected List<Tuple<int, int>> Edges;
        public Polyhedron(int pictureWidth, int pictureHeight, int pointCount = 255)
        {
            frm = Form1.getInstance();
            ResetTransformationMatrix();
            InitCenter(pictureWidth, pictureHeight);
            SetCameraAngles(45,45);
            InitCamera();
            Points = new List<Point3D>(pointCount);
            Edges = new List<Tuple<int, int>>(1 + pointCount / 2);
            DegreeStep = 12;
            Step = 10;

            reInitLinePoints();
        }
        public void reInitLinePoints()
        {
            LinePointsForFig = new List<List<Point>>(1000);
            for (int i = 0; i < 1000; i++)
            {
                LinePointsForFig.Add(new List<Point>(100));
            }
        }
        public void draw(Pen pen)
        {
            frm.g.Clear(Color.White);

            for (int e = 0; e < EdgeCount(); e+=4)
            {
                if (e + 2 >= EdgeCount()) break;
                Random rnd = new Random();
                Pen newPen = new Pen(Color.FromArgb(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255)));
                List<Point> f = new List<Point>(4);
                f.Add(new Point(Points[Edges[e].Item1].Item1, Points[Edges[e].Item1].Item2));
                f.Add(new Point(Points[Edges[e].Item2].Item1, Points[Edges[e].Item2].Item2));
                f.Add(new Point(Points[Edges[e+2].Item1].Item1, Points[Edges[e+2].Item1].Item2));
                f.Add(new Point(Points[Edges[e+2].Item2].Item1, Points[Edges[e+2].Item2].Item2));
                FillShape(newPen, f);
            }

            if (this.NotEmpty())
            {
                for (int i = 0; i < this.EdgeCount(); i++)
                {
                    Tuple<int, int> Edge = this.GetEdge(i);
                    Point p1 = this.Cam(Edge.Item1);
                    Point p2 = this.Cam(Edge.Item2);
                    frm.g.DrawLine(pen, p1, p2);
                }
            }
            
        }
        public int X(int i)
        {
            return Points[i].Item1;
        }
        public int Y(int i)
        {
            return Points[i].Item2;
        }
        public int Z(int i)
        {
            return Points[i].Item3;
        }

        public int CamX(int i)
        {
            return (int)Math.Round(Center.X + ScalarMultiplication(Camera.Item1, Points[i]));
        }
        public int CamY(int i)
        {
            return (int)Math.Round(Center.Y + ScalarMultiplication(Camera.Item2, Points[i]));
        }
        public Point Projection(int i)
        {
            return new Point(X(i), Y(i));
        }
        public Point Cam(int i)
        {
            return new Point(CamX(i), CamY(i));
        }
        public Point CamP(Point3D P)
        {
            int x = (int)Math.Round(Center.X + ScalarMultiplication(Camera.Item1, P));
            int y = (int)Math.Round(Center.Y + ScalarMultiplication(Camera.Item2, P));
            return new Point(x, y);
        }
        public Point Projection(Point3D val)
        {
            double X = val.Item1;
            double Y = val.Item2;
            return new Point((int)(X), (int)(Y));
        }
        public void ConvertFrom(string inText, string mode = "csv")
        {
            switch (mode)
            {
                case "csv":
                    string[] textRows = inText.Split('\n');
                    if (textRows.Length % 2 > 0)
                    {
                        break;
                    }
                    for (int i = 0; i < textRows.Length; i+=2)
                    {
                        string[] coords1 = textRows[i].Split(',');
                        string[] coords2 = textRows[i+1].Split(',');
                        if (coords1.Length < 2 || coords2.Length < 2)
                        {
                            continue;
                        }
                        int FirstPointIndex = AddPoint(new Point3D(int.Parse(coords1[0]), int.Parse(coords1[1]), int.Parse(coords1[2])));
                        int SecondPointIndex = AddPoint(new Point3D(int.Parse(coords2[0]), int.Parse(coords2[1]), int.Parse(coords2[2])));
                        AddEdge(FirstPointIndex, SecondPointIndex);
                    }
                    break;
                default: break;
            }
        }

        public string ConvertTo(string mode = "csv")
        {
            string converted = "";
            switch (mode)
            {
                case "csv":
                    foreach (Tuple<int, int> edge in Edges)
                    {
                        converted += Points[edge.Item1].Item1 + "," + Points[edge.Item1].Item2 + "," + Points[edge.Item1].Item3 + "\n";
                        converted += Points[edge.Item2].Item1 + "," + Points[edge.Item2].Item2 + "," + Points[edge.Item2].Item3 + "\n";
                    }
                    break;
                default: break;
            }
            return converted;
        }

        public int AddPoint(int x, int y, int z)
        {
            Point3D NewPoint = new Point3D(x, y, z);
            int IndexOfPoint = Points.IndexOf(NewPoint);
            if (IndexOfPoint < 0)
            {
                Points.Add(NewPoint);
                IndexOfPoint = Points.Count - 1;
            }
            return IndexOfPoint;
        }
        public int AddPoint(Point3D point)
        {
            int IndexOfPoint = Points.IndexOf(point);
            if (IndexOfPoint < 0)
            {
                Points.Add(point);
                IndexOfPoint = Points.Count - 1;
            }
            return IndexOfPoint;
        }
        public void AddEdge(int i1, int i2)
        {
            Edges.Add(new Tuple<int, int>(i1, i2));
        }

        public Tuple<int, int> GetEdge(int i)
        {
            return Edges[i];
        }

        private Point3D GetFigureCenter(int picWidth, int picHeight)
        {
            int  maxX = 0,
                 maxY = 0,
                 maxZ = 0,
                 minX = picWidth,
                 minY = picHeight,
                 minZ = int.MaxValue;
            for (int i = 0; i < Points.Count; i++)
            {
                maxX = X(i) > maxX ? X(i) : maxX;
                maxY = Y(i) > maxY ? Y(i) : maxY;
                maxZ = Z(i) > maxZ ? Z(i) : maxZ;
                minX = X(i) < minX ? X(i) : minX;
                minY = Y(i) < minY ? Y(i) : minY;
                minZ = Z(i) < minZ ? Z(i) : minZ;
            }
            int x = minX + (maxX - minX) / 2;
            int y = minY + (maxY - minY) / 2;
            int z = minZ + (maxZ - minZ) / 2;
            return new Point3D(x, y, z);
        }

        public bool NotEmpty()
        {
            return Edges.Count > 0;
        }
        public int PointCount()
        {
            return Points.Count;
        }
        public int EdgeCount()
        {
            return Edges.Count;
        }
        public void Move(string axis, bool increase)
        {
            int sign = increase ? 1 : -1;
            switch (axis)
            {
                case "X":
                    l = Step * sign;
                    break;
                case "Y":
                    m = Step * sign;
                    break;
                case "Z":
                    n = Step * sign;
                    break;
                default:
                    break;
            }
            Transform();
        }

        public void MoveCam(string axis, bool increase)
        {
            int sign = increase ? 1 : -1;
            switch (axis)
            {
                case "X":
                    AlphaCam += sign * GetAngleInRadians(DegreeStep);
                    break;
                case "Y":
                    AlphaCam += sign * GetAngleInRadians(DegreeStep);
                    break;
                case "Z":
                    BetaCam += sign * GetAngleInRadians(DegreeStep);
                    break;
                default:
                    break;
            }
            InitCamera();
        }
        public void Rotate(string axis, bool increase)
        {
            int sign = increase ? 1 : -1;
            int degrees = DegreeStep * sign;
            switch (axis)
            {
                case "X":
                    e = Math.Cos(GetAngleInRadians(degrees));
                    f = Math.Sin(GetAngleInRadians(degrees));
                    i = (-1) * Math.Sin(GetAngleInRadians(degrees));
                    j = Math.Cos(GetAngleInRadians(degrees));
                    break;
                case "Y":
                    a = Math.Cos(GetAngleInRadians(degrees));
                    c = Math.Sin(GetAngleInRadians(degrees));
                    h = (-1) * Math.Sin(GetAngleInRadians(degrees));
                    j = Math.Cos(GetAngleInRadians(degrees));
                    break;
                case "Z":
                    a = Math.Cos(GetAngleInRadians(degrees));
                    b = Math.Sin(GetAngleInRadians(degrees));
                    d = (-1) * Math.Sin(GetAngleInRadians(degrees));
                    e = Math.Cos(GetAngleInRadians(degrees));
                    break;
                default:
                    break;
            }
            Transform();
        }

        public void Resize(string axis, bool increase)
        {
            double multiplier = increase ? 1.1 : 0.9;
            switch (axis)
            {
                case "X":
                    a *= multiplier;
                    break;
                case "Y":
                    e *= multiplier;
                    break;
                case "Z":
                    j *= multiplier;
                    break;
                default:
                    break;
            }
            Transform();
        }


        public void InitCenter(int picWidth, int picHeight)
        {
            Center = new Point();
            Center.X = picWidth / 2;
            Center.Y = picHeight / 2;
        }
        public void InitCamera()
        {
            double x, y, z;
            x = Math.Cos(AlphaCam);
            y = Math.Sin(AlphaCam);
            z = 0;
            Vector3D e1 = new Vector3D(x, y, z);
            x = (-1) * Math.Sin(AlphaCam) * Math.Sin(BetaCam);
            y = Math.Cos(AlphaCam) * Math.Sin(BetaCam);
            z = Math.Cos(BetaCam);
            Vector3D e2 = new Vector3D(x, y, z);
            Vector3D e3 = VectorMultiplication(e1, e2);
            Camera = new Tuple<Vector3D, Vector3D, Vector3D>(e1, e2, e3);
        }

        public double GetAngleInRadians(double angle)
        {
            return angle * 0.01745;
        }

        public void SetCameraAngles(double a = 0, double b = 90)
        {
            AlphaCam = GetAngleInRadians(a);
            BetaCam = GetAngleInRadians(b);
        }
        private void ResetTransformationMatrix()
        {
            a = e = j = s = 1;
            b = c = p = d = f = q = h = i = r = l = m = n = 0;
        }
        private void Transform()
        {
            double[,] transformationMatrix =
                {   { a, b, c, p },
                    { d, e, f, q },
                    { h, i, j, r },
                    { l, m, n, s },};
            for (int i = 0; i < PointCount(); i++)
            {
                int[,] coordMatrix = { { X(i), Y(i), Z(i), 1 } };
                int[,] res = MatrixMultiplication(coordMatrix, transformationMatrix);
                Points[i] = new Point3D(res[0, 0], res[0, 1], res[0, 2]);
            }
            ResetTransformationMatrix();
        }
        protected int[,] MatrixMultiplication(int[,] a, double[,] b)
        {
            if (a.GetLength(1) != b.GetLength(0)) throw new Exception("Матрицы нельзя перемножить");
            int[,] r = new int[a.GetLength(0), b.GetLength(1)];
            for (int i = 0; i < a.GetLength(0); i++)
            {
                for (int j = 0; j < b.GetLength(1); j++)
                {
                    double tmpVal = 0;
                    for (int k = 0; k < b.GetLength(0); k++)
                    {
                        tmpVal += a[i, k] * b[k, j];
                    }
                    r[i, j] = (int)Math.Round(tmpVal);
                }
            }
            return r;
        }
        protected Vector3D VectorMultiplication(Vector3D e1, Vector3D e2)
        {
            double i, j, k;
            i = (e1.Item2 * e2.Item3) - (e1.Item3 * e2.Item2);
            j = (-1) * ((e1.Item1 * e2.Item3) - (e1.Item3 * e2.Item1));
            k = (e1.Item1 * e2.Item2) - (e1.Item2 * e2.Item1);
            return new Vector3D(i, j, k);
        }

        protected double ScalarMultiplication(Vector3D e1, Point3D e2)
        {
            return e1.Item1 * e2.Item1 + e1.Item2 * e2.Item2 + e1.Item3 * e2.Item3;
        }
        protected double ScalarMultiplication(Vector3D e1, Vector3D e2)
        {
            return e1.Item1 * e2.Item1 + e1.Item2 * e2.Item2 + e1.Item3 * e2.Item3;
        }

        private void FillShape(Pen drawPen, List<Point> face)
        {
            reInitLinePoints();
            FindExtremeYs(face);
            for (int y = extYs.Item1; y <= extYs.Item2; y++)
            {
                int sectionYMin, sectionYMax;
                bool secondPointYIsSmaller;
                for (int pt = 0; pt < face.Count; pt++)
                {
                    int ptNext = (pt + 1) != face.Count ? (pt + 1) : 0;
                    if (face[pt].Y == face[ptNext].Y)
                    {
                        continue;
                    }
                    if (face[pt].Y < face[ptNext].Y)
                    {
                        sectionYMin = Cam(pt).Y;
                        sectionYMax = Cam(ptNext).Y;
                        secondPointYIsSmaller = false;
                    }
                    else
                    {
                        sectionYMin = Cam(ptNext).Y;
                        sectionYMax = Cam(pt).Y;
                        secondPointYIsSmaller = true;
                    }

                    if (y >= sectionYMin && y <= sectionYMax)
                    {
                        if (Cam(pt).X - Cam(ptNext).X == 0)
                        {
                            LinePointsForFig[y].Add(new Point(Cam(pt).X, y));
                        }
                        else if (y != sectionYMin)
                        {
                            //вычислим два уравнения прямой вида y = kx+b и найдём координаты пересечения. Для первой прямой tg = k = 0, y = b;
                            double k = (double)(sectionYMin - sectionYMax) / (secondPointYIsSmaller ? (Cam(ptNext).X - Cam(pt).X) : (Cam(pt).X - Cam(ptNext).X));
                            double b = Cam(pt).Y - k * Cam(pt).X;
                            //абсцисса точки пересечения двух прямых
                            double tmpCross = ((b - y) / (0 - k));
                            int xCross = (int)Math.Round(tmpCross);
                            LinePointsForFig[y].Add(new Point(xCross, y));
                        }
                    }
                }
            }
            for (int line = extYs.Item1; line <= extYs.Item2; line++)
            {
                Point prevPoint = new Point();
                LinePointsForFig[line].Sort(PointSorter);
                foreach (Point pt in LinePointsForFig[line])
                {
                    if (!prevPoint.IsEmpty)
                    {
                        frm.g.DrawLine(drawPen, prevPoint.X, prevPoint.Y, pt.X, pt.Y);
                        prevPoint = pt;
                        //prevPoint = new Point();
                    }
                    else
                    {
                        prevPoint = pt;
                    }
                }
            }
        }
        private void FindExtremeYs(List<Point> face)
        {
            int tmpYMin = face.First().Y, tmpYMax = face.Last().Y;
            foreach (Point p in face)
            {
                tmpYMax = p.Y > tmpYMax ? p.Y : tmpYMax;
                tmpYMin = p.Y < tmpYMin ? p.Y : tmpYMin;
            }
            extYs = new Tuple<int, int>(tmpYMin + Center.Y, tmpYMax + Center.Y);
        }
        private static int PointSorter(Point a, Point b)
        {
            return a.X.CompareTo(b.X);
        }
    }

}
