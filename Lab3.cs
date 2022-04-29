using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lab3spline
{
    public partial class Form1 : Form
    {
        Graphics g;
        List<Point> points;
        Pen penMain, penSecondary, penBlue;
        int listSize = 255;
        public Form1()
        {
            InitializeComponent();
            g = pictureBox1.CreateGraphics();
            points = new List<Point>(listSize);
            penMain = new Pen(Brushes.Black);
            penSecondary = new Pen(Brushes.Red);
            penBlue = new Pen(Brushes.Blue);
        }

        void spline2()
        {
            float oldX, oldY, newX, newY, u;
            for (int i = 0; i < points.Count - 2; i++)
            {
                oldX = (points[i].X + points[i + 1].X) / 2;
                oldY = (points[i].Y + points[i + 1].Y) / 2;
                for (u = 0; u <= 1; u += 0.01F)
                {
                    newX = 0.5F * (1F - u) * (1F - u) * points[i].X + (0.75F - (u - 0.5F) * (u - 0.5F)) * points[i + 1].X + 0.5F * u * u * points[i + 2].X; //Сплайн второго порядка
                    newY = 0.5F * (1F - u) * (1F - u) * points[i].Y + (0.75F - (u - 0.5F) * (u - 0.5F)) * points[i + 1].Y + 0.5F * u * u * points[i + 2].Y;
                    double tmpX = Math.Round(oldX);
                    double tmpY = Math.Round(oldY);
                    double tmpXnew = Math.Round(newX);
                    double tmpYnew = Math.Round(newY);
                    g.DrawLine(penBlue, new Point((int)tmpX, (int)tmpY), new Point((int)tmpXnew, (int)tmpYnew));
                    oldX = newX;
                    oldY = newY;
                }
            }
        }

        private void onMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                drawCurvePart(e.X, e.Y, penMain);
            }
            else
            {
                spline2();
                g.Dispose();
                points.Clear();
                g = pictureBox1.CreateGraphics();
            }
        }


        private void drawCurvePart(int x, int y, Pen pen)
        {
            g.DrawRectangle(pen, x, y, 1, 1);
            if (points.Count > 0)
            {
                g.DrawLine(penMain, points.Last(), new Point(x, y));
            }
            points.Add(new Point(x, y));
        }
    }
}
