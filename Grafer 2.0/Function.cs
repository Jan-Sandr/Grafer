using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Grafer2
{
    public class Function
    {
        public Relation Relation { get; set; }
        public double MinimumX { get; set; }
        public double MaximumX { get; set; }
        public CalculationOrder CalculationOrder { get; set; }
        public List<Polyline> Curves { get; set; }
        public Canvas Canvas { get; }
        private List<Point> points;
        private double y;

        private string[] relationBackup = Array.Empty<string>();
        readonly private int[][] calculationOrderBackup = new int[2][];
        private double calculationMinimumX, calculationMaximumX;


        public Function(string relation, double minimumX, double maximumX, Canvas canvas)
        {
            Relation = new();
            Relation.AddRange(relation.Select(s => s.ToString()));
            MinimumX = minimumX;
            MaximumX = maximumX;
            CalculationOrder = new CalculationOrder() { new List<int>(), new List<int>() };
            Curves = new List<Polyline>();
            points = new List<Point>();
            Canvas = canvas;
        }

        public void PrepareForCalculation()
        {
            Relation = Relation.Adjust(Relation);
            CalculationOrder = CalculationOrder.GetOrder(Relation, CalculationOrder);
        }

        public void CalculatePoints()
        {
            SetBackup();
            SetCalculationXRange();
            for (double x = calculationMinimumX; x <= calculationMaximumX; x += 0.01)
            {
                x = Math.Round(x, 2);
                GetBackup();
                SubstituteX(x);
                y = (Relation.Count > 1) ? x : double.Parse(Relation[0]);
                CalculateYForX();

                if(y > 1000000)
                {
                    y = 1000000;
                }

                if(y < -1000000)
                {
                    y = -1000000;
                }

                SavePoint(x, y);
            }

            SaveCurve(points);
            points = new List<Point>();
        }

        public void Plot()
        {
            for (int i = 0; i < Curves.Count; i++)
            {
                Canvas.Children.Add(Curves[i]);
            }
        }

        private void SetCalculationXRange()
        {
            calculationMinimumX = (Math.Abs(MinimumX) > Canvas.Width / 200) ? -(Canvas.Width / 200) : MinimumX;
            calculationMaximumX = (MaximumX > Canvas.Width / 200) ? Canvas.Width / 200 : MaximumX;
        }

        private void SubstituteX(double x)
        {
            for (int i = 0; i < Relation.Count; i++)
            {
                Relation[i] = Relation[i] == "x" ? x.ToString() : Relation[i];
                Relation[i] = Relation[i] == "-x" ? (-x).ToString() : Relation[i];
            }
        }

        private void CalculateYForX()
        {
            int orderProgression = 0;
            while (Relation.Count > 1)
            {
                int index = CalculationOrder[0][orderProgression];
                Relation[index] = Operation(index).ToString();
                Relation.RemoveNeighbors(Relation, index);
                CalculationOrder.ShiftPosition(CalculationOrder, Relation.RemovedElementsCount, orderProgression);
                Relation.RemovedElementsCount = 0;
                orderProgression++;
            }

        }

        private void SavePoint(double x, double y)
        {
            if (!double.IsNaN(y) && !double.IsInfinity(y))
            {
                Point point = new(x, y);
                point = ConvertToCoordinatePoint(point);
                points.Add(point);
            }
            else if (points.Count > 1)
            {
                SaveCurve(points);
                points = new List<Point>();
            }
        }

        private void SaveCurve(List<Point> points)
        {
            Polyline polyline = new()
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            polyline.SnapsToDevicePixels = true;

            for (int i = 0; i < points.Count; i++)
            {
                polyline.Points.Add(points[i]);
            }

            Curves.Add(polyline);
        }

        private Point ConvertToCoordinatePoint(Point point)
        {
            point = new Point()
            {
                X = Math.Round(Canvas.Width / 2 + point.X * 100,2),
                Y = (-y * 100) + Canvas.Height / 2
            };
            return point;
        }

        private double Operation(int index)
        {
            switch (Relation[index])
            {
                case "+":
                    {
                        y = double.Parse(Relation[index - 1]) + double.Parse(Relation[index + 1]);
                        break;
                    }
                case "-":
                    {
                        y = double.Parse(Relation[index - 1]) - double.Parse(Relation[index + 1]);
                        break;
                    }
                case "*":
                    {
                        y = double.Parse(Relation[index - 1]) * double.Parse(Relation[index + 1]);
                        break;
                    }
                case "/":
                    {
                        y = double.Parse(Relation[index - 1]) / double.Parse(Relation[index + 1]);
                        break;
                    }
            }

            return y;
        }

        private void SetBackup()
        {
            relationBackup = Relation.ToArray();
            calculationOrderBackup[0] = CalculationOrder[0].ToArray();
            calculationOrderBackup[1] = CalculationOrder[1].ToArray();
        }

        private void GetBackup()
        {
            ResetRelationAndOrder();

            Relation.AddRange(relationBackup);

            CalculationOrder[0] = new List<int>(calculationOrderBackup[0]);
            CalculationOrder[1] = new List<int>(calculationOrderBackup[1]);
        }

        private void ResetRelationAndOrder()
        {
            Relation = new();
            CalculationOrder = new();
            CalculationOrder.Add(new List<int>());
            CalculationOrder.Add(new List<int>());
        }
    }
}
