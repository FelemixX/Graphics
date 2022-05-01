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
        Graphics g;
        Surface surface;
        Pen penMain;
        protected static Form1 instance;

        public Form1()
        {
            InitializeComponent();
            g = pictureBox1.CreateGraphics();
            penMain = new Pen(Brushes.Black);
            instance = this;
            surface = new Surface();
        }

        public static Form1 getInstance()
        {
            return instance;
        }

        private void onExitBtnClick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void onClearBtnClick(object sender, EventArgs e)
        {
            surface = new Surface();
            g.Clear(pictureBox1.BackColor);
        }

        private void drawCasual(object sender, EventArgs e)
        {
            g = pictureBox1.CreateGraphics();
            surface.InitCenter(pictureBox1.Width, pictureBox1.Height);
            surface.drawCasual();
        }

        private void drawParam(object sender, EventArgs e)
        {
            g = pictureBox1.CreateGraphics();
            surface.InitCenter(pictureBox1.Width, pictureBox1.Height);
            surface.drawParam();
        }

        public void drawLine(Point p1, Point p2)
        {
            Random rnd = new Random();
            penMain = new Pen(Color.FromArgb(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255)));
            if (p1.X > surface.a* 100 && p1.X < surface.b* 100 && p1.Y > surface.c * 100 && p1.Y < surface.d * 100)
            {
                if (p2.X > surface.a * 100 && p2.X < surface.b * 100 && p2.Y > surface.c * 100 && p2.Y < surface.d * 100)
                {
                    g.DrawLine(penMain, p1, p2);
                }
            }
        }
    }

    class Surface
    {
        private double phi, psi;
        public double a, b, c, d;
        int Nx, Ny;
        Vector3D e1, e2, e3;
        Point Center;

        public Surface()
        {
            init();
        }
        public void init()
        {
            Nx = Ny = 90;
            a = -300.0;
            b = 300.0;
            c = -300.0;
            d = 300.0;
            Center = new Point(0, 0);
            SetCameraAngles(50, 10);
            InitCamera();
        }

        protected double casualSurfaceFormula(double x, double y)
        {
            return Math.Sin(6 * y) * Math.Sin(4 * x);
        }

        protected Point3D paramSurfaceFormula(double u, double v)
        {
            double x, y, z;
            int a = 500;
            int b = 500;
            int c = 300;
            //x = a * Math.Sin(u) * Math.Cos(v);
            //y = a * Math.Sin(u) * Math.Sin(v);
            //z = a * (Math.Cos(u) + Math.Log(Math.Tan(u / 2)));
            x = a * Math.Cos(u) * Math.Cos(v);
            y = b * Math.Cos(u) * Math.Sin(v);
            z = c * Math.Sin(u);
            return new Point3D((int)Math.Round(x), (int)Math.Round(y), (int)Math.Round(z));
        }

        public void drawCasual()
        {
            double x, y, z, dx, dy;
            dx = (b - a) / Nx;
            dy = (d - c) / Ny;
            Point3D begin, current;
 
            int u0, v0, v1, u1;
            //рисуем вертикальную систему координатных кривых на поверхности
            for (x = a; x <= b; x += dx)
            {
                y = c;
                z = casualSurfaceFormula(x, y);
                begin = new Point3D((int)Math.Round(x), (int)Math.Round(y), (int)Math.Round(z));
                u0 = (int)Math.Round(ScalarMultiplication(e1, begin));
                v0 = (int)Math.Round(ScalarMultiplication(e2, begin));
                for (y = c; y <= d; y += dy)
                {
                    z = casualSurfaceFormula(x, y);
                    current = new Point3D((int)Math.Round(x), (int)Math.Round(y), (int)Math.Round(z));
                    u1 = (int)Math.Round(ScalarMultiplication(e1, current));
                    v1 = (int)Math.Round(ScalarMultiplication(e2, current));
                    Form1.getInstance().drawLine(new Point(Center.X + u0, Center.Y + v0), new Point(Center.X + u1, Center.Y + v1));
                    u0 = u1;
                    v0 = v1;
                }
            }
            //рисуем вертикальную систему координатных кривых на поверхности
            for (y = c; y <= d; y += dy)
            {
                x = a;
                z = casualSurfaceFormula(x, y);
                begin = new Point3D((int)Math.Round(x), (int)Math.Round(y), (int)Math.Round(z));
                u0 = (int)Math.Round(ScalarMultiplication(e1, begin));
                v0 = (int)Math.Round(ScalarMultiplication(e2, begin));
                for (x = a; x <= b; x += dx)
                {
                    z = casualSurfaceFormula(x, y);
                    current = new Point3D((int)Math.Round(x), (int)Math.Round(y), (int)Math.Round(z));
                    u1 = (int)Math.Round(ScalarMultiplication(e1, current));
                    v1 = (int)Math.Round(ScalarMultiplication(e2, current));
                    Form1.getInstance().drawLine(new Point(Center.X+u0, Center.Y + v0), new Point(Center.X + u1, Center.Y + v1));
                    u0 = u1;
                    v0 = v1;
                }
            }
        }

        public void drawParam()
        {
            double u, v, du, dv;
            du = (b - a) / Nx;
            dv = (d - c) / Ny;
            Point3D begin, current;

            int u0, v0, v1, u1;
            //рисуем вертикальную систему координатных кривых на поверхности
            for (u = a; u <= b; u += du)
            {
                v = c;
                begin = paramSurfaceFormula(u, v);
                u0 = (int)Math.Round(ScalarMultiplication(e1, begin));
                v0 = (int)Math.Round(ScalarMultiplication(e2, begin));
                for (v = c; v <= d; v += dv)
                {
                    current = paramSurfaceFormula(u, v);
                    u1 = (int)Math.Round(ScalarMultiplication(e1, current));
                    v1 = (int)Math.Round(ScalarMultiplication(e2, current));
                    //Console.WriteLine(u0 + " " + v0 + " " + u1 + " " + v1);
                    Form1.getInstance().drawLine(new Point(Center.X + u0, Center.Y + v0), new Point(Center.X + u1, Center.Y + v1));
                    u0 = u1;
                    v0 = v1;
                }
            }
            //рисуем вертикальную систему координатных кривых на поверхности
            for (v = c; v <= d; v += dv)
            {
                u = a;
                begin = paramSurfaceFormula(u, v);
                u0 = (int)Math.Round(ScalarMultiplication(e1, begin));
                v0 = (int)Math.Round(ScalarMultiplication(e2, begin));
                for (u = a; u <= b; u += du)
                {
                    current = paramSurfaceFormula(u, v);
                    u1 = (int)Math.Round(ScalarMultiplication(e1, current));
                    v1 = (int)Math.Round(ScalarMultiplication(e2, current));
                    Form1.getInstance().drawLine(new Point(Center.X + u0, Center.Y + v0), new Point(Center.X + u1, Center.Y + v1));
                    u0 = u1;
                    v0 = v1;
                }
            }
        }

        public double GetAngleInRadians(double angle)
        {
            return angle * 0.01745;
        }

        public void SetCameraAngles(double a = 0, double b = 90)
        {
            phi = GetAngleInRadians(a);
            psi = GetAngleInRadians(b);
        }
        public void InitCamera()
        {
            double x, y, z;
            x = Math.Cos(phi);
            y = Math.Sin(phi);
            z = 0;
            e1 = new Vector3D(x, y, z);
            x = (-1) * Math.Sin(phi) * Math.Sin(psi);
            y = Math.Cos(phi) * Math.Sin(psi);
            z = Math.Cos(psi);
            e2 = new Vector3D(x, y, z);
            e3 = VectorMultiplication(e1, e2);
        }

        public void InitCenter(int picWidth, int picHeight)
        {
            Center = new Point();
            Center.X = picWidth / 2;
            Center.Y = picHeight / 2;
        }
        public Vector3D VectorMultiplication(Vector3D e1, Vector3D e2)
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

    }

}
