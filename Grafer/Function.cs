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
        public string Name { get; } = string.Empty;
        public Brush Brush { get; }
        public string InputRelation { get; }
        public bool IsLimited { get; }
        public double MinimumX { get; }
        public double MaximumX { get; }
        public bool Inverse { get; }
        public FunctionType Type { get; }
        public int ErrorMessageID { get; private set; } = -1;
        public int Number { get; } = 1;
        public enum FunctionType // Typy funkce na základě vhodné míry pro konkrétní osu.
        {
            Basic,
            TrigonometricFunction,
            InverseTrigonometricFunction
        }

        private Relation relation;

        private readonly CalculationOrder calculationOrder;

        private readonly List<Polyline> curves;

        private readonly CoordinateSystem coordinateSystem;

        private List<Point> points;
        private double y;
        private string[] relationBackup = Array.Empty<string>(); // Záloha předpisu.
        private int[] calculationOrderIndexesBackup = Array.Empty<int>(); // záloha výpočetního postupu.
        private double calculationMinimumX, calculationMaximumX; // Výpočetní minimum a maximum.

        public Function(string name, Brush color, string relation, bool isLimited, double minimumX, double maximumX, CoordinateSystem coordinateSystem, bool inverse)
        {
            Name = name;
            this.relation = new Relation(relation);
            MinimumX = minimumX;
            MaximumX = maximumX;
            IsLimited = isLimited;
            calculationOrder = new CalculationOrder();
            curves = new List<Polyline>();
            points = new List<Point>();
            this.coordinateSystem = coordinateSystem;
            Brush = color;
            Inverse = inverse;
            Type = GetFunctionType();
            Number = int.Parse(Name.Split(new char[] { 'f', ':' })[1]);
            InputRelation = relation;
        }

        //Zjištení typu funkce.
        private FunctionType GetFunctionType()
        {
            FunctionType type = relation.Contains("⁻¹") ? FunctionType.InverseTrigonometricFunction : FunctionType.Basic;

            if (type != FunctionType.InverseTrigonometricFunction)
            {
                for (int i = 0; i < relation.Count; i++)
                {
                    if (relation[i].IsTrigonometricFunction())
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

            bool? isGrowing = curves[0].Points[0].Y > curves[0].Points[1].Y ? true : curves[0].Points[0].Y < curves[0].Points[1].Y ? false : (bool?)null;

            if (isGrowing != null)
            {
                for (int i = 0; i < curves.Count && isInvertible; i++)
                {
                    for (int j = 1; j < curves[i].Points.Count && isInvertible; j++)
                    {
                        if (isGrowing == true)
                        {
                            isInvertible = curves[i].Points[j - 1].Y > curves[i].Points[j].Y;
                        }

                        if (isGrowing == false)
                        {
                            isInvertible = curves[i].Points[j - 1].Y < curves[i].Points[j].Y;
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

            y = (relation.Count > 1) ? CalculateYForX() : double.Parse(relation[0]);
        }

        //Příprava pro výpoočet.
        private void PrepareForCalculation()
        {
            calculationOrder.Create(relation);
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
                if (Inverse && (curves[0].Points.Count > 2 && !IsInvertible()))
                {
                    ErrorMessageID = 26;
                }
            }

            return ErrorMessageID == -1;
        }

        //Jestli není funkce prádzná.
        private bool IsEmpty()
        {
            if (curves[0].Points.Count == 0)
            {
                ErrorMessageID = 27;
            }

            return ErrorMessageID != -1;
        }

        //Vykreslení funkce do plátna.
        public void Plot(bool inverse, double opacity)
        {
            for (int i = 0; i < curves.Count; i++)
            {
                Polyline curve = GetDeepCurveCopy(curves[i]);
                curve.Opacity = opacity;

                if (inverse)
                {
                    coordinateSystem.Children.Add(InvertCurve(curve));
                }
                else
                {
                    coordinateSystem.Children.Add(curve);
                }
            }
        }

        //Vytvoří novou křivku v paměti nikoliv referenci.
        private Polyline GetDeepCurveCopy(Polyline inputCurve)
        {
            Polyline curve = NewCurve();

            for (int i = 0; i < inputCurve.Points.Count; i++)
            {
                curve.Points.Add(inputCurve.Points[i]);
            }

            return curve;
        }

        //Vytvoří základní křivku.
        private Polyline NewCurve()
        {
            return new Polyline()
            {
                Stroke = Brush,
                StrokeThickness = 2
            };
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
                X = coordinateSystem.Width / 2 + coordinateSystem.Height / 2 - point.Y + coordinateSystem.AbsoluteShift.OnY + coordinateSystem.AbsoluteShift.OnX,
                Y = coordinateSystem.Height / 2 + coordinateSystem.Width / 2 - point.X + coordinateSystem.AbsoluteShift.OnY + coordinateSystem.AbsoluteShift.OnX
            };
        }

        //Nastavení výpočetního rozsahu. Pokud by byl rozsah větší než plátno, omezí to jen na viditelnou plochu interně.
        private void SetCalculationXRange()
        {
            calculationMinimumX = ((-Math.Abs(MinimumX) > coordinateSystem.NumberRange + (coordinateSystem.AbsoluteShift.OnX / 100 / coordinateSystem.Zoom)) ? -coordinateSystem.NumberRange - coordinateSystem.AbsoluteShift.OnX / 100 / coordinateSystem.Zoom : MinimumX);
            calculationMaximumX = ((MaximumX > coordinateSystem.NumberRange - (coordinateSystem.AbsoluteShift.OnX / 100 / coordinateSystem.Zoom)) ? coordinateSystem.NumberRange - coordinateSystem.AbsoluteShift.OnX / 100 / coordinateSystem.Zoom : MaximumX);
        }

        //Dosazení za x.
        private void SubstituteX(double x)
        {
            for (int i = 0; i < relation.Count; i++)
            {
                relation[i] = relation[i] == "x" ? x.ToString() : relation[i];
                relation[i] = relation[i] == "-x" ? (-x).ToString() : relation[i];
            }
        }

        //Výpočet y pro x.
        private double CalculateYForX()
        {
            int orderProgression = 0;

            while (relation.Count > 1 && !double.IsNaN(y))
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
            int index = calculationOrder.Indexes[orderProgression];

            y = Operation(index);

            index = relation[index].IsTrigonometricFunctionOrLogarithm() ? index + 1 : index;

            relation[index] = y.ToString();

            relation.RemoveNeighbors(index);

            y = double.Parse(relation[index - relation.RemovedElementsCount / 2]);

            calculationOrder.ShiftPosition(relation.RemovedElementsCount, orderProgression);

            relation.RemovedElementsCount = 0;

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
            Polyline polyline = NewCurve();

            polyline.SnapsToDevicePixels = true;

            for (int i = 0; i < points.Count; i++)
            {
                polyline.Points.Add(points[i]);
            }

            curves.Add(polyline);
        }

        //Převedení bodu na bod do soustavy.
        private Point ConvertToCoordinatePoint(Point point)
        {
            point = new Point()
            {
                X = coordinateSystem.Width / 2 + (point.X * coordinateSystem.Zoom * 100) + coordinateSystem.AbsoluteShift.OnX,
                Y = (-y * coordinateSystem.Zoom * 100) + coordinateSystem.Height / 2 + coordinateSystem.AbsoluteShift.OnY
            };
            return point;
        }

        //Operace mezi 2 členy v předpisu.
        private double Operation(int index)
        {
            switch (relation[index])
            {
                case "+":
                    {
                        y = double.Parse(relation[index - 1]) + double.Parse(relation[index + 1]);
                        break;
                    }
                case "-":
                    {
                        y = double.Parse(relation[index - 1]) - double.Parse(relation[index + 1]);
                        break;
                    }
                case "*":
                    {
                        y = double.Parse(relation[index - 1]) * double.Parse(relation[index + 1]);
                        break;
                    }
                case "/":
                    {
                        y = double.Parse(relation[index - 1]) / double.Parse(relation[index + 1]);
                        break;
                    }
                case "^":
                    {
                        y = Math.Pow(double.Parse(relation[index - 1]), double.Parse(relation[index + 1]));
                        break;
                    }
                case "√":
                    {
                        y = Root(double.Parse(relation[index + 1]), 1 / double.Parse(relation[index - 1]));
                        break;
                    }
                case "sin":
                    {
                        y = TrigFunc(double.Parse(relation[index + 2]), relation[index + 1], Math.Sin, Math.Asin);
                        break;
                    }
                case "cos":
                    {
                        y = TrigFunc(double.Parse(relation[index + 2]), relation[index + 1], Math.Cos, Math.Acos);
                        break;
                    }
                case "tg":
                    {
                        y = TrigFunc(double.Parse(relation[index + 2]), relation[index + 1], Math.Tan, Math.Atan);
                        break;
                    }
                case "cotg":
                    {
                        y = TrigFunc(double.Parse(relation[index + 2]), relation[index + 1], Cotangens, ArcusCotangens);
                        break;
                    }
                case "log":
                    {
                        y = Math.Log(double.Parse(relation[index + 2]), double.Parse(relation[index + 1]));
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
            relationBackup = relation.ToArray();
            calculationOrderIndexesBackup = new int[calculationOrder.Indexes.Length];
            calculationOrder.Indexes.CopyTo(calculationOrderIndexesBackup, 0);
        }

        //Načtení zálohy.
        private void GetBackup()
        {
            relation = new Relation();
            relation.AddRange(relationBackup);
            calculationOrderIndexesBackup.CopyTo(calculationOrder.Indexes, 0);
        }

        //Převod hodnot vlastností do pole.
        public string[] PropertiesValueToArray()
        {
            var properties = typeof(Function).GetProperties();

            string[] propertiesValue = new string[properties.Length];

            for (int i = 0; i < properties.Length; i++)
            {
                propertiesValue[i] = (properties[i].GetValue(this)!.ToString()!);
            }

            return propertiesValue;
        }
    }
}
