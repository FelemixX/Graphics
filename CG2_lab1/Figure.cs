using System;
using System.Collections.Generic;
using System.Drawing;

namespace CG2_lab1
{
    public class Figure
    {
        private List<Point> _points;

        public List<Point> ToList => _points;

        public Figure(List<Point> points)
        {
            foreach (Point p in points)
            {
                if (_points != null) _points.Add(p);
            }
        }

        public void Move(int dx, int dy)
        {
            
        }

        public void Scale(double scaleFactor)
        {
            
        }

        public void Rotate(int angle)
        {
            
        }
        
        // Utils

        private Point FindCenter()
        {
            Point center = new Point(); 
            foreach (var point in _points)
            {
                center.X += point.X;
                center.Y += point.Y;
            }
            center.X = center.X / _points.Count;
            center.Y = center.Y / _points.Count;
            return center;
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

        private Point MatrixToPoint(int[,] matrix)
        {
            return new Point(matrix[0, 0], matrix[0, 1]); //TODO Test!!!
        }
        
        private int[,] PointToMatrix(Point point)
        {
            return new int[,] {{point.X}, {point.Y}, {1}};
        }
        
    }
}