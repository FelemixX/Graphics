using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CG2_lab2
{
    public partial class Form1 : Form
    {
        List<Point> _points = new List<Point>();
        private Bitmap _bitmap;

        public Form1()
        {
            InitializeComponent();
            _bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _points.Add(new Point(e.X, e.Y));
                _bitmap.SetPixel(e.X, e.Y, Color.Black);
                pictureBox1.Image = _bitmap;
            }

            if (e.Button == MouseButtons.Right)
                //Bezier_curve();
                //Casteljau_curve();
        }

       /* void Bezier_curve()
        {
            List<Point> curvePoints = new List<Point>();
            if (_points.Count > 1) //если точек больше одной, то можно рисовать
            {
                for (double t = 0; t <= 1; t += 0.00001)
                {
                    int x = 0, y = 0;
                    for (int i = 0; i < _points.Count; i++)
                    {
                        x += (int)(_points[i].X * Ckn(_points.Count - 1, i) * Math.Pow(t, i)*Math.Pow((1-t), _points.Count-i - 1)); //формула для кривой безье 
                        y += (int)(_points[i].Y * Ckn(_points.Count - 1, i) * Math.Pow(t, i)*Math.Pow((1-t), _points.Count-i - 1)); // из учебника Постнова К.В. 2009 г.
                    }

                    curvePoints.Add(new Point(x, y));
                    if (x < _bitmap.Width && y < _bitmap.Height) //если не выходим за границу холста, то точку можно поставить, иначе ошибка
                    {
                        _bitmap.SetPixel(x, y, Color.Blue);
                        pictureBox1.Image = _bitmap;
                    }
                }

                //DrawLines(curvePoints);
            }
        } */

       /*void Casteljau_curve()
       {
           List<Point> curvePoints = new List<Point>();
           if (_points.Count > 1) //если точек больше одной, то можно рисовать
           {
               for (double t = 0; t <= 1; t += 0.01)
               {
                   int x = 0, y = 0;
                   for (int i = 0; i < _points.Count; i++)
                   {
                       x += (int)(_points[i].X + t * (_points[i + 1].X - _points[i].X));
                       y += (int)(_points[i].Y + t * (_points[i + 1].Y - _points[i].Y));
                   }

                   curvePoints.Add(new Point(x, y));
                   if (x < _bitmap.Width && y < _bitmap.Height) //если не выходим за границу холста, то точку можно поставить, иначе ошибка
                   {
                       _bitmap.SetPixel(x, y, Color.Blue);
                       pictureBox1.Image = _bitmap;
                   }
               }
           }
       } */

        int Ckn(int n, int k)
        {
            return (int)(Factorial(n)/(Factorial(k)*Factorial(n-k))); // n!/(k!*(n-k)!)
        }

        int Factorial(int n) //вычисление факториала 
        {
            int result = 1; //да, оказывается в шарпе нельзя считать факториал...
            for (int i = 1; i <= n; i++)
                result *= i;
            return result;
        }

        void DrawLines(List<Point> points)
        {
            for (int i = 1; i < points.Count; i++)
            {
                DrawLine(points[i - 1], points[i], Color.Black); //тупо кидаем все в алгоритм брезенхема. Рисуем начиная от предыдущей относительно последней поставленной точки
            }

            pictureBox1.Image = _bitmap;
        }

        private void DrawLine(Point first, Point second, Color color) //Алгоритм брезенхема для рисования линии
        {
            int dx = Math.Abs(second.X - first.X);
            int dy = Math.Abs(second.Y - first.Y);
            int sx, sy;
            if (second.X >= first.X) sx = 1;
            else sx = -1;
            if (second.Y >= first.Y) sy = 1;
            else sy = -1;
            if (dy < dx)
            {
                int d = (dy << 1) - dx;
                int d1 = dy << 1;
                int d2 = (dy - dx) << 1;
                if (first.X > 0 && first.X < _bitmap.Width && first.Y > 0 && first.Y < _bitmap.Height)
                    _bitmap.SetPixel(first.X, first.Y, color);
                int x = first.X + sx;
                int y = first.Y;
                for (int i = 1; i <= dx; i++)
                {
                    if (d > 0)
                    {
                        d += d2;
                        y += sy;
                    }
                    else
                        d += d1;

                    if (x > 0 && x < _bitmap.Width && y > 0 && y < _bitmap.Height)
                        _bitmap.SetPixel(x, y, color);
                    x += sx;
                }
            }
            else
            {
                int d = (dx << 1) - dy;
                int d1 = dx << 1;
                int d2 = (dx - dy) << 1;
                if (first.X > 0 && first.X < _bitmap.Width && first.Y > 0 && first.Y < _bitmap.Height)
                    _bitmap.SetPixel(first.X, first.Y, color);
                int x = first.X;
                int y = first.Y + sy;
                for (int i = 1; i <= dy; i++)
                {
                    if (d > 0)
                    {
                        d += d2;
                        x += sx;
                    }
                    else
                        d += d1;

                    if (x > 0 && x < _bitmap.Width && y > 0 && y < _bitmap.Height)
                        _bitmap.SetPixel(x, y, color);
                    y += sy;
                }
            }
        }

    }
}
