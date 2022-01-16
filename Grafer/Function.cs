using Grafer.CustomControls;
using Grafer.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Grafer
{
    public class Function
    {
        public Relation Relation { get; set; }
        public double MinimumX { get; set; }
        public double MaximumX { get; set; }
        public CalculationOrder CalculationOrder { get; set; }
        public List<Polyline> Curves { get; set; }
        public CoordinateSystem CoordinateSystem { get; }
        public Brush Brush { get; }
        public bool Inverse { get; private set; }
        public FunctionType Type { get; private set; }

        public int ErrorMessageID { get; private set; } = -1;

        public enum FunctionType // Typy funkce na základě vhodné míry pro konkrétní osu.
        {
            Basic,
            TrigonometricFunction,
            InverseTrigonometricFunction
        }

        private List<Point> points;
        private double y;
        private string[] relationBackup = Array.Empty<string>(); // Záloha předpisu.
        private int[] calculationOrderIndexesBackup = Array.Empty<int>(); // záloha výpočetního postupu.
        private double calculationMinimumX, calculationMaximumX; // Výpočetní minimum a maximum.

        public Function(string relation, double minimumX, double maximumX, CoordinateSystem coordinateSystem, Brush color, bool inverse)
        {
            Relation = new Relation(relation);
            MinimumX = minimumX;
            MaximumX = maximumX;
            CalculationOrder = new CalculationOrder();
            Curves = new List<Polyline>();
            points = new List<Point>();
            CoordinateSystem = coordinateSystem;
            Brush = color;
            Inverse = inverse;
            Type = GetFunctionType();
        }

        //Zjištení typu funkce.
        private FunctionType GetFunctionType()
        {
            FunctionType type = Relation.Contains("⁻¹") ? FunctionType.InverseTrigonometricFunction : FunctionType.Basic;

            if (type != FunctionType.InverseTrigonometricFunction)
            {
                for (int i = 0; i < Relation.Count; i++)
                {
                    if (Relation[i].IsTrigonometricFunction())
                    {
                        type = Inverse ? FunctionType.InverseTrigonometricFunction : FunctionType.TrigonometricFunction;
                        break;
                    }
                }
            }
            else
            {
                type = Inverse ? FunctionType.TrigonometricFunction : FunctionType.InverseTrigonometricFunction;
            }

            return type;
        }

        //Jestli je možné křivku udělat inverzní.
        private bool IsInvertible()
        {
            bool isInvertible = true;

            bool? isGrowing = Curves[0].Points[0].Y > Curves[0].Points[1].Y ? true : Curves[0].Points[0].Y < Curves[0].Points[1].Y ? false : (bool?)null;

            if (isGrowing != null)
            {
                for (int i = 0; i < Curves.Count && isInvertible; i++)
                {
                    for (int j = 1; j < Curves[i].Points.Count && isInvertible; j++)
                    {
                        if (isGrowing == true)
                        {
                            isInvertible = Curves[i].Points[j - 1].Y > Curves[i].Points[j].Y;
                            
                        }

                        if (isGrowing == false)
                        {
                            isInvertible = Curves[i].Points[j - 1].Y < Curves[i].Points[j].Y;
                        }
                    }

                }
            }
            else
            {
                isInvertible = false;
            }

            return isInvertible;
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
                GetBackup();

                x = SetX(x);

                ComputeY();

                SavePoint(x, y);
            }
        }

        //Nastevní aktuální hodnoty x v předpisu.
        private double SetX(double x)
        {
            x = Math.Round(x, 2);

            SubstituteX(x);

            return x;
        }

        //Vypočet y.
        private void ComputeY()
        {
            y = 0;

            y = (Relation.Count > 1) ? CalculateYForX() : double.Parse(Relation[0]);
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

        //Jestli lze funkce vykreslit.
        public bool IsDrawable()
        {
            if (!IsEmpty())
            {
                if (Inverse && !IsInvertible())
                {
                    ErrorMessageID = 26;
                }
            }

            return ErrorMessageID == -1;
        }

        //Jestli není funkce prádzná.
        private bool IsEmpty()
        {
            if (Curves[0].Points.Count == 0)
            {
                ErrorMessageID = 27;
            }

            return ErrorMessageID != -1;
        }

        //Vykreslení funkce do plátna.
        public void Plot()
        {
            for (int i = 0; i < Curves.Count; i++)
            {
                if (Inverse)
                {
                    CoordinateSystem.Children.Add(InvertCurve(Curves[i]));
                }
                else
                {
                    CoordinateSystem.Children.Add(Curves[i]);
                }

            }
        }

        //Invertuje body křivky.
        private Polyline InvertCurve(Polyline polyline)
        {
            for (int i = 0; i < polyline.Points.Count; i++)
            {
                polyline.Points[i] = InvertPoint(polyline.Points[i]);
            }

            return polyline;
        }

        //Invertuje bod.
        private Point InvertPoint(Point point)
        {
            return new Point()
            {
                X = CoordinateSystem.Width / 2 + CoordinateSystem.Height / 2 - point.Y + CoordinateSystem.AbsoluteShift.OnY + CoordinateSystem.AbsoluteShift.OnX,
                Y = CoordinateSystem.Height / 2 + CoordinateSystem.Width / 2 - point.X + CoordinateSystem.AbsoluteShift.OnY + CoordinateSystem.AbsoluteShift.OnX
            };
        }

        //Nastavení výpočetního rozsahu. Pokud by byl rozsah větší než plátno, omezí to jen na viditelnou plochu interně.
        private void SetCalculationXRange()
        {
            calculationMinimumX = ((-Math.Abs(MinimumX) > CoordinateSystem.NumberRange + (CoordinateSystem.AbsoluteShift.OnX / 100 / CoordinateSystem.Zoom)) ? -CoordinateSystem.NumberRange - CoordinateSystem.AbsoluteShift.OnX / 100 / CoordinateSystem.Zoom : MinimumX);
            calculationMaximumX = ((MaximumX > CoordinateSystem.NumberRange - (CoordinateSystem.AbsoluteShift.OnX / 100 / CoordinateSystem.Zoom)) ? CoordinateSystem.NumberRange - CoordinateSystem.AbsoluteShift.OnX / 100 / CoordinateSystem.Zoom : MaximumX);
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

            while (Relation.Count > 1 && !double.IsNaN(y))
            {
                y = CalculateYValue(orderProgression);
                orderProgression++;
            }

            y = Math.Abs(y) > 1000000 ? LimitY() : y;

            return y;
        }

        //Výpočet hodnoty y.
        private double CalculateYValue(int orderProgression)
        {
            int index = CalculationOrder.Indexes[orderProgression];

            y = Operation(index);

            index = Relation[index].IsTrigonometricFunction() ? index + 1 : index;

            Relation[index] = y.ToString();

            Relation.RemoveNeighbors(index);

            y = double.Parse(Relation[index - Relation.RemovedElementsCount / 2]);

            CalculationOrder.ShiftPosition(Relation.RemovedElementsCount, orderProgression);

            Relation.RemovedElementsCount = 0;

            return y;
        }

        //Uložení bodu.
        private void SavePoint(double x, double y)
        {
            Point point = new Point(x, y);
            point = ConvertToCoordinatePoint(point);

            if (!double.IsNaN(y) && !double.IsInfinity(y) && Math.Abs(point.Y) < 5000) // Pokud bod není definovaný, vzniká mezera.
            {
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
            Polyline polyline = new Polyline()
            {
                Stroke = Brush,
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
                X = CoordinateSystem.Width / 2 + (point.X * CoordinateSystem.Zoom * 100) + CoordinateSystem.AbsoluteShift.OnX,
                Y = (-y * CoordinateSystem.Zoom * 100) + CoordinateSystem.Height / 2 + CoordinateSystem.AbsoluteShift.OnY
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
                case "√":
                    {
                        y = Root(double.Parse(Relation[index + 1]), 1 / double.Parse(Relation[index - 1]));
                        break;
                    }
                case "sin":
                    {
                        y = TrigFunc(double.Parse(Relation[index + 2]), Relation[index + 1], Math.Sin, Math.Asin);
                        break;
                    }
                case "cos":
                    {
                        y = TrigFunc(double.Parse(Relation[index + 2]), Relation[index + 1], Math.Cos, Math.Acos);
                        break;
                    }
                case "tg":
                    {
                        y = TrigFunc(double.Parse(Relation[index + 2]), Relation[index + 1], Math.Tan, Math.Atan);
                        break;
                    }
                case "cotg":
                    {
                        y = TrigFunc(double.Parse(Relation[index + 2]), Relation[index + 1], Cotangens, ArcusCotangens);
                        break;
                    }
            }

            return y;
        }

        //Odmocnění
        private double Root(double number, double index)
        {
            return (index > 0 && !double.IsInfinity(index)) ? Math.Pow(number, index) : double.NaN;
        }

        private double TrigFunc(double number, string argument, Func<double, double> func, Func<double, double> arcusFunc)
        {
            return argument == "⁻¹" ? arcusFunc(number) : func(number);
        }

        private double Cotangens(double number)
        {
            return Math.Cos(number) / Math.Sin(number);
        }

        private double ArcusCotangens(double number)
        {
            return Math.Atan(number) * -1;
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
            Relation = new Relation();
            Relation.AddRange(relationBackup);
            calculationOrderIndexesBackup.CopyTo(CalculationOrder.Indexes, 0);
        }
    }
}
