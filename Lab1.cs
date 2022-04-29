using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Media3D;

namespace CG2_lab1
{
    public partial class Form1 : Form
    {

        Bitmap _bitmap;
        List<Point> _points = new List<Point>();

        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;
            _bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Application.StartupPath;
            saveFileDialog.Filter = @"Json file (*.json)|*.json";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                StreamWriter sw = File.CreateText(saveFileDialog.FileName);
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.Formatting = Formatting.Indented;
                var json = JsonSerializer.Create(settings); //запись координат точек в файл
                json.Serialize(sw, _points);
                sw.Close();
            }
        }

        private void LoadBtn_Click(object sender, EventArgs e)
        {
        OpenFileDialog ofd = new OpenFileDialog(); //открыть проводник для выбора файла
            ofd.InitialDirectory = Application.StartupPath;
            ofd.Filter = @"Json file (*.json)|*.json";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                StreamReader sr = File.OpenText(ofd.FileName); //считываем из файла координаты
                JsonSerializer jsonSerializer = new JsonSerializer();
                _points = (List<Point>)jsonSerializer.Deserialize(sr, typeof(List<Point>)); // перевод записи из json в список точек
                sr.Close();
                if (_points != null && _points.Count > 0)
                {
                    for (int i = 1; i < _points.Count; i++)
                    {
                        DrawLine(_points[i - 1], _points[i], Color.Black);
                    }
                    DrawLine(_points.First(), _points.Last(), Color.Black);
                    pictureBox1.Image = _bitmap;
                }
                else MessageBox.Show(@"Points not found");
            }
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                _points.Add(new Point(e.X, e.Y));
                _bitmap.SetPixel(e.X, e.Y, Color.Black);
            }
            if(e.Button == MouseButtons.Right) //тут натыкать точек для рисования линий
            {
                _bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                for (int i = 1; i < _points.Count; i++)
                {
                    DrawLine(_points[i - 1], _points[i], Color.Black);
                }
                DrawLine(_points.First(), _points.Last(), Color.Black);
            }
            pictureBox1.Image = _bitmap;
        }

        private void MoveObject(int dx, int dy)
        {
            int[,] moveMatrix = {
                                {1, 0, dx}, 
                                {0, 1, dy}, 
                                {0, 0, 1}
                                }; //матрица поворота
            for(int i = 0; i < _points.Count; i++)
            {
                //_points[i] = new Point(_points[i].X + dx, _points[i].Y + dy);
                _points[i] = MatrixToPoint(MultiplyMatrix(moveMatrix, PointToMatrix(_points[i])));
            }
        }

        private void ScaleObject(double scaleСoef) //формула с пары
        {
            double[,] scaleMatrix =
            {
                {scaleСoef, 0, 0}, 
                {0, scaleСoef, 0 }, 
                {0, 0, 1}
            };
            Point prevCenter = FindCenter();
			MoveObject(-prevCenter.X, -prevCenter.Y);
            for (int i = 0; i < _points.Count; i++)
            {
                _points[i] = MatrixToPoint(MultiplyMatrix(scaleMatrix, PointToMatrix(_points[i])));
            }
            MoveObject(prevCenter.X, prevCenter.Y);
        }

        private void RotateObject(int angle)
        {
            double[,] rotateMatrix =
            {
                {Math.Cos(angle * Math.PI / 180), Math.Sin(angle * Math.PI / 180), 0},
                {-Math.Sin(angle * Math.PI / 180), Math.Cos(angle * Math.PI / 180), 0},
                {0, 0, 1} //формула с прошлого семестра
            };
            Point prevCenter = FindCenter();
            for (int i = 0; i < _points.Count; i++)
            {
                // double x = _points[i].X * Math.Cos(angle * Math.PI / 180) - _points[i].Y * Math.Sin(angle * Math.PI / 180);
                // double y = _points[i].X * Math.Sin(angle * Math.PI / 180) + _points[i].Y * Math.Cos(angle * Math.PI / 180);
                // _points[i] = new Point((int)x, (int)y);
                _points[i] = MatrixToPoint(MultiplyMatrix(rotateMatrix, PointToMatrix(_points[i])));
            }
            Point newCenter = FindCenter();
            MoveObject(prevCenter.X - newCenter.X, prevCenter.Y - newCenter.Y);
        }

        private Point FindCenter()
        {
            Point center = new Point(); 
            for(int i = 0; i < _points.Count;i++)
            {
                center.X += _points[i].X; //для нахождения центра делим сумму координат на их количество
                center.Y += _points[i].Y;
            }
            center.X = center.X / _points.Count;
            center.Y = center.Y / _points.Count;
            return center;
        }

        private void DrawLine(Point first, Point second, Color color) //алг брезенхема
        {
            // Сам алгоритм был дан на лекции
            int dx = Math.Abs(second.X - first.X); //Модуль для того, чтобы не было отрицательного значения, если точки слева направо(по координатам)
            int dy = Math.Abs(second.Y - first.Y);
            int sx, sy; // знаки для перемещения по координатам
            if (second.X >= first.X) sx = 1; // если справа налево по иксу
            else sx = -1; // и наоборот для икса
            if (second.Y >= first.Y) sy = 1; // сверху вниз по y
            else sy = -1; // и наоборот для y
            if (dy < dx) // если угол прямой от нуля до 45 градусов относительно координатной оси Ox
            {
                // сам алгоритм взят из лекции
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
                    if(x > 0 && x < _bitmap.Width && y > 0 && y < _bitmap.Height)
                        _bitmap.SetPixel(x, y, color);
                    x += sx;
                }
            }
            else // если угол от 45 до 90
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

        private void DrawObject()
        {
            _bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            for (int i = 1; i < _points.Count; i++)
            {
                DrawLine(_points[i - 1], _points[i], Color.Black);
            }
            DrawLine(_points.First(), _points.Last(), Color.Black);
        
        pictureBox1.Image = _bitmap;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                // Move
                case Keys.W:
                    MoveObject(0, -1);
                    DrawObject();
                    break;
                case Keys.S:
                    MoveObject(0, 1);
                    DrawObject();
                    break;
                case Keys.A:
                    MoveObject(-1, 0);
                    DrawObject();
                    break;
                case Keys.D:
                    MoveObject(1, 0);
                    DrawObject();
                    break;
                // Scale
                case Keys.Z:
                    ScaleObject(0.9);
                    DrawObject();
                    break;
                case Keys.X:
                    ScaleObject(1.1);
                    DrawObject();
                    break;
                // Rotate
                case Keys.Q:
                    RotateObject(-5);
                    DrawObject();
                    break;
                case Keys.E:
                    RotateObject(5);
                    DrawObject();
                    break;
            }
           
        }

        private void reset_Click(object sender, EventArgs e)
        {
            _points.Clear();
            _bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = _bitmap;
        }
        
        
        private int[,] MultiplyMatrix(int[,] first, int[,] second)
        {
            int firstRows = first.GetLength(0);
            int firstCols = first.GetLength(1);
            int secondCols = second.GetLength(1);
            int[,] result = new int[firstRows, secondCols];
            for (int i = 0; i < firstRows; i++)
            {
                for (int j = 0; j < secondCols; j++)
                {
                    for(int k = 0; k < firstCols; k++)
                        result[i,j] += first[i,k] * second[k,j];
                    //result[i,j] = first[i,0] * second[0,j] + first[i,1]*second[1,j] + first[i,2]*second[2,j];
                }
            }
            /*
            for (int i = 0; i < firstRows; i++)
            {
                for (int j = 0; j < secondCols; j++)
                {
                    Console.Write(result[i,j] + " ");
                }
                Console.WriteLine();
            }
            */
            return result;
        }
        
        private int[,] MultiplyMatrix(double[,] first, int[,] second)
        {
            int firstRows = first.GetLength(0);
            int firstCols = first.GetLength(1);
            int secondCols = second.GetLength(1);
            int[,] result = new int[firstRows, secondCols];
            for (int i = 0; i < firstRows; i++)
            {
                for (int j = 0; j < secondCols; j++)
                {
                    for(int k = 0; k < firstCols; k++)
                        result[i,j] += (int)(first[i,k] * second[k,j]);
                    //result[i,j] = first[i,0] * second[0,j] + first[i,1]*second[1,j] + first[i,2]*second[2,j];
                }
            }
            return result;
        }

        private Point MatrixToPoint(int[,] matrix)
        {
            return new Point(matrix[0, 0], matrix[1, 0]);
        }
        
        private int[,] PointToMatrix(Point point)
        {
            return new int[,] {{point.X}, {point.Y}, {1}};
        }
        
    }
}
