using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Grafer2
{
    public class Function
    {
        Relation Relation { get; set; }
        double MinimumX {  get; set; }
        double MaximumX {  get; set; }
        CalculationOrder CalculationOrder {  get; set; }
        List<List<Point>> Curves { get; set; }
        public Canvas Canvas { get; }
        private List<Point> points;
        double y;

        Relation relationBackup;
        CalculationOrder calculationOrderBackup;

        public Function(string relation,double minimumX, double maximumX, Canvas canvas)
        {
            Relation = new();
            Relation.AddRange(relation.Select(s => s.ToString()));
            MinimumX = minimumX;
            MaximumX = maximumX;
            CalculationOrder = new CalculationOrder() { new List<int>(), new List<int>() };
            Curves = new List<List<Point>>();
            points = new List<Point>();
            Canvas = canvas;
        }

        public void PrepareForCalculation()
        {
            CalculationOrder = CalculationOrder.GetOrder(Relation, CalculationOrder);
        }

        public List<List<Point>> CalculatePoints()
        {
            SetBackup();
            for(double x = MinimumX; x <= MaximumX; x += 0.01)
            {
                GetBackup();
                SubstituteX(x);
                y = x;
                CalculateYForX();
                SavePoint(x, y);
            }
          
            return Curves;
        }

        private void SubstituteX(double x)
        {
            for(int i = 0;i< Relation.Count;i++)
            {
                Relation[i] = Relation[i] == "x" ? x.ToString() : Relation[i];
                Relation[i] = Relation[i] == "-x" ? (-x).ToString() : Relation[i];
            }
        }

        private void CalculateYForX()
        {
            int orderProgression = 0;
            while(Relation.Count >1)
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
            if(!double.IsNaN(y))
            {
                Point point = new(x, y);
                point = ConvertToCoordinatePoint(point);
                points.Add(point);
            }
          
        }

        private Point ConvertToCoordinatePoint(Point point)
        {
            point = new Point()
            {
                X = Canvas.Width / 2 + point.X * 100,
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
            relationBackup = new();
            relationBackup.AddRange(Relation);
            calculationOrderBackup = new();
            calculationOrderBackup.AddRange(CalculationOrder);
        }

        private void GetBackup()
        {
            Relation = new();
            CalculationOrder = new();
            Relation.AddRange(relationBackup);
            CalculationOrder.AddRange(calculationOrderBackup);
        }
    }
}
