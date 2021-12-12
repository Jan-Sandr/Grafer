using System;
using System.Collections.Generic;
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
        private string[] relationBackup = Array.Empty<string>(); // Záloha předpisu.
        private int[] calculationOrderIndexesBackup = Array.Empty<int>(); // záloha výpočetního postupu.
        private double calculationMinimumX, calculationMaximumX; // Výpočetní minimum a maximum.

        public Function(string relation, double minimumX, double maximumX, Canvas canvas)
        {
            Relation = new(relation);
            MinimumX = minimumX;
            MaximumX = maximumX;
            CalculationOrder = new();
            Curves = new List<Polyline>();
            points = new List<Point>();
            Canvas = canvas;
        }

        //Výpočítání křivky.
        public void CalculatePoints()
        {
            PrepareForCalculation();
            DoCalculation();
            SaveCurve(points);
            points = new List<Point>();
        }

        //Výpoočet bodů.
        private void DoCalculation()
        {
            for (double x = calculationMinimumX; x <= calculationMaximumX; x += 0.01)
            {
                x = Math.Round(x, 2);

                GetBackup();

                SubstituteX(x);

                y = (Relation.Count > 1) ? CalculateYForX() : double.Parse(Relation[0]);

                SavePoint(x, y);
            }

        }

        //Příprava pro výpoočet.
        private void PrepareForCalculation()
        {
            CalculationOrder.Create(Relation);
            SetBackup();
            SetCalculationXRange();
        }

        //Omezení y pro případ velkých čísel.
        private double LimitY()
        {
            if (!double.IsInfinity(y))
            {
                if (y > 1000000)
                {
                    y = 1000000;
                }

                if (y < -1000000)
                {
                    y = -1000000;
                }
            }

            return y;
        }

        //Vykreslení funkce do plátna.
        public void Plot()
        {
            for (int i = 0; i < Curves.Count; i++)
            {
                Canvas.Children.Add(Curves[i]);
            }
        }

        //Nastavení výpočetního rozsahu. Pokud by byl rozsah větší než plátno, omezí to jen na viditelnou plochu interně.
        private void SetCalculationXRange()
        {
            calculationMinimumX = (Math.Abs(MinimumX) > Canvas.Width / 200) ? -(Canvas.Width / 200) : MinimumX;
            calculationMaximumX = (MaximumX > Canvas.Width / 200) ? Canvas.Width / 200 : MaximumX;
        }

        //Dosazení za x.
        private void SubstituteX(double x)
        {
            for (int i = 0; i < Relation.Count; i++)
            {
                Relation[i] = Relation[i] == "x" ? x.ToString() : Relation[i];
                Relation[i] = Relation[i] == "-x" ? (-x).ToString() : Relation[i];
            }
        }

        //Výpočet y pro x.
        private double CalculateYForX()
        {
            int orderProgression = 0;

            while (Relation.Count > 1)
            {
                y = CalculateY(orderProgression);
                orderProgression++;
            }

            y = Math.Abs(y) > 1000000 ? LimitY() : y;

            return y;
        }

        //Výpočet y.
        private double CalculateY(int orderProgression)
        {
            int index = CalculationOrder.Indexes[orderProgression];

            Relation[index] = Operation(index).ToString();

            Relation.RemoveNeighbors(index);

            CalculationOrder.ShiftPosition(Relation.RemovedElementsCount, orderProgression);

            Relation.RemovedElementsCount = 0;

            return y;
        }

        //Uložení bodu.
        private void SavePoint(double x, double y)
        {
            if (!double.IsNaN(y) && !double.IsInfinity(y)) // Pokud bod není definovaný, vzniká mezera.
            {
                Point point = new(x, y);
                point = ConvertToCoordinatePoint(point);
                points.Add(point);
            }
            else if (points.Count > 0)
            {
                if (points.Count > 1) // Křivka minimálně ze 2 bodů.
                {
                    SaveCurve(points);
                }

                points = new List<Point>();
            }
        }

        //Uložení křivky.
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

        //Převedení bodu na bod do soustavy.
        private Point ConvertToCoordinatePoint(Point point)
        {
            point = new Point()
            {
                X = Math.Round(Canvas.Width / 2 + point.X * 100, 2),
                Y = (-y * 100) + Canvas.Height / 2
            };
            return point;
        }

        //Operace mezi 2 členy v předpisu.
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
                case "^":
                    {
                        y = Math.Pow(double.Parse(Relation[index - 1]), double.Parse(Relation[index + 1]));
                        break;
                    }
            }

            return y;
        }

        //Vytvoření zálohy pro výpočet.
        private void SetBackup()
        {
            relationBackup = Relation.ToArray();
            calculationOrderIndexesBackup = new int[CalculationOrder.Indexes.Length];
            CalculationOrder.Indexes.CopyTo(calculationOrderIndexesBackup, 0);
        }

        //Načtení zálohy.
        private void GetBackup()
        {
            Relation = new();
            Relation.AddRange(relationBackup);
            calculationOrderIndexesBackup.CopyTo(CalculationOrder.Indexes, 0);
        }
    }
}
